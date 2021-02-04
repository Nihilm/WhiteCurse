namespace _Template {
    using System;
    using UnityEngine;

    [Serializable] public class TurnAction : IAction {
        [SerializeReference] public LocationState location;
        [SerializeReference] public IAgent prevAgent;
        [SerializeReference] public IAgent nextAgent;
        public static TurnAction Create(LocationState location, IAgent agent){
            int index = agent.World.agents.IndexOf(agent);
            int nextIndex = (index + 1) % agent.World.agents.Length;
            //TODO skip agents not present on the map?
            IAgent nextAgent = agent.World.agents[nextIndex];
            return Create(location, agent, nextAgent);
        }
        public static TurnAction Create(LocationState location, IAgent prevAgent, IAgent nextAgent){
            if(location.order != prevAgent) return null;
            return new TurnAction(){
                location = location,
                prevAgent = prevAgent,
                nextAgent = nextAgent
            };
        }
        public void Apply(WorldState world){
            Debug.Log($"Next Turn {nextAgent}");
            for(int tile = 0; tile < location.TileCount; tile++){
                UnitState unit = location[tile] as UnitState;
                if(unit == null) continue;
                if(prevAgent == null ? unit.Agency == nextAgent.Agency : unit.Agency != prevAgent.Agency) continue;

                foreach(var stamina in unit.GetNodes<StaminaState>())
                    stamina.Modify(-Math.Max(0, stamina.Value));
                foreach(var statusEffect in unit.GetNodes<IActiveEffect>())
                    statusEffect.Update(world, ActionTriggerType.TurnEnd, this);
            }

            location.order = nextAgent;
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
                if(prevAgent != null && groundEffect.Agency == prevAgent.Agency)
                    groundEffect.Update(world, ActionTriggerType.TurnEnd, this);
                else if(groundEffect.Agency == nextAgent.Agency)
                    groundEffect.Update(world, ActionTriggerType.TurnStart, this);
            }
        }
    }
}
