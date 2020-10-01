using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Core.AssetSystem
{
	public enum LoadPriority
	{
		Medium,
		Low,
		High
	}

	internal static class AssetLoader
	{
		internal struct LoadJob
		{
			public int assetId;
			public string assetName;
			public LoadPriority priority;
			public IAsset asset;
		}

		private static BlockingCollection<LoadJob> highPriorityQueue = new BlockingCollection<LoadJob>(new ConcurrentQueue<LoadJob>());
		private static BlockingCollection<LoadJob> mediumPriorityQueue = new BlockingCollection<LoadJob>(new ConcurrentQueue<LoadJob>());
		private static BlockingCollection<LoadJob> lowPriorityQueue = new BlockingCollection<LoadJob>(new ConcurrentQueue<LoadJob>());

		private static BlockingCollection<LoadJob>[] queues = new[]
			{highPriorityQueue, mediumPriorityQueue, lowPriorityQueue};

		private static bool isSetup = false;
		private static readonly object SetupLock = new object();
		private static AssetLoaderThread[] normalLoaderThreads;
		private static AssetLoaderThread highPriorityLoaderThread;
		private static int numNormalLoaderThreads = 2;

		private static void Setup()
		{
			lock (SetupLock) //lock while setup incase multiple threads try to access setup
			{
				if (isSetup) return;

				normalLoaderThreads = new AssetLoaderThread[numNormalLoaderThreads];
				for (int i = 0; i < numNormalLoaderThreads; i++)
				{
					var loaderThread = new AssetLoaderThread(false);
					var t = new Thread(loaderThread.Work);
					loaderThread.thread = t;

					t.Name = "AssetLoaderThread";
					t.IsBackground = true;
					t.Priority = ThreadPriority.BelowNormal;
					t.Start();

					normalLoaderThreads[i] = loaderThread;
				}

				highPriorityLoaderThread = new AssetLoaderThread(true);
				var ht = new Thread(highPriorityLoaderThread.Work);
				highPriorityLoaderThread.thread = ht;

				ht.Name = "AssetLoaderThread_HighPriority";
				ht.IsBackground = true;
				ht.Priority = ThreadPriority.Normal;
				ht.Start();

				isSetup = true;
			}
		}
		internal static void QueueAssetLoad<T>(AssetReference<T> asset, LoadPriority priority) where T : class, IAsset
		{
			AssetManager.SetState(asset.assetIndex, AssetState.Loading);

			if (!isSetup)
			{
				Setup();
			}
			LoadJob job = new LoadJob()
			{
				asset = asset.Get(),
				assetId = asset.assetIndex,
				assetName = asset.GetAssetName(),
				priority = priority
			};
			switch (priority)
			{
				case LoadPriority.Medium:
					mediumPriorityQueue.Add(job);
					break;
				case LoadPriority.Low:
					lowPriorityQueue.Add(job);
					break;
				case LoadPriority.High:
					highPriorityQueue.Add(job);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
			}
		}

		internal static void WaitForAssetToLoad(int assetId) {
			while (AssetManager.GetState(assetId) == AssetState.Loading) {
				Thread.Sleep(10);
			}
		}

		private class AssetLoaderThread
		{
			public volatile bool loading = false;
			public Thread thread;

			private readonly bool isHighPriorityOnly;

			public AssetLoaderThread(bool isHighPriorityOnly)
			{
				this.isHighPriorityOnly = isHighPriorityOnly;
			}

			private void CompleteJob(LoadJob job)
			{
				try
				{
					loading = true;
					var assetPackage = AssetManager.GetAssetPackageForAsset(job.assetName);
					using Stream s = assetPackage.GetUncompressedFileStream(job.assetName);
					IAsset asset = job.asset;
					asset.LoadFromStream(s);
					AssetManager.SetState(job.assetId, AssetState.Loaded);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					AssetManager.SetState(job.assetId, AssetState.FailedToLoad);
				}
				finally
				{
					loading = false;
				}
			}

			public void Work()
			{
				while (true)
				{
					try
					{
						if (isHighPriorityOnly)
						{
							var job = highPriorityQueue.Take();
							CompleteJob(job);
						}
						else
						{
							if (highPriorityQueue.TryTake(out var hj))
							{
								CompleteJob(hj);
							}
							else if (mediumPriorityQueue.TryTake(out var mj))
							{
								CompleteJob(mj);
							}
							else if (lowPriorityQueue.TryTake(out var lj))
							{
								CompleteJob(lj);
							}
							else
							{
								BlockingCollection<LoadJob>.TakeFromAny(queues, out var job);
								CompleteJob(job);
							}
						}

					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
			}
		}
	}
}
