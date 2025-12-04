using TMPro;
using UnityEngine;

namespace Towers
{
    public class TowerDebugText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro[] labels;

        private void Awake()
        {
            ApplyTexts();
        }

        private void ApplyTexts()
        {
            if (labels == null || labels.Length == 0)
                return;

            var qualityText = GetQualityTextFromName();
            if (string.IsNullOrEmpty(qualityText))
                return;

            foreach (var label in labels)
            {
                if (label == null)
                    continue;
                label.gameObject.SetActive(true);
                label.text = qualityText;
            }
        }

        private string GetQualityTextFromName()
        {
            var nameLower = GetNameForDetect().ToLowerInvariant();

            return nameLower.Contains("chipped") ? "I" :
                nameLower.Contains("flawed") ? "II" :
                nameLower.Contains("normal") ? "III" :
                nameLower.Contains("flawless") ? "IV" :
                nameLower.Contains("perfect") ? "V" :
                string.Empty;
        }

        private string GetNameForDetect()
        {
            var tower = GetComponentInParent<Tower>();
            return tower != null
                ? tower.gameObject.name
                : gameObject.name;
        }
    }
}