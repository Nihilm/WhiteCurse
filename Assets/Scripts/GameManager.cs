using UnityEngine;
using _Template;

public class GameManager : MonoBehaviour {
    [SerializeField] private WorldTemplate world;

    void Start(){
        StartCoroutine(world.state.Update());
    }
}
