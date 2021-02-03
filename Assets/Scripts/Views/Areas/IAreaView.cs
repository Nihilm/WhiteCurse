using UnityEngine;
using _Template;

public interface IAreaView {
    AreaState Area{set;}
    LocationState Location{set;}
    event System.Action<LocationState> navigationEvent;
}