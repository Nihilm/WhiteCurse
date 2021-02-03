namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "AI", menuName = "Template/Agent/AI")]
    public class AITemplate : AgentTemplate {
        public override IAgent Create() => new AIState(){template = this};
    }

    [Serializable] public class AIState : IAgent {
        [SerializeField] public AITemplate template;
        [NonSerialized] WorldState world;
        public AgentTemplate Agency => template;
        public void Enter(WorldState world){
            this.world = world;
            world.actionEvent += OnAction;
        }
        void OnAction(IAction action){
            switch(action){
                case TurnAction nextTurn:
                    if(nextTurn.location.order == this)
                        PerformBattlefieldLogic(nextTurn.location);
                break;
            }
        }
        void PerformBattlefieldLogic(LocationState location){
            var units = new List<UnitState>();
            for(int i = 0; i < location.TileCount; i++){
                UnitState unit = location[i] as UnitState;
                if(unit == null || unit.Agency != Agency) continue;
                units.Add(unit);
            }
            foreach(var unit in units){
                var tile = location.GetTile(unit.Index);
                var zone = location.GetZone(Agency);
                if(tile.x > zone.x && location[tile.x - 1, tile.y] == null){
                    int index = tile.y + (tile.x-1) * location.template.rows;
                    var action = unit.Act(this, location, index);
                    Debug.Log($"Moving {unit.DisplayName} {index} {action}");
                    if(action != null) world.AddAction(action);
                }
                List<IAction> actions = new List<IAction>(GetPossibleActions(unit));
                Debug.Log($"{unit.DisplayName} with {actions.Count} options");
                if(actions.Count == 0) continue;
                world.AddAction(actions[UnityEngine.Random.Range(0, actions.Count)]);
            }

            world.AddAction(TurnAction.Create(location, this));
        }
        IEnumerable<IAction> GetPossibleActions(UnitState unit){
            InventoryState skillset = unit.GetNodes<InventoryState>().First();
            LocationState location = unit.Parent as LocationState;
            if(skillset == null) yield break;
            for(int i = 0; i < skillset.Count; i++){
                SkillState skill = skillset[i] as SkillState;
                if(skill == null) continue;

                for(int tile = 0; tile < location.TileCount; tile++){
                    IAction action = skill.Act(this, location, tile);
                    if(action == null) continue;
                    yield return action;
                }
            }
        }
    }
}
