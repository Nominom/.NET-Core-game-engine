using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Core.Shared;

namespace Core.Profiling {
	public static class ChromeTracingWriter {
		public static void WriteToFile(ProfilerFrame source, string filePath) {
			FileInfo file = new FileInfo(filePath);
			DirectoryInfo dir = file.Directory;
			if (!dir.Exists) {
				dir.Create();
			}

			if (file.Exists) {
				file.Delete();
			}

			using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
			using StreamWriter writeStream = new StreamWriter(fs, Encoding.UTF8);
			Export(source, writeStream, Path.GetFileNameWithoutExtension(filePath));
		}

		private static void Export(ProfilerFrame source, TextWriter writer, string name) {
			var events = GetEvents(source);
			WriteToFile(events, writer, name);
		}

		private static IList<TraceEvent> GetEvents(ProfilerFrame frame) {
			var events = new List<TraceEvent>();
			int processId = Process.GetCurrentProcess().Id;
			int threadId = 0;
			string categories = "PERF";
			foreach (var frameThreadMethod in frame.threadMethods.OrderByDescending(x => x.Item1)) {
				foreach (ProfilerMethod profilerMethod in frameThreadMethod.Item2) {
					//Chrome tracing shows methods with same startTime as parallel, so we add depth to the startTime
					long startTime = (long) profilerMethod.startTime + profilerMethod.depth;
					long endTime = (long) profilerMethod.endTime < startTime
						? startTime
						: (long) profilerMethod.endTime;
					events.Add(new TraceEvent() {
						name = profilerMethod.methodName,
						cat = categories,
						pid = processId,
						ph = EventType.B,
						tid = frameThreadMethod.Item1,
						ts = startTime
					});
					events.Add(new TraceEvent() {
						name = profilerMethod.methodName,
						cat = categories,
						pid = processId,
						ph = EventType.E,
						tid = frameThreadMethod.Item1,
						ts = endTime
					});
				}

				threadId++;
			}

			if (frame.markers != null) {
				foreach (ProfilerMarker marker in frame.markers) {
					events.Add(new TraceEvent() {
						name = marker.markName,
						cat = "EVENTS",
						pid = processId,
						ph = EventType.I,
						tid = "Events",
						ts = (long) (marker.time)
					});
				}
			}

			return events;
		}


		private static void WriteToFile(IList<TraceEvent> events, TextWriter writer, string name) {
			writer.Write("[\n");
			bool first = true;
			foreach (TraceEvent traceEvent in events) {
				if (!first) {
					writer.Write(", \n");
				}
				else {
					first = false;
				}

				writer.Write("{");
				writer.Write($"\"name\": \"{traceEvent.name}\", ");
				writer.Write($"\"cat\": \"{traceEvent.cat}\", ");
				writer.Write($"\"ph\": \"{traceEvent.ph}\", ");
				writer.Write($"\"ts\": \"{traceEvent.ts}\", ");
				writer.Write($"\"pid\": \"{traceEvent.pid}\", ");
				writer.Write($"\"tid\": \"{traceEvent.tid}\"");

				writer.Write("}");
			}

			writer.Write("\n]");
		}


		private enum EventType {
			B, //begin
			E, //end
			I //instant
		}

		private struct TraceEvent {
			public string name; //Name of function
			public string cat; //Comma separated list of categories
			public EventType ph; //Either B or E (Begin or End)
			public long ts; //Time stamp in microseconds
			public int pid; //Process id
			public string tid; //Thread id
		}
	}
}