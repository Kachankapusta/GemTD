using UnityEngine;

namespace Towers
{
    public class TowerRangeIndicator : MonoBehaviour
    {
        [SerializeField] private Transform visual;

        public void SyncWithTower(Tower tower)
        {
            if (tower == null || visual == null || tower.Config == null)
                return;

            var worldRange = tower.RangeInCells * tower.CellSize;
            var diameter = worldRange * 2f;

            var parent = visual.parent;
            var parentScale = parent != null ? parent.lossyScale : Vector3.one;

            var scale = visual.localScale;
            var safeParentScaleX = parentScale.x != 0f ? parentScale.x : 1f;
            var safeParentScaleZ = parentScale.z != 0f ? parentScale.z : 1f;

            scale.x = diameter / safeParentScaleX;
            scale.z = diameter / safeParentScaleZ;
            visual.localScale = scale;
        }

        public void SetVisible(bool visible)
        {
            if (visual != null)
                visual.gameObject.SetActive(visible);
        }
    }
}