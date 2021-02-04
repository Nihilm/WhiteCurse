namespace _Template {
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NPC", menuName = "Template/Agent/NPC")]
    public class NPCTemplate : AgentTemplate {
        public override IAgent Create() => new NPCState(){template = this};
    }

    [Serializable] public class NPCState : IAgent {
        [SerializeField] public NPCTemplate template;
        [NonSerialized] WorldState world;
        public AgentTemplate Agency => template;
        public WorldState World => world;
        public void Enter(WorldState world){
            this.world = world;
            world.actionEvent += OnAction;
        }
        void OnAction(IAction action){
            switch(action){
                case TurnAction nextTurn:
                    if(nextTurn.location.order == this)
                        world.AddAction(TurnAction.Create(nextTurn.location, this));
                break;
            }
        }
    }
}
