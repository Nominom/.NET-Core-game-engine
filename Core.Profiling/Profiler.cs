using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Core.Shared;

namespace Core.Profiling
{
	public static class Profiler {
		private struct MethodProfilingData {
			public string methodName;
			public double startTime;
			public int depth;
#if PROFILER_DEBUG
			public StackTrace startTrace;
#endif
		}

		public static double CollectionThresholdMicroseconds { get; set; } = 1;
		public static bool ProfilingEnabled { get; set; } = false;

		private static bool capturingFrame = false;
		private static ThreadLocal<Stack<MethodProfilingData>> methodStarts = new ThreadLocal<Stack<MethodProfilingData>>(() => new Stack<MethodProfilingData>(), true);
		private static ThreadLocal<List<ProfilerMethod>> collectedMethods = new ThreadLocal<List<ProfilerMethod>>(() => new List<ProfilerMethod>(), true);
		private static ThreadLocal<List<ProfilerMarker>> collectedMarkers = new ThreadLocal<List<ProfilerMarker>>(() => new List<ProfilerMarker>(), true);
		private static ConcurrentDictionary<int, string> profilingThreads = new ConcurrentDictionary<int, string>();
		private static Stopwatch stopwatch = new Stopwatch();
		private static long currentFrame = 0;
		private static CircularBuffer<ProfilerFrame> collectedFrames = new CircularBuffer<ProfilerFrame>(256);

		private static WeakReference<GCMonitor> gcMon = GCMonitor.CurrentInstance;

		public static void RegisterThread(string threadName) {
			Thread thread = Thread.CurrentThread;
			profilingThreads.TryAdd(thread.ManagedThreadId, threadName);
		}

		public static void RegisterThread(Thread thread, string threadName) {
			profilingThreads.TryAdd(thread.ManagedThreadId, threadName);
		}

		public static void StartFrame(long frameNumber) {
			if (!ProfilingEnabled) return;
			if (capturingFrame) {
				throw new InvalidOperationException("Cannot Start a new frame without calling EndFrame first.");
			}
			currentFrame = frameNumber;
			capturingFrame = true;
			stopwatch.Restart();
			StartMethod("Frame - " + frameNumber.ToString());
		}

		public static void EndFrame() {
			if (!ProfilingEnabled) return;
			if (!capturingFrame) {
				return;
			}
			capturingFrame = false;
			stopwatch.Stop();
			EndMethod();
			CollectFrame();
			Reset();
		}

		public static void StartMethod(string methodName) {
			if (!ProfilingEnabled) return;
			if (!capturingFrame) return;
			var methodStart = methodStarts.Value;
			var data = new MethodProfilingData() {
				depth = methodStart.Count > 0 ? methodStart.Peek().depth + 1 : 0,
				methodName = methodName,
				startTime = (stopwatch.ElapsedTicks / (double) Stopwatch.Frequency) * 1000000.0
			};
#if PROFILER_DEBUG
			data.startTrace = new StackTrace(1);
#endif
			methodStart.Push(data);
		}

		public static void EndMethod()
		{
			if (!ProfilingEnabled) return;
			if (!capturingFrame) return;
			var methodStart = methodStarts.Value;
			double endTime =  (stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000.0;
			if (methodStart.TryPop(out var data)) {
#if PROFILER_DEBUG
				var endTrace = new StackTrace(1);

				if (data.startTrace.GetFrame(0).GetMethod() != endTrace.GetFrame(0).GetMethod()) {
					Console.WriteLine($"A profiler method was started in {data.startTime.ToString()} but ended in {endTrace.ToString()}. Is this an error?");
				}
#endif

				if (endTime - data.startTime > CollectionThresholdMicroseconds) {
					collectedMethods.Value.Add( new ProfilerMethod(
						data.methodName, data.depth, data.startTime, endTime,
						Thread.CurrentThread.ManagedThreadId));
				}
			}
			else {
				Console.WriteLine("Cannot end method. Reached end of stack.");
			}

		}

		public static void AddMarker(string markerName) {
			if (!ProfilingEnabled) return;
			if (!capturingFrame) return;
			double time =  (stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000.0;
			collectedMarkers.Value.Add(new ProfilerMarker(markerName, time));
		}

		internal static void RegisterGC(int generation) {
			if (!ProfilingEnabled) return;
			if (!capturingFrame) return;
			AddMarker($"GarbageCollection Gen{generation.ToString()}");
		}

		public static void WriteToFile(string filename, ProfilingFormat format, FrameSelection frameSelection = FrameSelection.Median) {
			if (!ProfilingEnabled) {
				throw new InvalidOperationException("Cannot write profile to file when profiling is not enabled.");
			}
			ProfilerFrame frame = default;

			if (collectedFrames.IsEmpty) {
				return;
			}

			bool needsDispose = false;

			switch (frameSelection) {
				case FrameSelection.Median:
					var ordered = collectedFrames.OrderBy(x => x.frameLength).ToArray();
					frame = ordered[(int)Math.Floor(ordered.Length / 2.0)];
					break;
				case FrameSelection.Longest:
					frame = collectedFrames.MaxBy(x => x.frameLength);
					break;
				case FrameSelection.Shortest:
					frame = collectedFrames.MinBy(x => x.frameLength);
					break;
				case FrameSelection.Latest:
					frame = collectedFrames.Back();
					break;
				case FrameSelection.Percentile95:
					ordered = collectedFrames.OrderBy(x => x.frameLength).ToArray();
					int index = ordered.Length - (int) (ordered.Length * 0.05f);
					if (index >= ordered.Length) {
						index = ordered.Length - 1;
					}
					frame = ordered[index];
					break;
				case FrameSelection.All:
					frame = new ProfilerFrame();
					frame.frameNumber = collectedFrames.Min(x => x.frameNumber);
					frame.frameLength = collectedFrames.Sum(x => x.frameLength);
					frame.markers = PooledList<ProfilerMarker>.Create();
					double elapsedTime = 0;
					foreach (var collectedFrame in collectedFrames) {
						if(collectedFrame.markers != null) {
							frame.markers.AddRange(collectedFrame.markers.Select(x =>
								new ProfilerMarker(x.markName, x.time + elapsedTime)));
						}
						elapsedTime += collectedFrame.frameLength;
					}
					frame.threadMethods = PooledList<(string, PooledList<ProfilerMethod>)>.Create();
					elapsedTime = 0;
					foreach (var collectedFrame in collectedFrames) {
						foreach (var collectedFrameThreadMethod in collectedFrame.threadMethods) {
							var tuple = frame.threadMethods.SingleOrDefault(x => x.Item1 == collectedFrameThreadMethod.Item1);
							PooledList<ProfilerMethod> outList = null;
							if (tuple.Item1 == null) {
								outList = PooledList<ProfilerMethod>.Create();
								frame.threadMethods.Add((collectedFrameThreadMethod.Item1, outList));
							}
							else {
								outList = tuple.Item2;
							}
							outList.AddRange(collectedFrameThreadMethod.Item2.Select(x => new ProfilerMethod(
								x.methodName,x.depth, x.startTime + elapsedTime, x.endTime + elapsedTime, x.threadId)));
						}
						elapsedTime += collectedFrame.frameLength;
					}

					needsDispose = true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(frameSelection), frameSelection, null);
			}
			
			switch (format) {
				case ProfilingFormat.SpeedScope:
					SpeedScopeWriter.WriteToFile(frame, filename);
					break;
				case ProfilingFormat.ChromeTracing:
					ChromeTracingWriter.WriteToFile(frame, filename);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}

			if (needsDispose) {
				frame.Dispose();
			}
		}


		private static void CollectFrame() {
			ProfilerFrame frame = new ProfilerFrame();
			frame.frameNumber = currentFrame;
			frame.frameLength = (stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000.0;
			frame.threadMethods = PooledList<(string, PooledList<ProfilerMethod>)>.Create();

			foreach (var dataList in collectedMethods.Values) {
				if (dataList.Count == 0) continue;
				if (profilingThreads.TryGetValue(dataList[0].threadId, out string threadName)) {
					var tuple = frame.threadMethods.SingleOrDefault(x => x.Item1 == threadName);
					PooledList<ProfilerMethod> outList = null;
					if (tuple.Item1 == null) {
						outList = PooledList<ProfilerMethod>.Create();
						frame.threadMethods.Add((threadName, outList));
					}
					else {
						outList = tuple.Item2;
					}

					outList.AddRange(dataList);
				}
			}

			foreach (var dataList in collectedMarkers.Values) {
				if (frame.markers == null) frame.markers = PooledList<ProfilerMarker>.Create();
				frame.markers.AddRange(dataList);
			}

			if (collectedFrames.IsFull) {
				collectedFrames.Front().Dispose();
			}
			collectedFrames.PushBack(frame);
		}

		private static void Reset() {
			foreach (var data in methodStarts.Values) {
				data.Clear();
			}
			foreach (var data in collectedMethods.Values) {
				data.Clear();
			}
			foreach (var data in collectedMarkers.Values) {
				data.Clear();
			}
		}
	}
}
