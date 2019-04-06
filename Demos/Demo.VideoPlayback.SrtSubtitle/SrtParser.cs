using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Demo.VideoPlayback.SrtSubtitle {
    internal static class SrtParser {

        static SrtParser() {
        }

        [NotNull, ItemNotNull]
        public static SrtEntry[] Parse([NotNull] string content) {
            var entries = new List<SrtEntry>();

            var lines = content.Split(LineSeparator, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; ++i) {
                lines[i] = lines[i].Trim(TrimChars);
            }

            var state = ParseState.EntryEnd;

            var seqNumber = 0;
            string currentText = null;
            TimeSpan start = TimeSpan.Zero, end = TimeSpan.Zero;

            void AddEntry(int sequenceNumber, in TimeSpan startTime, in TimeSpan endTime, string text) {
                var entry = new SrtEntry(sequenceNumber, startTime, endTime, text);

                entries.Add(entry);
            }

            foreach (var line in lines) {
                switch (state) {
                    case ParseState.EntryEnd: {
                        if (string.IsNullOrEmpty(line)) {
                            continue;
                        }

                        seqNumber = Convert.ToInt32(line);

                        state = ParseState.ParsedSeqNumber;

                        break;
                    }
                    case ParseState.ParsedSeqNumber: {
                        if (string.IsNullOrEmpty(line)) {
                            continue;
                        }

                        var codes = line.Split(TimeCodeSeparator, StringSplitOptions.RemoveEmptyEntries);

                        if (codes.Length != 2) {
                            throw new FormatException("Time code should have 2 parts");
                        }

                        start = ParseTimeSpan(codes[0]);
                        end = ParseTimeSpan(codes[1]);

                        state = ParseState.ParsedTimeCode;

                        break;
                    }
                    case ParseState.ParsedTimeCode: {
                        if (string.IsNullOrEmpty(line)) {
                            continue;
                        }

                        currentText = line;

                        state = ParseState.ReadingText;

                        break;
                    }
                    case ParseState.ReadingText: {
                        if (string.IsNullOrEmpty(line)) {
                            AddEntry(seqNumber, start, end, currentText);

                            state = ParseState.EntryEnd;
                        } else {
                            if (string.IsNullOrEmpty(currentText)) {
                                currentText = line;
                            } else {
                                currentText = currentText + Environment.NewLine + line;
                            }
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (state == ParseState.ReadingText) {
                // Finish and create a new entry
                AddEntry(seqNumber, start, end, currentText);
            } else if (state == ParseState.EntryEnd) {
                // All read
            } else {
                throw new FormatException("Something is wrong with the SRT file");
            }

            entries.Sort();

            return entries.ToArray();
        }

        private static TimeSpan ParseTimeSpan([NotNull] string str) {
            var seps = str.Split(TimeSpanSeparators);

            Debug.Assert(seps.Length == 4);

            var h = Convert.ToInt32(seps[0]);
            var m = Convert.ToInt32(seps[1]);
            var s = Convert.ToInt32(seps[2]);
            var ms = Convert.ToInt32(seps[3]);

            var ts = new TimeSpan(0, h, m, s, ms);

            return ts;
        }

        private enum ParseState {

            EntryEnd = 0,
            ParsedSeqNumber = 1,
            ParsedTimeCode = 2,
            ReadingText = 3

        }

        [NotNull]
        private static readonly char[] LineSeparator = { '\n' };

        [NotNull]
        private static readonly char[] TimeSpanSeparators = { ':', ',' };

        [NotNull]
        private static readonly char[] TrimChars = { '\r', ' ', '\t' };

        [NotNull, ItemNotNull]
        private static readonly string[] TimeCodeSeparator = { "-->" };

    }
}
