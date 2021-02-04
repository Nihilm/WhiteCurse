namespace _Template {
    using System;
    using UnityEngine;

    [Serializable] public struct AttributeDamage {
        [SerializeField] public AttributeTemplate attribute;
        [SerializeField] public double multiplier;
        [SerializeField] public bool permament;
        [SerializeField] public bool clamp;
    }
    
    [CreateAssetMenu(fileName = "Damage", menuName = "Template/Action/Damage")]
    public class DamageTemplate : ActionTemplate {
        [SerializeField] public AttributeDamage[] sourceAttributes = new AttributeDamage[0];
        [SerializeField] public AttributeDamage[] targetAttributes = new AttributeDamage[0];
        [SerializeField] public int baseValue;
        [SerializeField] public double[] multipliers = new double[]{0};

        public override int TierCount => multipliers.Length;
        public override string Description(int tier) => $"<b>{100*multipliers[tier]}%</b> <color=#ff0000ff>{displayName}</color>";

        public override IAction Create(IAgent agent, ITarget source, IContainer target, int index, int tier){
            UnitState targetUnit = target?[index] as UnitState;
            UnitState sourceUnit = source?.Parent?.Parent as UnitState;

            //TODO support when cast by thing directly, not through item
            
            if(targetUnit == null || sourceUnit == null) return null;
            return new DamageAction(){
                template = this,
                tier = tier,
                source = sourceUnit,
                target = targetUnit
            };
        }
    }

    public class DamageAction : IAction {
        [SerializeField] public DamageTemplate template;
        [SerializeReference] public IContainer source;
        [SerializeReference] public IContainer target;
        [SerializeField] public int tier;

        public virtual int CalculcateBaseValue(){
            int value = -template.baseValue;
            for(int i = 0; i < template.sourceAttributes.Length; i++)
                foreach(var attribute in source.GetNodes<IAttribute>(template.sourceAttributes[i].attribute)){
                    value -= (int) (attribute.Value * template.sourceAttributes[i].multiplier);
                }
            value = (int) (value * template.multipliers[Math.Min(tier, template.multipliers.Length - 1)]);
            return value;
        }

        public void Apply(WorldState world){
            int baseValue = CalculcateBaseValue();
            int value = baseValue;
            Debug.Log($"{source?.DisplayName} deals {value} {template.displayName} to {target.DisplayName}");
            for(int i = 0; i < template.targetAttributes.Length && value < 0; i++){
                value = (int) (value * template.targetAttributes[i].multiplier);
                foreach(var attribute in target.GetNodes<IAttribute>(template.targetAttributes[i].attribute)){
                    int prev = value;
                    if(!template.targetAttributes[i].clamp) value = attribute.Modify(value);
                    else value = attribute.Modify(baseValue < 0
                    ? Math.Max(value, baseValue - attribute.Value)
                    : Math.Min(value, baseValue - attribute.Value));
                    if(template.targetAttributes[i].permament) attribute.Capacity -= (value - prev);
                }
                value = (int) (value / template.targetAttributes[i].multiplier);
            }
        }
    }
}