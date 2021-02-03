using UnityEngine;

[System.Serializable] public class TopologyGraph {
    [HideInInspector] public Vector2[] positions;
    [HideInInspector] public Vector2Int[] links;
    [HideInInspector] public float zoom = 1;
    [HideInInspector] public Vector2 pan = Vector2.zero;
}
