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
            sliderAlignmentStrength.value = GameSettings.AlignmentStrength;
            sliderCohesionStrength.value = GameSettings.CohesionStrength;
            sliderSeparationStrength.value = GameSettings.SeparationStrength;
            sliderAlignmentRange.value = GameSettings.AlignmentRange;
            sliderCohesionRange.value = GameSettings.CohesionRange;
            sliderSeparationRange.value = GameSettings.SeparationRange;

            textAlignmentStrength.text = string.Format("Alignment Strength ({0})", GameSettings.AlignmentStrength);
            textCohesionStrength.text = string.Format("Cohesion Strength ({0})", GameSettings.CohesionStrength);
            textSeparationStrength.text = string.Format("Separation Strength ({0})", GameSettings.SeparationStrength);
            textAlignmentRange.text = string.Format("Alignment Range ({0})", GameSettings.AlignmentRange);
            textCohesionRange.text = string.Format("Cohesion Range ({0})", GameSettings.CohesionRange);
            textSeparationRange.text = string.Format("Separation Range ({0})", GameSettings.SeparationRange);

            sliderAlignmentStrength.onValueChanged.AddListener(delegate {
                GameSettings.AlignmentStrength = sliderAlignmentStrength.value;
                textAlignmentStrength.text = string.Format("Alignment Strength ({0})", GameSettings.AlignmentStrength);
            });
            sliderCohesionStrength.onValueChanged.AddListener(delegate {
                GameSettings.CohesionStrength = sliderCohesionStrength.value;
                textCohesionStrength.text = string.Format("Cohesion Strength ({0})", GameSettings.CohesionStrength);
            });
            sliderSeparationStrength.onValueChanged.AddListener(delegate {
                GameSettings.SeparationStrength = sliderSeparationStrength.value;
                textSeparationStrength.text = string.Format("Separation Strength ({0})", GameSettings.SeparationStrength);
            });
            sliderAlignmentRange.onValueChanged.AddListener(delegate {
                GameSettings.AlignmentRange = sliderAlignmentRange.value;
                textAlignmentRange.text = string.Format("Alignment Range ({0})", GameSettings.AlignmentRange);
            });
            sliderCohesionRange.onValueChanged.AddListener(delegate {
                GameSettings.CohesionRange = sliderCohesionRange.value;
                textCohesionRange.text = string.Format("Cohesion Range ({0})", GameSettings.CohesionRange);
            });
            sliderSeparationRange.onValueChanged.AddListener(delegate {
                GameSettings.SeparationRange = sliderSeparationRange.value;
                textSeparationRange.text = string.Format("Separation Range ({0})", GameSettings.SeparationRange);
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