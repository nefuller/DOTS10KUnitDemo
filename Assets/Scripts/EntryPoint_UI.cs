using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DOTS10KUnitDemo
{
    public class EntryPoint_UI : MonoBehaviour
    {
        public Slider sliderAlignmentStrength;
        public Slider sliderCohesionStrength;
        public Slider sliderSeparationStrength;
        public Slider sliderAlignmentRange;
        public Slider sliderCohesionRange;
        public Slider sliderSeparationRange;

        public TextMeshProUGUI textAlignmentStrength;
        public TextMeshProUGUI textCohesionStrength;
        public TextMeshProUGUI textSeparationStrength;
        public TextMeshProUGUI textAlignmentRange;
        public TextMeshProUGUI textCohesionRange;
        public TextMeshProUGUI textSeparationRange;

        private void OnEnable()
        {
            sliderAlignmentStrength.value   = GameSettings.AlignmentStrength;
            sliderCohesionStrength.value    = GameSettings.CohesionStrength;
            sliderSeparationStrength.value  = GameSettings.SeparationStrength;
            sliderAlignmentRange.value      = GameSettings.AlignmentRange;
            sliderCohesionRange.value       = GameSettings.CohesionRange;
            sliderSeparationRange.value     = GameSettings.SeparationRange;

            textAlignmentStrength.text      = $"Alignment Strength ({GameSettings.AlignmentStrength})";
            textCohesionStrength.text       = $"Cohesion Strength ({GameSettings.CohesionStrength})";
            textSeparationStrength.text     = $"Separation Strength ({GameSettings.SeparationStrength})";
            textAlignmentRange.text         = $"Alignment Range ({GameSettings.AlignmentRange})";
            textCohesionRange.text          = $"Cohesion Range ({GameSettings.CohesionRange})";
            textSeparationRange.text        = $"Separation Range ({GameSettings.SeparationRange})";

            sliderAlignmentStrength.onValueChanged.AddListener(delegate {
                GameSettings.AlignmentStrength = sliderAlignmentStrength.value;
                textAlignmentStrength.text = $"Alignment Strength ({GameSettings.AlignmentStrength})";
            });

            sliderCohesionStrength.onValueChanged.AddListener(delegate {
                GameSettings.CohesionStrength = sliderCohesionStrength.value;
                textCohesionStrength.text = $"Cohesion Strength ({GameSettings.CohesionStrength})";
            });

            sliderSeparationStrength.onValueChanged.AddListener(delegate {
                GameSettings.SeparationStrength = sliderSeparationStrength.value;
                textSeparationStrength.text = $"Separation Strength ({GameSettings.SeparationStrength})";
            });

            sliderAlignmentRange.onValueChanged.AddListener(delegate {
                GameSettings.AlignmentRange = sliderAlignmentRange.value;
                textAlignmentRange.text = $"Alignment Range ({GameSettings.AlignmentRange})";
            });

            sliderCohesionRange.onValueChanged.AddListener(delegate {
                GameSettings.CohesionRange = sliderCohesionRange.value;
                textCohesionRange.text = $"Cohesion Range ({GameSettings.CohesionRange})";
            });

            sliderSeparationRange.onValueChanged.AddListener(delegate {
                GameSettings.SeparationRange = sliderSeparationRange.value;
                textSeparationRange.text = $"Separation Range ({GameSettings.SeparationRange})";
            });
        }

        private void OnDisable()
        {
            sliderAlignmentStrength.onValueChanged.RemoveAllListeners();
            sliderCohesionStrength.onValueChanged.RemoveAllListeners();
            sliderSeparationStrength.onValueChanged.RemoveAllListeners();
            sliderAlignmentRange.onValueChanged.RemoveAllListeners();
            sliderCohesionRange.onValueChanged.RemoveAllListeners();
            sliderSeparationRange.onValueChanged.RemoveAllListeners();
        }
    }
}