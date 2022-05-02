using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SpeenChroma
{
    public static class ChromaLogic
    {
        private static readonly List<ChromaBlender> ChromaBlenders = new List<ChromaBlender>();
        private static readonly Dictionary<string, Dictionary<NoteColorType, List<ChromaTrigger>>> CachedTriggers =
            new Dictionary<string, Dictionary<NoteColorType, List<ChromaTrigger>>>();

        public static float CurrentBeat { get; private set; } = 0f;
        public static int BlenderCount => ChromaBlenders.Count;
        
        public static ChromaMode Mode = ChromaMode.Reactive;

        public static float RainbowSpeed = Plugin.ModConfig.GetValueOrDefaultTo("Rainbow", "Speed", 1f);
        public static float TapReactiveStrength = Plugin.ModConfig.GetValueOrDefaultTo("Reactive", "TapStrength", 0.75f);
        public static float MatchReactiveStrength = Plugin.ModConfig.GetValueOrDefaultTo("Reactive", "MatchStrength", 0.6f);
        public static float BeatReactiveStrength = Plugin.ModConfig.GetValueOrDefaultTo("Reactive", "BeatStrength", 0.9f);
        public static float SpinReactiveStrength = Plugin.ModConfig.GetValueOrDefaultTo("Reactive", "SpinStrength", 0.75f);
        public static float ScratchReactiveStrength = Plugin.ModConfig.GetValueOrDefaultTo("Reactive", "ScratchStrength", 0.75f);

        public static void RegisterColorBlender(GameplayColorBlender blender) => ChromaBlenders.Add(new ChromaBlender(blender));

        public static void UpdateChroma()
        {
            CurrentBeat = Track.Instance.currentBeatWithOffset;
            foreach (var blender in ChromaBlenders)
                blender.UpdateColor();
        }

        public static void UpdateStartColors()
        {
            foreach (var blender in ChromaBlenders)
                blender.UpdateStartColor();
        }

        public static void ResetCurrentColors()
        {
            foreach (var blender in ChromaBlenders)
                blender.ResetCurrentColor();
        }

        public static void SetEnabled(int blenderIndex, bool enabled)
        {
            ChromaBlenders[blenderIndex].Enabled = enabled;
        }

        public static void PrintColors()
        {
            foreach (var blender in ChromaBlenders)
                blender.PrintColor();
        }

        public static void SendReactiveTriggers(Note note)
        {
            float noteBeat = Track.Instance.playStateFirst?.trackData?.GetBeatAtTime(note.time) ?? 1;
            // Plugin.LogMessage("Hit " + note.NoteType + " " + note.colorIndex + " at " + noteBeat);
            switch (note.NoteType)
            {
                case NoteType.Tap:
                case NoteType.HoldStart:
                    ChromaBlenders[(note.colorIndex ^= 1) + 1].UpdateReactiveTrigger(TapReactiveStrength, noteBeat);
                    break;
                case NoteType.Match:
                    ChromaBlenders[(note.colorIndex ^= 1) + 1].UpdateReactiveTrigger(MatchReactiveStrength, noteBeat);
                    break;
                case NoteType.DrumStart:
                    ChromaBlenders[3].UpdateReactiveTrigger(BeatReactiveStrength, noteBeat);
                    break;
                case NoteType.SpinLeftStart:
                    ChromaBlenders[4].UpdateReactiveTrigger(SpinReactiveStrength, noteBeat);
                    break;
                case NoteType.SpinRightStart:
                    ChromaBlenders[5].UpdateReactiveTrigger(SpinReactiveStrength, noteBeat);
                    break;
                case NoteType.ScratchStart:
                    ChromaBlenders[6].UpdateReactiveTrigger(ScratchReactiveStrength, noteBeat);
                    break;
            }
        }

        public static void SendTriggers(string chart)
        {
            if (!CachedTriggers.ContainsKey(chart)) return;
            var triggers = CachedTriggers[chart];
            for (int i = 0; i < BlenderCount; i++)
            {
                var n = (NoteColorType) i;
                if (!triggers.ContainsKey(n)) continue;
                foreach (var t in triggers[n])
                    ChromaBlenders[i].AddTrigger(t);
                ChromaBlenders[i].FinalizeTriggersSetup(i);
            }
        }

        public static void RemoveTriggers()
        {
            foreach (var blender in ChromaBlenders)
            {
                blender.ClearTriggers();
                blender.ResetCurrentColor();
            }
        }

        public static void LoadChromaTriggers(string path)
        {
            // Format overview:
            // Triggers in the file will look something like this:
            // Starting Beat (float), End Beat (float) and Color (int, int, int)
            // The Color will be measured in degrees (0-360°) and percentage (0-100%)
            // https://g.co/kgs/n3oK8U

            string chart = Path.GetFileNameWithoutExtension(path);
            if (CachedTriggers.ContainsKey(chart)) return;
            string filePath = Path.Combine(Directory.GetParent(path).FullName, "Chroma", chart + ".chroma");
            ParseFile(filePath, chart);
            foreach (var p in CachedTriggers[chart])
            {
                Plugin.LogMessage(p.Key);
                foreach (var t in p.Value)
                {
                    Plugin.LogMessage($"{t.StartBeat} {t.Duration} ({t.Color.H} {t.Color.S} {t.Color.L})");
                }
            }
        }

        private static void ParseFile(string filepath, string chart)
        {
            // TODO: Make better parser + file format
            const int GROUP_LENGTH = 6;
            IFormatProvider culture = new CultureInfo("en-US");
            var triggers = new Dictionary<NoteColorType, List<ChromaTrigger>>();
            var fileContents = File.ReadAllLines(filepath);
            foreach (string line in fileContents)
            {
                var groups = line.Split();
                if (groups.Length != GROUP_LENGTH) continue; // Ignore this line if it doesn't have exactly 6 values
                var affectedNotes = groups[0];
                ChromaTrigger trigger = new ChromaTrigger()
                {
                    StartBeat = float.Parse(groups[1], culture),
                    Duration = float.Parse(groups[2], culture),
                    Color = new HSLColor
                    {
                        H = Math.Max(Math.Min(int.Parse(groups[3]), 180), 0) / 360.0f,
                        S = Math.Max(Math.Min(int.Parse(groups[4]), 100), 0) / 100.0f,
                        L = Math.Max(Math.Min(int.Parse(groups[5]), 100), 0) / 100.0f,
                    },
                };
                for (int i = 0; i < BlenderCount - 1; i++)
                {
                    var n = (NoteColorType) (i + 1);
                    if (!triggers.ContainsKey(n))
                        triggers.Add(n, new List<ChromaTrigger>());
                    if (affectedNotes[i] == '1')
                        triggers[n].Add(trigger);
                }
            }

            if (CachedTriggers.ContainsKey(chart))
                CachedTriggers.Remove(chart);
            CachedTriggers.Add(chart, triggers);
        }
    }
}
