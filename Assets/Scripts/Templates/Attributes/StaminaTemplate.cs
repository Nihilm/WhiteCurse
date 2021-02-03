namespace _Template {
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "Stamina", menuName = "Template/Attribute/Stamina")]
    public class StaminaTemplate : AttributeTemplate {
        public override IAttribute Create(int value) => new StaminaState(){
            template = this,
            capacity = value,
            value = value
        };
    }

    public class StaminaState : AttributeState {
        
    }
}