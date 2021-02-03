namespace _Template {
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Skill", menuName = "Template/Item/Skill")]
    public class SkillTemplate : ItemTemplate {
        [SerializeField] public GameObject prefab;
        [SerializeField] public TargetingArea targetingArea;
        [SerializeField] public AttributeRequirement[] cost = new AttributeRequirement[0];

        [SerializeField] public ActionTemplate[] effects = new ActionTemplate[0];
        [SerializeField] public ActionModifierTemplate[] modifiers = new ActionModifierTemplate[0];
        [SerializeField, Min(0)] public int count;
        [SerializeField, HideInInspector] public int[] tierMatrix = new int[0];
        [SerializeField, HideInInspector] public TargetType[] modifierMatrix = new TargetType[0];

        public override ItemState Create() => SkillState.Create(this).OnAfterDeserialize<SkillState>();
    }
    [Serializable] public class SkillState : ItemState {
        public static SkillState Create(SkillTemplate template) => new SkillState(){
            template = template
        };
        public override IAction Act(IAgent agent, IContainer target, int index){
            var action = base.Act(agent, target, index);
            if(action != null) return action;
            var template = (SkillTemplate) this.template; 

            if(agent == null || target == null) return null;
            UnitState unit = this.Root<UnitState>();
            if(unit == null || unit.Agency != agent.Agency || unit.Parent != target) return null;
            LocationState location = target as LocationState;
            index = template.targetingArea.GetTile(agent, unit, location, index);
            if(index == -1) return null;
            foreach(var requirement in template.cost) if(!requirement.Match(unit)) return null;

            return new EffectState(){
                template = template,
                agent = agent,
                item = this,
                source = unit,
                location = location,
                tile = index
            };
        }


        public override string Description{get{
            SkillTemplate template = (SkillTemplate) this.template;
            string[] sides = new string[template.count];
            for(int i = 0; i < sides.Length; i++){
                sides[i] += " - [";
                for(int j = 0; j < template.effects.Length; j++){
                    int tier = template.tierMatrix[i + j * template.count];
                    if(tier == 0) continue;
                    sides[i] += $"{template.effects[j].Description(tier - 1)} ";
                }
                for(int j = 0; j < template.modifiers.Length; j++){
                    int tier = template.tierMatrix[i + (j + template.effects.Length) * template.count];
                    sides[i] += $"{template.modifiers[j].Description(tier)} ";
                }
                sides[i] += "]";
            }
            return $"{base.Description}\n{string.Join("\n", sides)}";
        }}
    }

    [Serializable] public class EffectState : IAction {
        [SerializeReference] public IAgent agent;
        [SerializeField] public SkillTemplate template;
        [SerializeReference] public SkillState item;
        [SerializeReference] public UnitState source;
        [SerializeReference] public LocationState location;
        [SerializeField] public int tile;

        public void Apply(WorldState world){
            int set = UnityEngine.Random.Range(0, template.count);
            Debug.Log($"{source.displayName} casted:{set} {template.displayName}");
            foreach(var requirement in template.cost) requirement.Apply(source);

            IModifierNode[] nodes = new IModifierNode[1 + template.effects.Length + template.modifiers.Length];
            nodes[0] = new ModifierEntryNode();
            for(int i = 0; i < template.effects.Length; i++){
                int actionTier = template.tierMatrix[set + i * template.count];
                if(actionTier <= 0) continue;
                nodes[1 + i] = new ModifierSourceNode(){
                    template = template.effects[i],
                    tier = actionTier - 1
                };
            }
            for(int i = 0; i < template.modifiers.Length; i++){
                int modifierTier = template.tierMatrix[set + (template.effects.Length + i) * template.count];
                nodes[1 + template.effects.Length + i] = new ModifierNode(){
                    template = template.modifiers[i],
                    tier = modifierTier
                };
            }
            for(int c = template.modifiers.Length; c >= 0; c--){
                int connected = 0; int nodeIndex = c == 0 ? 0 : nodes.Length - c;
                //TODO check if is not connected and cleanup not working for cycles
                for(int r = 0; r < template.modifiers.Length + template.effects.Length; r++){
                    int index = r + c * (template.modifiers.Length + template.effects.Length);
                    TargetType flag = template.modifierMatrix[index];
                    IModifierNode node = nodes[1 + r];
                    if(flag == TargetType.None || node == null) continue;
                    nodes[nodeIndex].Connect(node, flag);
                    connected++;
                }
                if(connected == 0) nodes[nodeIndex] = null;
            }
            if(nodes[0] == null) return;

            int[] hitTiles = new int[location.TileCount];
            foreach(var action in nodes[0].Apply(agent, item, location, tile)){
                var damage = action as DamageAction;
                //TODO use action index for multihit support?
                if(damage != null && hitTiles[damage.target.Index] == 0){
                    hitTiles[damage.target.Index]++;
                    foreach(var statusEffect in damage.target.GetNodes<IActiveEffect>())
                        statusEffect.Update(world, ActionTriggerType.Hit, action);
                }
                world.AddAction(action);
            }
            for(int i = 0; i < hitTiles.Length; i++)
                if(i != 0){
                    var unit = location[i] as UnitState;
                    if(unit != null){
                        unit.PostAction(world, this);
                    }
                }
        }
    }
}