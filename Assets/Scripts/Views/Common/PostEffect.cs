// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.Rendering;
// using UnityEngine;

// [RequireComponent(typeof(Camera))]
// public class PostEffect : MonoBehaviour {
//     public Shader shader;
//     public GameObject[] objects;

//     private CommandBuffer commandBuffer;
//     private RenderTexture renderTexture;
//     private Camera _camera;
//     private Material material;

//     void Start(){
//         _camera = GetComponent<Camera>();
//         _camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
//         material = new Material(shader);

//         //renderTexture = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
//         //renderTexture.Create();
//         //_camera.targetTexture = renderTexture;
//         //RenderTexture.active = texture;
//         //Graphics.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), texture);

//         // if(objects == null || objects.Length == 0) return;

//         // commandBuffer = new CommandBuffer();
//         // int tempID = Shader.PropertyToID("_Temp1");
//         // commandBuffer.GetTemporaryRT(tempID, -1, -1, 24, FilterMode.Bilinear);
//         // commandBuffer.SetRenderTarget(tempID);
//         // commandBuffer.ClearRenderTarget(true, true, Color.black);
//         // if(objects != null) foreach(var o in objects){
//         //     Renderer r = o.GetComponent<Renderer>();
//         //     if(r && r.material) commandBuffer.DrawRenderer(r, r.material);
//         // }
//         // commandBuffer.SetGlobalTexture("_EffectMap", tempID);
//         // _camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer);
//     }
//     void OnDestroy(){
//         renderTexture.DiscardContents();
//         renderTexture.Release();
//         _camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer); 
//     }
//     //void OnPreRender(){}
//     //void OnPostRender(){}
//     //void OnWillRenderObject(){if(!gameObject.activeInHierarchy || !enabled) return;var _camera = Camera.current;}
//     [ImageEffectOpaque]
//     void OnRenderImage(RenderTexture source, RenderTexture destination){
//         Graphics.Blit(source, destination, material);
//         // if(commandBuffer != null) Graphics.ExecuteCommandBuffer(commandBuffer);
//     }
// }