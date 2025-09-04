using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ToeicAudioHelper
{
    public static class FileScanner
    {
        // Overall: non-digit right after hyphen (avoid locale-specific literals like "전체")
        private static readonly Regex OverallRegex = new(@"Test[_\s-]?(\d{2})-(?!\d).+\.mp3", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex QuestionRegex = new(@"Test[_\s-]?(\d{2})-(\d{1,3})(?:-(\d{1,3}))?\.mp3", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex TestTokenRegex = new(@"Test[_\s-]?(0?[1-9]|10)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex BareTwoDigitTestRegex = new(@"\b(0?[1-9]|10)\b", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex VocaRcFileRegex = new(@"^Test[_\s-]?(0?[1-9]|10)[_\s-]*RC[_\s-]*Voca\.mp3$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex VocaLcFileRegex = new(@"^Test[_\s-]?(0?[1-9]|10)[_\s-]*LC[_\s-]*Voca\.mp3$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static AudioCatalog Scan(string root)
        {
            var catalog = new AudioCatalog();
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                return catalog;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(root, "*.mp3", SearchOption.AllDirectories);
            }
            catch
            {
                return catalog;
            }

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var dir = Path.GetDirectoryName(file) ?? string.Empty;

                // Questions first (explicit numeric patterns)
                var mQ = QuestionRegex.Match(name);
                if (mQ.Success)
                {
                    var t = int.Parse(mQ.Groups[1].Value);
                    int start = int.Parse(mQ.Groups[2].Value);
                    int end = start;
                    if (mQ.Groups[3].Success && int.TryParse(mQ.Groups[3].Value, out var e))
                        end = e;
                    if (!catalog.QuestionsByTest.TryGetValue(t, out var list))
                    {
                        list = new List<QuestionSegment>();
                        catalog.QuestionsByTest[t] = list;
                    }
                    list.Add(new QuestionSegment(start, end, file));
                    continue;
                }

                // Overall per test: TestXX-nonDigit.mp3
                var mOverall = OverallRegex.Match(name);
                if (mOverall.Success)
                {
                    var t = int.Parse(mOverall.Groups[1].Value);
                    catalog.OverallByTest[t] = file;
                    continue;
                }

                // VOCA LC/RC exact filename patterns first
                var mRc = VocaRcFileRegex.Match(name);
                var mLc = VocaLcFileRegex.Match(name);
                if (mRc.Success || mLc.Success)
                {
                    int t = int.Parse((mRc.Success ? mRc : mLc).Groups[1].Value);
                    var idx = ExtractVocaIndex(name, t);
                    var vt = new VocaTrack(idx, file);
                    if (mRc.Success)
                    {
                        if (!catalog.VocaRcByTest.TryGetValue(t, out var list)) { list = new List<VocaTrack>(); catalog.VocaRcByTest[t] = list; }
                        list.Add(vt);
                    }
                    if (mLc.Success)
                    {
                        if (!catalog.VocaLcByTest.TryGetValue(t, out var list)) { list = new List<VocaTrack>(); catalog.VocaLcByTest[t] = list; }
                        list.Add(vt);
                    }
                    continue;
                }

                // VOCA LC/RC by folder and filename heuristics (fallback)
                var up = (dir + "\\" + name).ToUpperInvariant();
                bool hasVoca = up.Contains("VOCA");
                bool isLc = hasVoca && up.Contains("LC");
                bool isRc = hasVoca && up.Contains("RC");
                if (hasVoca)
                {
                    var tOpt = ExtractTestFromPath(file);
                    if (tOpt.HasValue)
                    {
                        int t = tOpt.Value;
                        var idx = ExtractVocaIndex(name, t);
                        var vt = new VocaTrack(idx, file);
                        if (isLc)
                        {
                            if (!catalog.VocaLcByTest.TryGetValue(t, out var list)) { list = new List<VocaTrack>(); catalog.VocaLcByTest[t] = list; }
                            list.Add(vt);
                        }
                        if (isRc)
                        {
                            if (!catalog.VocaRcByTest.TryGetValue(t, out var list)) { list = new List<VocaTrack>(); catalog.VocaRcByTest[t] = list; }
                            list.Add(vt);
                        }
                    }
                    continue;
                }
            }

            // Normalize: sort question segments by start
            foreach (var kv in catalog.QuestionsByTest.ToList())
            {
                kv.Value.Sort((a, b) => a.Start.CompareTo(b.Start));
            }

            // Sort VOCA tracks by index asc, then size desc
            foreach (var kv in catalog.VocaLcByTest.ToList())
                kv.Value.Sort((a, b) => a.Index != b.Index ? a.Index.CompareTo(b.Index) : b.SizeBytes.CompareTo(a.SizeBytes));
            foreach (var kv in catalog.VocaRcByTest.ToList())
                kv.Value.Sort((a, b) => a.Index != b.Index ? a.Index.CompareTo(b.Index) : b.SizeBytes.CompareTo(a.SizeBytes));

            return catalog;
        }

        private static int? ExtractTestFromPath(string fullPath)
        {
            try
            {
                var fileName = Path.GetFileName(fullPath);
                var directories = (Path.GetDirectoryName(fullPath) ?? string.Empty)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                // 1) Look for TestXX in file name
                var m = TestTokenRegex.Match(fileName);
                if (m.Success && int.TryParse(m.Groups[1].Value, out var t1)) return t1;

                // 2) Look for TestXX in any directory segment
                foreach (var seg in directories)
                {
                    var md = TestTokenRegex.Match(seg);
                    if (md.Success && int.TryParse(md.Groups[1].Value, out var td)) return td;
                }

                // 3) Look for standalone 01..10 in file name (word boundary)
                var m2 = BareTwoDigitTestRegex.Match(fileName);
                if (m2.Success && int.TryParse(m2.Value, out var t2)) return t2;

                // 4) Look for parent directory named like Test_01, Test-01, Test 01
                if (directories.Length > 0)
                {
                    var parent = directories.Last();
                    var mp = TestTokenRegex.Match(parent);
                    if (mp.Success && int.TryParse(mp.Groups[1].Value, out var tp)) return tp;
                }
            }
            catch { }
            return null;
        }

        private static int ExtractVocaIndex(string fileName, int test)
        {
            // Determine a stable index from the filename (without extension)
            var stem = Path.GetFileNameWithoutExtension(fileName);
            var nums = Regex.Matches(stem, "\\d+").Cast<Match>().Select(m => m.Value).ToList();
            if (nums.Count >= 2)
            {
                // Often includes the test number and a trailing index; use the last number
                if (int.TryParse(nums.Last(), out var last)) return last;
            }
            else if (nums.Count == 1)
            {
                // If only one number appears and equals the test number, default to 1
                if (int.TryParse(nums[0], out var n))
                {
                    if (n != test) return n;
                }
            }
            return 1;
        }
    }
}
