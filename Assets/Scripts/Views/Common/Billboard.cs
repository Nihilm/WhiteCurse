using UnityEngine;

public class Billboard : MonoBehaviour {
    private Transform cameraTransform;
    void Start(){
        cameraTransform = Camera.main.transform;
        if(GetComponent<Canvas>() != null) GetComponent<Canvas>().worldCamera = Camera.main;
    }
    void LateUpdate(){
        transform.forward = cameraTransform.forward;
        //transform.position = _camera.WorldToScreenPoint(target.transform.position + offset);
        //transform.position = _camera.WorldToViewportPoint(target.transform.position + offset);
    }
}
