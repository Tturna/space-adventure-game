using UnityEngine;

namespace Inventory.Item_SOs.Accessories
{
    [CreateAssetMenu(fileName = "Basic Accessory", menuName = "SO/Accessories/Basic Accessory")]
    public class BasicAccessorySo : ItemSo
    {
        // Making all the variables into virtual properties so that they can be overridden by other scriptable objects.
        // This preserves polymorphism.
        
        // C# geek note: auto-properties have a backing field that is generated by the compiler.
        // We use the "field" keyword to serialize it so it shows up in the inspector.
        
        [field: SerializeField]
        public virtual float MaxHealthIncrease { get; set; }

        [field: SerializeField, Range(0f, 100f)]
        public virtual float MaxHealthMultiplier { get; set; } = 1f;
        
        [field: SerializeField]
        public virtual float MaxEnergyIncrease { get; set; }

        [field: SerializeField, Range(0f, 100f)]
        public virtual float MaxEnergyMultiplier { get; set; } = 1f;
        
        [field: SerializeField]
        public virtual float DefenseIncrease { get; set; }

        [field: SerializeField, Range(0f, 100f)]
        public virtual float DefenseMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float DamageReductionMultiplier { get; set; } = 1f;
        
        [field: SerializeField]
        public virtual float DamageIncrease { get; set; }

        [field: SerializeField, Range(0f, 100f)]
        public virtual float DamageMultiplier { get; set; } = 1f;
        
        [field: SerializeField]
        public virtual float DefensePenetrationIncrease { get; set; }

        [field: SerializeField, Range(0f, 100f)]
        public virtual float DefensePenetrationMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float CritChanceMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float KnockbackMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float MoveSpeedMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float JumpHeightMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float AttackSpeedMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float MiningSpeedMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float MiningPowerMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float BuildingSpeedMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(0f, 100f)]
        public virtual float JetpackRechargeMultiplier { get; set; } = 1f;
        
        [field: SerializeField]
        public virtual float JetpackChargeIncrease { get; set; }
        
        //
        
        public virtual void ResetBehavior() { }
        
        public virtual void UpdateProcess() { }
    }
}
