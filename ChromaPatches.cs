using System;
using HarmonyLib;

namespace SpeenChroma
{
    public class ChromaPatches
    {
        private static bool _internalChromaUpdate = false;

        public static bool ChromaUpdate = true;

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void UpdateChroma()
        {
            if (!_internalChromaUpdate || !ChromaUpdate) return;
            ChromaLogic.UpdateChroma();
        }

        [HarmonyPatch(typeof(GameplayColorBlender), nameof(GameplayColorBlender.ApplyAllModifications))]
        [HarmonyPostfix]
        private static void AddColorBlender(GameplayColorBlender __instance) => ChromaLogic.RegisterColorBlender(__instance);

        [HarmonyPatch(typeof(Track), "Awake")]
        [HarmonyPostfix]
        private static void UpdateColorArray()
        {
            InitializeConfigFields();
            ChromaLogic.UpdateStartColors();
            _internalChromaUpdate = true;
        }

        public static void InitializeConfigFields()
        {
            // TODO: Move this to ChromaUI once SpinCore is fixed
            string enabledNotes = Plugin.ModConfig.GetValueOrDefaultTo("Chroma", "AffectedNotes", new string('1', ChromaLogic.BlenderCount - 1));
            for (int i = 0; i < ChromaLogic.BlenderCount - 1; i++)
            {
                ChromaLogic.SetEnabled(i+1, enabledNotes[i] == '1');
            }
        }

        [HarmonyPatch(typeof(XDOptionsMenu), nameof(XDOptionsMenu.OpenColors))]
        [HarmonyPostfix]
        private static void PauseChroma()
        {
            // ChromaLogic.PrintColors(); // For debugging purposes
            ChromaLogic.ResetCurrentColors();
            _internalChromaUpdate = false;
            NotificationSystemGUI.AddMessage("Chroma Updates don't run while the Note Color Overrides menu is open. Remember to hit Close to resume Chroma effects!", 5f);
        }

        [HarmonyPatch(typeof(XDOptionsMenu), "Awake")]
        [HarmonyPostfix]
        private static void AddResumeChromaCallback(XDOptionsMenu __instance)
        {
            __instance.closeColorTabButton.onClick.AddListener(delegate
            {
                ChromaLogic.UpdateStartColors();
                _internalChromaUpdate = true;
            });
        }
        
        [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.AddScore))]
        [HarmonyPostfix]
        private static void GetHitNote(int amount, int comboIncrease, NoteTimingAccuracy timingAccuracy, int noteIndex, PlayState.ScoreState __instance)
        {
            if (!__instance.isMaxPossibleCalculation && amount > 1)
            {
                Note n = Track.Instance.playStateFirst.trackData.NoteData.GetNote(noteIndex);
                ChromaLogic.SendReactiveTriggers(n);
            }
        }

        private static int _currentTrackIndex = 0;
        private static string chartfile = "";
        
        [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
        [HarmonyPostfix]
        private static void AddTriggersForSong()
        {
            // if (Track.IsEditing) return;
            ChromaLogic.SendTriggers(chartfile);
        }

        [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack))]
        [HarmonyPostfix]
        private static void RemoveTriggers()
        {
            ChromaLogic.RemoveTriggers();
        }

        [HarmonyPatch(typeof(XDCustomLevelSelectMenu), "UpdateSelectedHandleIfNeeded")]
        [HarmonyPostfix]
        private static void LoadCustomChroma(int wheelSelectionIndex, XDCustomLevelSelectMenu __instance)
        {
            if (_currentTrackIndex == wheelSelectionIndex) return;
            _currentTrackIndex = wheelSelectionIndex;
            var file = __instance.GetMetadataHandleForIndex(_currentTrackIndex).TrackInfoRef.customFile;
            Plugin.LogMessage(file.FilePath);
            chartfile = file.FileNameNoExtension;
            try
            {
                ChromaLogic.LoadChromaTriggers(file.FilePath);
            }
            catch (Exception e)
            {
                Plugin.LogWarning("Failed to parse chroma triggers for this chart: " + file.FileNameNoExtension);
                Plugin.LogWarning(e);
            }
        }
    }
}
