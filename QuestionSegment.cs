using System;

namespace ToeicAudioHelper
{
    public class QuestionSegment
    {
        public int Start { get; }
        public int End { get; }
        public string FilePath { get; }

        public QuestionSegment(int start, int end, string filePath)
        {
            Start = start;
            End = Math.Max(start, end);
            FilePath = filePath;
        }

        public bool Contains(int q) => q >= Start && q <= End;
        public override string ToString() => $"{Start}-{End}: {System.IO.Path.GetFileName(FilePath)}";
    }
}

