using System.Collections;
using UnityEngine;
using _Template;

public class EffectView : MonoBehaviour {
    public EffectState Effect{get;set;}
    public BattlefieldView Battlefield{get;set;}
    public GameObject SourceUnit => Battlefield.units[Effect.source];
    public GameObject TargetUnit{get{
        var unit = Effect.location[Effect.tile] as UnitState;
        return unit == null ? null : Battlefield.units[unit];
    }}
}