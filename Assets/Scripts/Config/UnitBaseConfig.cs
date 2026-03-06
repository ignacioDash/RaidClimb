using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "UnitBaseConfig", menuName = "Config/UnitBase")]
    public class UnitBaseConfig : ScriptableObject
    {
        [SerializeField] private float health;
        [SerializeField] private float damage;
        [SerializeField] private float range;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float climbSpeed;
        [SerializeField] private float attackSpeed;
        
        public float Health => health;
        public float Damage => damage;
        public float Range => range;
        public float MovementSpeed => movementSpeed;
        public float ClimbSpeed => climbSpeed;
        public float AttackSpeed => attackSpeed;
    }
}