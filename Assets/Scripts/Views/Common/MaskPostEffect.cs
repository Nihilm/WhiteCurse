using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MaskPostEffect : MonoBehaviour {
    [SerializeField] private Shader shader;

    private List<GameObject> sprites = new List<GameObject>();
    private CommandBuffer _commandBuffer;
    private Camera _camera;
    private Material _material;
    private RenderTexture _renderTexture;
    private int _MaskMap;
    private bool dirty;

    void Start(){
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
        _material = new Material(shader);
        _MaskMap = Shader.PropertyToID("_MaskMap");
        #if PERMAMENT_RENDER_TEXTURE
        _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        _renderTexture.Create();
        #endif
        _commandBuffer = new CommandBuffer();
        #if !IMMIDIATE_COMMAND_BUFFER
        _camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
        #endif
    }
    void OnDestroy(){
        #if PERMAMENT_RENDER_TEXTURE
        _renderTexture.Release();
        _renderTexture.DiscardContents();
        #endif
        #if !IMMIDIATE_COMMAND_BUFFER
        _camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
        #endif
    }
    void OnPreRender(){
        if(!dirty) return;
        dirty = false;

        _commandBuffer.Clear();
        #if PERMAMENT_RENDER_TEXTURE
        _commandBuffer.SetRenderTarget(_renderTexture);
        #else
        _commandBuffer.GetTemporaryRT(_MaskMap, -1, -1, 24, FilterMode.Bilinear);
        _commandBuffer.SetRenderTarget(_MaskMap);
        #endif
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);
        foreach(var sprite in sprites){
            Renderer r = sprite.GetComponent<Renderer>();
            if(r && r.material) _commandBuffer.DrawRenderer(r, r.material);
        }
        #if PERMAMENT_RENDER_TEXTURE
        _commandBuffer.SetGlobalTexture("_MaskMap", _renderTexture);
        #else
        _commandBuffer.SetGlobalTexture("_MaskMap", _MaskMap);
        #endif
    }
    public void Add(GameObject sprite){
        sprites.Add(sprite);
        dirty = true;
    }
    public void Remove(GameObject sprite){
        sprites.Remove(sprite);
        dirty = true;
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination){
        #if IMMIDIATE_COMMAND_BUFFER
        Graphics.ExecuteCommandBuffer(_commandBuffer);
        #endif
        Graphics.Blit(source, destination, _material);
    }
}