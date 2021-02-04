namespace _Template {
    using System;
    using UnityEngine;

    [Serializable] public class TravelAction : IAction {
        [SerializeField] private IAgent agent;
        [SerializeReference] private LocationState prevLocation;
        [SerializeReference] private LocationState nextLocation;
        public static TravelAction Create(
            IAgent agent, LocationState prevLocation, LocationState nextLocation
        ){
            if(prevLocation != null && prevLocation.order != null && prevLocation.order != agent) return null;
            return new TravelAction(){
                agent = agent,
                prevLocation = prevLocation,
                nextLocation = nextLocation
            };
        }
        public void Apply(WorldState world){
            Debug.Log($"Navigate to {nextLocation.area.template.displayName}/{nextLocation.template.displayName}");
            UnityEngine.Random.InitState(0x1b41 + nextLocation.area.seed + nextLocation.Index);
            TransferUnits(agent, prevLocation, nextLocation);

            if(nextLocation.order == null)
                world.AddAction(TurnAction.Create(nextLocation, null, agent));
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