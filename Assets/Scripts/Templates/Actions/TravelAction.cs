namespace _Template {
    using System;
    using UnityEngine;

    [Serializable] public class TravelAction : IAction {
        [SerializeField] private IAgent agent;
        [SerializeReference] private LocationState prevLocation;
        [SerializeReference] private LocationState nextLocation;
        public static TravelAction Create(
            IAgent agent, LocationState prevLocation, LocationState nextLocation
        ) => new TravelAction(){
            agent = agent,
            prevLocation = prevLocation,
            nextLocation = nextLocation
        };
        public void Apply(WorldState world){
            UnityEngine.Random.InitState(0x1b41 + nextLocation.area.seed + nextLocation.Index);
            Debug.Log($"Navigate to {nextLocation.area.template.displayName}/{nextLocation.template.displayName}");
            TransferUnits(agent, prevLocation, nextLocation);

            if(nextLocation.order == null) nextLocation.order = agent;
            //world.AddAction(TurnAction.Create(nextLocation, ))

            for(int i = 0; i < nextLocation.TileCount; i++){
                UnitState unit = nextLocation[i] as UnitState;
                if(unit == null) continue;
                if(unit.Agency == agent.Agency){
                    //TODO remove that. Only apply debuf
                    var stamina = unit.GetNodes<StaminaState>().First();
                    stamina.Modify(stamina.Capacity - stamina.Value);
                }else{
                    var stamina = unit.GetNodes<StaminaState>().First();
                    stamina.Modify(-stamina.Value);
                }
            }

        }
        void TransferUnits(IAgent agent, LocationState prevLocation, LocationState nextLocation){
            if(prevLocation == null) return;
            for(int count = 0, offset = 0, i = 0; i < prevLocation.TileCount; i++){
                var unit = prevLocation[i] as UnitState;
                if(unit == null || unit.Agency != agent.Agency) continue;
                var tile = prevLocation.GetTile(i);
                if(count == 0) offset = tile.x;
                tile.x -= offset;
                nextLocation.Add(unit, tile[1] + tile[0] * nextLocation.template.rows);
                count++;
            }
        }
    }
}