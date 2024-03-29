﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPan : MonoBehaviour {
    public bool resetPositionOnEndDrag = true;
    private Vector3 worldStartPosition;
    private Vector3 cameraStartPosition;
    private Camera camera;
    private bool drag = false;

    void Start(){ camera = GetComponent<Camera>(); }
    void Update(){
        RaycastHit hit;
        var screenPosition = Input.mousePosition;
 
        if(Input.GetMouseButtonDown(0)){
            var ray = camera.ScreenPointToRay(screenPosition);
            drag = Physics.Raycast(ray,out hit,camera.farClipPlane);
            if(drag){
                cameraStartPosition = camera.transform.position;
                worldStartPosition = ray.GetPoint(hit.distance);
            }
            return;
        }
 
 
        if(drag){
            if(Input.GetMouseButton(0)){
                if(camera){
                    var ray = camera.ScreenPointToRay(screenPosition);
 
                    if(!Physics.Raycast(ray,out hit,camera.farClipPlane)){
                        //current raycast not hitting anything then don't move camera
                        bool offscreen = screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x > camera.pixelWidth || screenPosition.y > camera.pixelHeight;
 
                        if(!offscreen) //but if not hitting anything and mouse is onscreen, then assume player dragged to the void and reset camera position
                            camera.transform.position = cameraStartPosition;
 
                        return;
                    }
 
                    var worldDelta = worldStartPosition - ray.GetPoint(hit.distance);
                    camera.transform.position += worldDelta;
                }
            }else{
                // reset and cleanup data from drag
                worldStartPosition = Vector3.zero;
                drag = false;
 
                if(resetPositionOnEndDrag){
                    camera.transform.position = cameraStartPosition;
                }
            }
        }
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
 
// public class SlideWithMouse : MonoBehaviour
// {
//     // Only set one of these
//     public Transform MapToScroll;
//     public Transform CameraToScroll;
 
//     void Start()
//     {
//         // this boogie-logic catches BOTH being defined as well as NEITHER being defined
//         // it compares their not-ness and when it matches, it must be both or neither, which is bad.
//         if (!MapToScroll == !CameraToScroll)
//         {
//             Debug.LogError( "You must only set MapToScroll or CameraToScroll, do not set both!");
//             Debug.Break();
//         }
//     }
 
//     // this state is assocated only with ProcessTouch
//     Vector3 PreviousScreenPoint;
//     Vector3 PreviousWorldPoint;
//     void ProcessTouch( Camera viewCamera, Vector3 screenPosition, bool isDown, bool wentDown)
//     {
//         // replace this part with however you want to get from screen mouse
//         // coordinate to where it is in the world. This just casts it to a
//         // Plane at y == 0
//         var ray = viewCamera.ScreenPointToRay( screenPosition);
//         var plane = new Plane( Vector3.up, Vector3.zero);
//         float distance = 0;
//         if (plane.Raycast( ray, out distance))
//         {
//             Vector3 worldPoint = ray.GetPoint( distance);
 
//             if (wentDown)
//             {
//                 // on the frame the mouse went down, by definition we didn't move at all
//                 PreviousWorldPoint = worldPoint;
//             }
 
//             if (isDown)
//             {
//                 // we are down, how much did we move in world space?
//                 Vector3 worldDelta = worldPoint - PreviousWorldPoint;
 
//                 // NOW! If you want the map to move FASTER than finger speed,
//                 // you can do that here, like so:
// //                worldDelta *= 2.0f;
 
//                 // move the map that much
//                 if (MapToScroll)
//                 {
//                     MapToScroll.position += worldDelta;
//                 }
 
//                 if (CameraToScroll)
//                 {
//                     // recalculate the PreviousScreenPoint using the current
//                     // camera position:
//                     ray = viewCamera.ScreenPointToRay( PreviousScreenPoint);
//                     // we'll presume you're still hitting
//                     plane.Raycast( ray, out distance);
//                     // reget that previous screen point
//                     PreviousWorldPoint = ray.GetPoint( distance);
 
//                     worldDelta = worldPoint - PreviousWorldPoint;
 
//                     CameraToScroll.position -= worldDelta;
//                 }
//             }
 
//             PreviousWorldPoint = worldPoint;
//         }
 
//         PreviousScreenPoint = screenPosition;
//     }
 
//     void Update ()
//     {
//         Vector3 screenPosition = Input.mousePosition;
 
//         bool isDown = Input.GetMouseButton(0);
//         bool wentDown = Input.GetMouseButtonDown(0);
 
//         // at this point screenPosition, isDown and wentDown have been
//         // abstracted from the core Input module; if you want to implement
//         // something like multitouch, you can just replace the above code
//         // and call the ProcessTouch() function below.
 
//         // The camera is where you are regarding the world from.
//         ProcessTouch( viewCamera: Camera.main, screenPosition: screenPosition, isDown: isDown, wentDown: wentDown);
//     }
// }