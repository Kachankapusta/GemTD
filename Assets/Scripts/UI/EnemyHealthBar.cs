using Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Enemy enemy;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Camera targetCamera;

        private void LateUpdate()
        {
            if (targetCamera == null)
                return;

            var camTransform = targetCamera.transform;
            var direction = camTransform.position - transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private void OnEnable()
        {
            if (enemy == null)
                enemy = GetComponentInParent<Enemy>();

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (enemy != null)
                enemy.HealthChanged += OnHealthChanged;

            if (healthSlider != null)
                healthSlider.value = enemy != null ? enemy.HealthPercent : 1f;
        }

        private void OnDisable()
        {
            if (enemy != null)
                enemy.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float normalizedValue)
        {
            if (healthSlider == null)
                return;

            healthSlider.value = Mathf.Clamp01(normalizedValue);
        }
    }
}