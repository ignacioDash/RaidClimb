using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "UnitBaseConfig", menuName = "Config/UnitBase")]
    public class UnitBaseConfig : ScriptableObject
    {
        [SerializeField] private string unitName;
        [SerializeField] private int arenaUnlock;
        [SerializeField] private int squadCost;
        [SerializeField] private float health;
        [SerializeField] private float damage;
        [SerializeField] private float range;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float climbSpeed;
        [SerializeField] private float attackSpeed;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;

        public string UnitName => unitName;
        public int ArenaUnlock => arenaUnlock;
        public int SquadCost => squadCost;
        public float Health => health;
        public float Damage => damage;
        public float Range => range;
        public float MovementSpeed => movementSpeed;
        public float ClimbSpeed => climbSpeed;
        public float AttackSpeed => attackSpeed;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
    }
}