using UnityEngine.Rendering;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OpaquePostEffect : MonoBehaviour {
    [SerializeField] private Shader shader;

    private Camera _camera;
    private Material _material;
    void Start(){
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
        _material = new Material(shader);
    }
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination){
        Graphics.Blit(source, destination, _material);
    }
}
