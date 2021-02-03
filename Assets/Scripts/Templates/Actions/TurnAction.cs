namespace _Template {
    using System;
    using UnityEngine;

    [Serializable] public class TurnAction : IAction {
        [SerializeReference] public LocationState location;
        [SerializeReference] public IAgent prevAgent;
        public static TurnAction Create(
            LocationState location, IAgent agent
        ){
            if(location.order != agent) return null;
            return new TurnAction(){
                location = location,
                prevAgent = agent
            };
        }
        public void Apply(WorldState world){
            int index = world.agents.IndexOf(prevAgent);
            for(int tile = 0; tile < location.TileCount; tile++){
                UnitState unit = location[tile] as UnitState;
                if(unit == null || unit.Agency != prevAgent.Agency) continue;

                foreach(var stamina in unit.GetNodes<StaminaState>())
                    stamina.Modify(-Math.Max(0, stamina.Value));
                foreach(var statusEffect in unit.GetNodes<IActiveEffect>())
                    statusEffect.Update(world, ActionTriggerType.TurnEnd, this);
            }

            int nextIndex = (index + 1) % world.agents.Length;
            //TODO skip agents not present on the map?
            IAgent nextAgent = world.agents[nextIndex];
            location.order = nextAgent;
            Debug.Log($"Next Turn {nextAgent}");

            for(int tile = 0; tile < location.TileCount; tile++){
                UnitState unit = location[tile] as UnitState;
                if(unit == null || unit.Agency != nextAgent.Agency) continue;

                foreach(var stamina in unit.GetNodes<StaminaState>())
                    stamina.Modify(stamina.Capacity - Math.Max(0, stamina.Value));
                foreach(var statusEffect in unit.GetNodes<IActiveEffect>())
                    statusEffect.Update(world, ActionTriggerType.TurnStart, this);
            }

            for(int tile = 0; tile < location.TileCount; tile++){
                GroundEffectState groundEffect = location[tile + location.TileCount] as GroundEffectState;
                if(groundEffect == null) continue;
                if(groundEffect.Agency == prevAgent.Agency)
                    groundEffect.Update(world, ActionTriggerType.TurnEnd, this);
                else if(groundEffect.Agency == nextAgent.Agency)
                    groundEffect.Update(world, ActionTriggerType.TurnStart, this);
            }
        }
    }
}
