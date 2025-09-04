using System.Collections.Generic;

namespace ToeicAudioHelper
{
    public class AudioCatalog
    {
        public Dictionary<int, string> OverallByTest { get; } = new();
        public Dictionary<int, List<QuestionSegment>> QuestionsByTest { get; } = new();
        public Dictionary<int, List<VocaTrack>> VocaLcByTest { get; } = new();
        public Dictionary<int, List<VocaTrack>> VocaRcByTest { get; } = new();

        public void Clear()
        {
            OverallByTest.Clear();
            QuestionsByTest.Clear();
            VocaLcByTest.Clear();
            VocaRcByTest.Clear();
        }
    }
}
