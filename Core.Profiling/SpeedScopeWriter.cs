using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Profiling
{
    internal static class SpeedScopeWriter
    {
        
        public static void WriteToFile(ProfilerFrame source, string filePath) {
	        var file = new FileInfo(filePath);
	        var dir = file.Directory;
	        if (!dir.Exists) {
				dir.Create();
	        }
	       
            if (file.Exists)
                file.Delete();
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            using var writeStream = new StreamWriter(fs, Encoding.UTF8);
            Export(source, writeStream, Path.GetFileNameWithoutExtension(filePath));
        }

        #region private
        private static void Export(ProfilerFrame source, TextWriter writer, string name)
        {
            var methodNameToId = new Dictionary<string, int>();
            var eventsPerThread = new Dictionary<string, IList<ProfileEvent>>();

			OutputProfilerFrame(source, methodNameToId, eventsPerThread);

            var orderedFrameNames = methodNameToId.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToArray();

            WriteToFile(eventsPerThread, orderedFrameNames, writer, name);
        }

        private static void OutputProfilerFrame(ProfilerFrame source,
	        Dictionary<string, int> exportedFrameNameToExportedFrameId,
	        Dictionary<string, IList<ProfileEvent>> profileEventsPerThread) {

	        int frameId = 0;
	        foreach (var sourceThreadMethod in source.threadMethods) {
				var list = new List<ProfileEvent>(sourceThreadMethod.Item2.Count);
				
		        foreach (ProfilerMethod method in sourceThreadMethod.Item2) {
			        if (!exportedFrameNameToExportedFrameId.TryGetValue(method.methodName, out int value)) {
				        value = frameId;
				        frameId++;
						exportedFrameNameToExportedFrameId.Add(method.methodName, value);
			        }

					list.Add(new ProfileEvent(ProfileEventType.Open, value, method.startTime, method.depth));
					list.Add(new ProfileEvent(ProfileEventType.Close, value, method.endTime, method.depth));
		        }

		        profileEventsPerThread[sourceThreadMethod.Item1] = OrderForExport(list).ToList();
	        }
        }

        private static IEnumerable<ProfileEvent> OrderForExport(IEnumerable<ProfileEvent> profiles)
        {
            return profiles
                .GroupBy(@event => @event.RelativeTime)
                .OrderBy(group => group.Key)
                .SelectMany(group =>
                {
                    var closingDescendingByDepth = group.Where(@event => @event.Type == ProfileEventType.Close).OrderByDescending(@event => @event.Depth);
					var openingAscendingByDepth = group.Where(@event => @event.Type == ProfileEventType.Open).OrderBy(@event => @event.Depth);

                    return closingDescendingByDepth.Concat(openingAscendingByDepth);
                });
        }

        
        private static void WriteToFile(IReadOnlyDictionary<string, IList<ProfileEvent>> profilerEventsPerThread, 
            IReadOnlyList<string> orderedFrameNames, TextWriter writer, string name)
        {
            writer.Write("{");
            writer.Write("\"exporter\": \"speedscope@1.3.2\", ");
            writer.Write($"\"name\": \"{name}\", ");
            writer.Write("\"activeProfileIndex\": 0, ");
            writer.Write("\"$schema\": \"https://www.speedscope.app/file-format-schema.json\", ");

            writer.Write("\"shared\": { \"frames\": [ ");
            for (int i = 0; i < orderedFrameNames.Count; i++)
            {
                writer.Write($"{{ \"name\": \"{orderedFrameNames[i].Replace("\\", "\\\\").Replace("\"", "\\\"")}\" }}");

                if (i != orderedFrameNames.Count - 1)
                    writer.Write(", ");
            }
            writer.Write("] }, ");

            writer.Write("\"profiles\": [ ");

            bool isFirst = true;
            foreach (var threadEventList in profilerEventsPerThread.OrderBy(pair => pair.Value.First().RelativeTime))
            {
                if (!isFirst)
                    writer.Write(", ");
                else
                    isFirst = false;

                var sortedProfileEvents = threadEventList.Value;

                writer.Write("{ ");
                    writer.Write("\"type\": \"evented\", ");
                    writer.Write($"\"name\": \"{threadEventList.Key}\", ");
                    writer.Write("\"unit\": \"microseconds\", ");
                    writer.Write($"\"startValue\": \"{sortedProfileEvents.First().RelativeTime.ToString("R", CultureInfo.InvariantCulture)}\", ");
                    writer.Write($"\"endValue\": \"{sortedProfileEvents.Last().RelativeTime.ToString("R", CultureInfo.InvariantCulture)}\", ");
                    writer.Write("\"events\": [ ");
                    for (int i = 0; i < sortedProfileEvents.Count; i++)
                    {
                        var frameEvent = sortedProfileEvents[i];

                        writer.Write($"{{ \"type\": \"{(frameEvent.Type == ProfileEventType.Open ? "O" : "C")}\", ");
                        writer.Write($"\"frame\": {frameEvent.FrameId.ToString()}, ");
                        writer.Write($"\"at\": {frameEvent.RelativeTime.ToString("R", CultureInfo.InvariantCulture)} }}");

                        if (i != sortedProfileEvents.Count - 1)
                            writer.Write(", ");
                    }
                    writer.Write("]");
                writer.Write("}");
            }

            writer.Write("] }");
        }

        private enum ProfileEventType : byte
        {
            Open = 0, Close = 1
        }

        private struct ProfileEvent
        {
            public ProfileEvent(ProfileEventType type, int frameId, double relativeTime, int depth)
            {
                Type = type;
                FrameId = frameId;
                RelativeTime = relativeTime;
                Depth = depth;
            }

            public override string ToString() => $"{RelativeTime.ToString(CultureInfo.InvariantCulture)} {Type} {FrameId}";

            #region private
            internal ProfileEventType Type { get; }
            internal int FrameId { get; }
            internal double RelativeTime { get; }
            internal int Depth { get; }
            #endregion private
        }
        #endregion private
    }
}
