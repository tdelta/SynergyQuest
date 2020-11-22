using UnityEngine;

namespace DamageSystem
{
    [RequireComponent(typeof(Attackable))]
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxValue = 1;
        public int MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                if (Value > maxValue)
                {
                    Value = maxValue;
                }
            }
        }

        private int _value = 0;
        /**
         * <summary>
         * How much health the owner of this component has.
         * </summary>
         */
        public int Value
        {
            get => _value;
            private set
            {
                _value = value;
                if (value > maxValue)
                {
                    Value = maxValue;
                }
                
                OnHealthChanged?.Invoke(value);
                if (value <= 0)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public bool IsDead => Value <= 0;

        public delegate void HealthChangedAction(int value);
        public delegate void DeathAction();

        public event HealthChangedAction OnHealthChanged;
        public event DeathAction OnDeath;

        private Attackable _attackable;

        private void Awake()
        {
            _value = maxValue;
            _attackable = GetComponent<Attackable>();
        }

        private void OnEnable()
        {
            _attackable.OnAttack += OnAttack;
        }
        
        private void OnDisable()
        {
            _attackable.OnAttack -= OnAttack;
        }

        private void OnAttack(AttackData attack)
        {
            Value -= attack.damage;
        }

        public void Reset()
        {
            Value = MaxValue;
        }
    }
}