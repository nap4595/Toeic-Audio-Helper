using System.IO;

namespace ToeicAudioHelper
{
    public class VocaTrack
    {
        public int Index { get; }
        public string FilePath { get; }
        public long SizeBytes { get; }

        public VocaTrack(int index, string filePath)
        {
            Index = index <= 0 ? 1 : index;
            FilePath = filePath;
            try { SizeBytes = new FileInfo(filePath).Length; } catch { SizeBytes = 0; }
        }
        public override string ToString() => $"{Index}: {Path.GetFileName(FilePath)}";
    }
}

