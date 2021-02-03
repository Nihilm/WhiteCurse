using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitAnimation {
    IDLE,
    HIT,
    DEATH,
    ATTACK
}

public class UnitView : MonoBehaviour {
    private Animator animator;
    void Start(){
        animator = GetComponentInChildren<Animator>();
    }
    public void TriggerAnimation(UnitAnimation animation){
        switch(animation){
            case UnitAnimation.ATTACK:
                animator.SetTrigger("Attack");
            break;
            case UnitAnimation.HIT:
                animator.SetTrigger("Hit");
            break;
            case UnitAnimation.DEATH:
                animator.SetTrigger("Death");
                Destroy(gameObject, 2.0f);
            break;
        }
    }
}
