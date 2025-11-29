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

            var cellSize = tower.CellSize;
            var worldRange = tower.RangeInCells * cellSize;
            var diameter = worldRange * 2f;

            var scale = visual.localScale;
            scale.x = diameter;
            scale.z = diameter;
            visual.localScale = scale;
        }

        public void SetVisible(bool visible)
        {
            if (visual != null)
                visual.gameObject.SetActive(visible);
        }
    }
}