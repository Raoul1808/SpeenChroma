using System.Collections.Generic;

namespace SpeenChroma
{
    public class ChromaBlender
    {
        private GameplayColorBlender _blender;
        private HSLColor _startColor;
        private HSLColor _currentColor;
        private ChromaTrigger _reactiveTrigger;
        private List<ChromaTrigger> _triggers = new List<ChromaTrigger>();
        private int _currentTriggerIndex = 0;
        
        public bool Enabled = true;
        
        public ChromaBlender(GameplayColorBlender blender)
        {
            _blender = blender;
        }

        public void AddTrigger(ChromaTrigger trigger)
        {
            _triggers.Add(trigger);
        }

        public void UpdateColor()
        {
            if (!Enabled) return;
            switch (ChromaLogic.Mode)
            {
                case ChromaMode.Rainbow:
                    UpdateRainbow(ChromaLogic.RainbowSpeed);
                    break;
                case ChromaMode.Reactive:
                    UpdateReactive();
                    break;
                case ChromaMode.Custom:
                    UpdateTriggers();
                    break;
            }

            _blender.SetHSL(_currentColor.H, _currentColor.S, _currentColor.L);
        }

        public void PrintColor()
        {
            Plugin.LogMessage($"H: {_startColor.H} S: {_startColor.S} L: {_startColor.L}");
        }

        private void UpdateRainbow(float rainbowSpeed)
        {
            var h = _currentColor.H;
            _currentColor.H = (h >= 1f ? h - 1f : h) + 0.1f * Time.deltaTime * rainbowSpeed;
        }

        private void UpdateTriggers()
        {
            // Check if we have any triggers left
            if (_currentTriggerIndex >= _triggers.Count) return;
            
            // Get the current trigger
            var currentTrigger = _triggers[_currentTriggerIndex];
            
            // Skip if not in time
            if (currentTrigger.StartBeat > ChromaLogic.CurrentBeat) return;
            
            // Check if the current trigger is obsolete
            if (currentTrigger.StartBeat + currentTrigger.Duration <= ChromaLogic.CurrentBeat)
            {
                _currentColor = currentTrigger.Color;
                _currentTriggerIndex++;
                UpdateTriggers();
                return;
            }

            HSLColor previousEndColor = _currentTriggerIndex <= 0 ? _startColor : _triggers[_currentTriggerIndex - 1].Color;
            
            // Set color according to progress through trigger
            float progress = (ChromaLogic.CurrentBeat - currentTrigger.StartBeat) / currentTrigger.Duration;
            if (progress < 0) return;
            
            // currentColor.H = (float)(targetColor.H + (startColor.H - targetColor.H) * progress);
            // currentColor.H = (float)(startColor.H + (targetColor.H - startColor.H) * progress);
            _currentColor.H = (float) (previousEndColor.H + (currentTrigger.Color.H - previousEndColor.H) * progress);
            _currentColor.S = (float) (previousEndColor.S + (currentTrigger.Color.S - previousEndColor.S) * progress);
            _currentColor.L = (float) (previousEndColor.L + (currentTrigger.Color.L - previousEndColor.L) * progress);
        }

        public void UpdateTriggerIndex()
        {
            // This is a very dirty way of doing it, but it'll get the job done for the editor
            _currentTriggerIndex = 0;
            ChromaTrigger t = new ChromaTrigger(), pt = new ChromaTrigger();
            for (int i = 0; i < _triggers.Count; i++)
            {
                pt = t;
                t = _triggers[i];
                if (i == 0) continue; // Skip first so we can compare
                
            }
        }

        public void ResetTriggerVariables()
        {
            _currentTriggerIndex = 0;
            ResetCurrentColor();
        }

        public void FinalizeTriggersSetup(int i)
        {
            ResetTriggerVariables();
            _currentTriggerIndex = 0;
            _triggers.Sort((x, y) => x.StartBeat.CompareTo(y.StartBeat));
        }

        public void ClearTriggers()
        {
            _triggers.Clear();
        }

        public void UpdateReactive()
        {
            if (_reactiveTrigger.StartBeat > ChromaLogic.CurrentBeat) return;
            if (_reactiveTrigger.StartBeat + _reactiveTrigger.Duration < ChromaLogic.CurrentBeat)
            {
                _currentColor = _startColor;
                return;
            }

            float progress = (ChromaLogic.CurrentBeat - _reactiveTrigger.StartBeat) / _reactiveTrigger.Duration;
            _currentColor.L = (float) (_reactiveTrigger.Color.L + (_startColor.L - _reactiveTrigger.Color.L) * progress);
        }

        public void UpdateReactiveTrigger(float lightness, float noteBeat)
        {
            if (_currentColor.L > lightness) return;
            _reactiveTrigger = new ChromaTrigger
            {
                Color = new HSLColor
                {
                    L = lightness,
                },
                Duration = 1,
                StartBeat = noteBeat,
            };
        }

        public void UpdateStartColor()
        {
            float hue = (float) Utilities.GetInstanceField(_blender.GetType(), _blender, "hue");
            float saturation = (float) Utilities.GetInstanceField(_blender.GetType(), _blender, "saturation");
            float lightness = (float) Utilities.GetInstanceField(_blender.GetType(), _blender, "lightness");
            _currentColor = _startColor = new HSLColor
            {
                H = hue,
                S = saturation,
                L = lightness,
            };
        }

        public void ResetCurrentColor()
        {
            _currentColor = _startColor;
        }
    }
}
