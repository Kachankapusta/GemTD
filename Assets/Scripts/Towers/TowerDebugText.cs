using TMPro;
using UnityEngine;

namespace Towers
{
    public class TowerDebugText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro[] labels;
        [SerializeField] private string[] texts;

        private void OnValidate()
        {
            ApplyTexts();
        }

        private void Awake()
        {
            ApplyTexts();
        }

        private void ApplyTexts()
        {
            if (labels == null || texts == null)
                return;

            var count = Mathf.Min(labels.Length, texts.Length);

            for (var i = 0; i < count; i++)
            {
                if (labels[i] == null)
                    continue;

                labels[i].text = texts[i];
            }
        }
    }
}