﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using BepuUtilities;
using BepuUtilities.Memory;
using Core.ECS.JobSystem;
using Core.Profiling;

namespace Core.Physics
{

	//public class JobThreadDispatcher : IThreadDispatcher, IDisposable
	//{

	//	BufferPool[] bufferPools;
	//	public int ThreadCount { get; private set; }

	//	public JobThreadDispatcher(int numJobs) {
	//		this.ThreadCount = numJobs;
	//		bufferPools = new BufferPool[numJobs];
	//		for (int i = 0; i < bufferPools.Length; ++i)
	//		{
	//			bufferPools[i] = new BufferPool();
	//		}
	//	}

	//	public struct PhysicsJob : IJob {
	//		public int workerIndex;
	//		public Action<int> action;

	//		public void Execute() {
	//			action(workerIndex);
	//		}
	//	}

	//	public void DispatchWorkers(Action<int> workerBody) {
	//		Jobs.CompleteAllJobs();
	//		var group = Jobs.StartNewGroup();
	//		for(int i = 0; i < ThreadCount; i++) {
	//			PhysicsJob job = new PhysicsJob() {
	//				action = workerBody,
	//				workerIndex = i
	//			};
	//			job.Schedule(group);
	//		}
	//		Jobs.CompleteAllJobs();
	//	}

	//	public BufferPool GetThreadMemoryPool(int workerIndex) {
	//		return bufferPools[workerIndex];
	//	}

	//	public void Dispose() {
	//		for (int i = 0; i < ThreadCount; i++) {
	//			bufferPools?[i]?.Clear();
	//		}
	//	}
	//}


	public class SimpleThreadDispatcher : IThreadDispatcher, IDisposable
    {
        int threadCount;
        public int ThreadCount => threadCount;
        struct Worker
        {
            public Thread Thread;
            public AutoResetEvent Signal;
        }

        Worker[] workers;
        AutoResetEvent finished;

        BufferPool[] bufferPools;

        public SimpleThreadDispatcher(int threadCount)
        {
            this.threadCount = threadCount;
            workers = new Worker[threadCount - 1];
            for (int i = 0; i < workers.Length; ++i)
            {
                workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
                workers[i].Thread.IsBackground = true;
                workers[i].Thread.Name = "Physics thread" + i;
                workers[i].Thread.Start(workers[i].Signal);
				Profiler.RegisterThread(workers[i].Thread, "PhysicsWorker" + i);
            }
            finished = new AutoResetEvent(false);
            bufferPools = new BufferPool[threadCount];
            for (int i = 0; i < bufferPools.Length; ++i)
            {
                bufferPools[i] = new BufferPool();
            }
        }

        void DispatchThread(int workerIndex)
        {
            Debug.Assert(workerBody != null);
			Profiler.StartMethod("PhysicsJob");
			Profiler.StartMethod(workerBodyMethodName);
			workerBody(workerIndex);
			Profiler.EndMethod();
			Profiler.EndMethod();

            if (Interlocked.Increment(ref completedWorkerCounter) == threadCount)
            {
                finished.Set();
            }
        }

        volatile Action<int> workerBody;
        volatile string workerBodyMethodName;
        int workerIndex;
        int completedWorkerCounter;

        void WorkerLoop(object untypedSignal)
        {
            var signal = (AutoResetEvent)untypedSignal;
            while (true)
            {
                signal.WaitOne();
                if (disposed)
                    return;
                DispatchThread(Interlocked.Increment(ref workerIndex) - 1);
            }
        }

        void SignalThreads()
        {
            for (int i = 0; i < workers.Length; ++i)
            {
                workers[i].Signal.Set();
            }
        }

        public void DispatchWorkers(Action<int> workerBody)
        {
            Debug.Assert(this.workerBody == null);
            workerIndex = 1; //Just make the inline thread worker 0. While the other threads might start executing first, the user should never rely on the dispatch order.
            completedWorkerCounter = 0;
            this.workerBody = workerBody;
            this.workerBodyMethodName = workerBody.Method.Name;
            SignalThreads();
            //Calling thread does work. No reason to spin up another worker and block this one!
            DispatchThread(0);
            finished.WaitOne();
            this.workerBody = null;
        }

        volatile bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                SignalThreads();
                for (int i = 0; i < bufferPools.Length; ++i)
                {
                    bufferPools[i].Clear();
                }
                foreach (var worker in workers)
                {
                    worker.Thread.Join();
                    worker.Signal.Dispose();
                }
            }
        }

        public BufferPool GetThreadMemoryPool(int workerIndex)
        {
            return bufferPools[workerIndex];
        }
    }
}
