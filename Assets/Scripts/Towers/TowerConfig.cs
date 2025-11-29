using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(menuName = "GemTD/Tower Config")]
    public class TowerConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private GemType type;
        [SerializeField] private GemQuality quality;
        [SerializeField] private float range = 5f;
        [SerializeField] private float fireInterval = 0.5f;
        [SerializeField] private int damage = 10;
        [SerializeField] private int goldCost;
        [SerializeField] private string abilityDescription;

        public string Id => id;
        public GemType Type => type;
        public GemQuality Quality => quality;
        public float Range => range;
        public float FireInterval => fireInterval;
        public int Damage => damage;
        public int GoldCost => goldCost < 0 ? 0 : goldCost;
        public string AbilityDescription => abilityDescription;

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                    return quality + " " + type;
                return id;
            }
        }
    }
}