namespace _Template {
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Player", menuName = "Template/Agent/Player")]
    public class PlayerTemplate : AgentTemplate {
        [NonSerialized, HideInInspector] public PlayerState state;
        [SerializeField] private InventoryTemplate inventory;
        public override IAgent Create() => new PlayerState(){
            template = this,
            shared = inventory?.Create()
        };

        public void EndTurn(){
            if(state.hoveredSlot != -1) UnHoverTarget(state.hoveredContainer, state.hoveredSlot);
            if(state.pressedSlot != -1) ReleaseTarget(state.pressedContainer, state.pressedSlot);
            var action = TurnAction.Create(state.activeLocation, state);
            if(action != null) state.world.AddAction(action);
        }
        #region Navigation
        public delegate void NavigationEvent(LocationState previousLocation);
        public event NavigationEvent navigationEvent;
        public void Navigate(LocationState location){
            LocationState previousLocation = state.activeLocation;
            if(previousLocation != null && previousLocation.order != state) return;
            AreaState nextArea = location.area;
            if(previousLocation != null && previousLocation.area == nextArea
            && previousLocation.area.graph[previousLocation.Index, location.Index] == 0)
                return;
            var action = TravelAction.Create(this.state, previousLocation, location);
            if(action == null) return;
            state.activeLocation = location;
            state.world.AddAction(action);
            navigationEvent?.Invoke(previousLocation);
        }
        #endregion
        #region Interaction
        public delegate void InteractionEvent(IContainer container, int index, bool toggle);
        public event InteractionEvent pressTargetEvent;
        public event InteractionEvent hoverTargetEvent;
        public bool SelectTarget(IContainer container, int slot){
            if(state.pressedSlot != -1) return false;
            if(container[slot] == null) return false;
            state.pressedContainer = container; state.pressedSlot = slot;
            pressTargetEvent?.Invoke(container, slot, true);
            return true;
        }
        public bool ReleaseTarget(IContainer container, int slot){
            if(state.pressedContainer != container || state.pressedSlot != slot) return false;
            if(state.hoveredSlot != -1 && state.activeAction != null){
                state.world.AddAction(state.activeAction);
                UnHoverTarget(state.hoveredContainer, state.hoveredSlot);
            }
            state.pressedContainer = null; state.pressedSlot = -1;
            pressTargetEvent?.Invoke(container, slot, false);
            return true;
        }
        public bool UnHoverTarget(IContainer container, int slot){
            if(state.hoveredContainer != container || state.hoveredSlot != slot) return false;
            state.hoveredContainer = null; state.hoveredSlot = -1;
            state.activeAction = null;
            hoverTargetEvent?.Invoke(container, slot, false);
            return true;
        }
        public bool HoverTarget(IContainer container, int slot){
            if(state.hoveredSlot != -1)
                UnHoverTarget(state.hoveredContainer, state.hoveredSlot);
            if(state.pressedSlot != -1){
                if(state.pressedSlot == slot && state.pressedContainer == container) return false;
                state.activeAction = state.ActiveTarget.Act(this.state, container, slot);
                if(state.activeAction == null) return false;
            } //else return false; //mobile behaviour
            state.hoveredContainer = container;
            state.hoveredSlot = slot;
            hoverTargetEvent?.Invoke(container, slot, true);
            return true;
        }
        #endregion
    }
    [System.Serializable] public class PlayerState : IAgent {
        [SerializeField] public PlayerTemplate template;
        public AgentTemplate Agency => template;
        public WorldState World => world;
        [SerializeReference] public InventoryState shared;

        [NonSerialized] public WorldState world;
        [NonSerialized] public IContainer pressedContainer;
        [NonSerialized] public int pressedSlot = -1;
        [NonSerialized] public IContainer hoveredContainer;
        [NonSerialized] public int hoveredSlot = -1;
        [NonSerialized] public IAction activeAction;
        [SerializeReference] public LocationState activeLocation;

        public ITarget ActiveTarget => pressedSlot == -1 ? null : pressedContainer[pressedSlot];
        public AreaState ActiveArea => activeLocation?.area;

        public void Enter(WorldState world){
            this.world = world;
            this.template.state = this;
            this.template.Navigate(activeLocation != null
            ? activeLocation : world.areas[0].locations[0]);
        }
    }
}