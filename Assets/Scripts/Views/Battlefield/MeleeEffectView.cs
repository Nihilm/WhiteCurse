using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEffectView : EffectView {
    void Start(){
        StartCoroutine(AnimateHit(SourceUnit, TargetUnit));
    }
    IEnumerator AnimateHit(GameObject source, GameObject target){
        source.GetComponentInChildren<UnitView>().TriggerAnimation(UnitAnimation.ATTACK);
        transform.position = target.transform.position;
        TargetUnit.GetComponentInChildren<UnitView>().TriggerAnimation(UnitAnimation.HIT);

        ParticleSystem explosion = GetComponentInChildren<ParticleSystem>();
        explosion.Play();
        Destroy(gameObject, explosion.main.duration);
        yield return new WaitForSeconds(explosion.main.duration);
    }
}
