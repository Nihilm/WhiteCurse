using System.Collections;
using UnityEngine;

public class ProjectileEffectView : EffectView {
    void Start(){
        SourceUnit.GetComponentInChildren<UnitView>().TriggerAnimation(UnitAnimation.ATTACK);
        transform.position = SourceUnit.transform.position;

        var targetUnit = Effect.location[Effect.tile] as _Template.UnitState;
        if(targetUnit != null)
            StartCoroutine(AnimateProjectile(gameObject, TargetUnit));
        else
            StartCoroutine(Animate(Battlefield.tiles[Effect.tile]));
    }
    IEnumerator Animate(GameObject target){
        Vector3 startingPosition = gameObject.transform.position;
        Vector3 finalPosition = target.transform.position;
        float elapsedTime = 0;
        float duration = 1.0f;
        while(elapsedTime < duration){
            gameObject.transform.position = Vector3.Lerp(startingPosition, finalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    IEnumerator AnimateProjectile(GameObject projectile, GameObject target){
        Vector3 startingPosition = projectile.transform.position;
        Vector3 finalPosition = target.transform.position;
        float elapsedTime = 0;
        float duration = 1.0f;
        while(elapsedTime < duration){
            projectile.transform.position = Vector3.Lerp(startingPosition, finalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if(TargetUnit != null) TargetUnit.GetComponentInChildren<UnitView>().TriggerAnimation(UnitAnimation.HIT);
        Destroy(projectile);
    }
}
