using UnityEngine;
using UnityEngine.Rendering;

public class UVSpaceRenderer : MonoBehaviour
{
    const int TEXTURE_RESOLUTION = 128;
    [SerializeField] Material _uvMaskMaterial;
    [SerializeField] Renderer _renderer;
    [SerializeField] Material _uvDilationMaterial;
    [SerializeField] Material _maskFillMaterial;
    
    RenderTexture _uvDilateRenderTexture;
    RenderTexture[] _uvMaskRenderTextures = new RenderTexture[2];
    int _currentRenderTextureIndex = 0;

    [SerializeField] Painter _painter;
    
    [Header("Debug Settings")]
    [SerializeField] bool _doFillPingPong = true;
    [SerializeField] bool _applyDilation = true;
    
    public class ShaderParamaters
    {
        public static int centerID = Shader.PropertyToID("_Center");
        public static int hardnessID = Shader.PropertyToID("_Hardness");
        public static int strengthID = Shader.PropertyToID("_Strength");
        public static int radiusID = Shader.PropertyToID("_Radius");
        public static int fillMultiplierID = Shader.PropertyToID("_FillMultiplier");
        public static int blendOpID = Shader.PropertyToID("_BlendOp");
    }

    void OnEnable() 
    {
        _uvDilateRenderTexture = new RenderTexture(TEXTURE_RESOLUTION, TEXTURE_RESOLUTION, 0);
        _uvDilateRenderTexture.filterMode = FilterMode.Bilinear;

        //Setup Texture Ping pong
        _uvMaskRenderTextures[0] = new RenderTexture(TEXTURE_RESOLUTION, TEXTURE_RESOLUTION, 0);
        _uvMaskRenderTextures[1] = new RenderTexture(TEXTURE_RESOLUTION, TEXTURE_RESOLUTION, 0);

        _renderer.material.SetTexture("_MaskTexture", _uvDilateRenderTexture);
    }

    void OnDisable()
    {
        _uvDilateRenderTexture.Release();
        _uvMaskRenderTextures[0].Release();
        _uvMaskRenderTextures[1].Release();
    }

    void Update()
    {
        //Set shader globals
        Vector4 center = (Vector4)_painter.transform.position;
        Shader.SetGlobalVector(ShaderParamaters.centerID, center);
        Shader.SetGlobalFloat(ShaderParamaters.hardnessID, _painter.hardness);
        Shader.SetGlobalFloat(ShaderParamaters.strengthID, _painter.strength);
        Shader.SetGlobalFloat(ShaderParamaters.radiusID, _painter.transform.localScale.x * 0.5f);
        Shader.SetGlobalFloat(ShaderParamaters.fillMultiplierID, _painter.fillMultiplier);
        
        if (_painter.mode == Painter.Mode.Additive)
        {
            Shader.SetGlobalInt(ShaderParamaters.blendOpID, (int)BlendOp.Add);
        }
        else if (_painter.mode == Painter.Mode.Subtractive)
        {
            Shader.SetGlobalInt(ShaderParamaters.blendOpID, (int)BlendOp.ReverseSubtract);
        }

        //Setup Command buffer
        RenderTexture uvMaskRenderTexture = _uvMaskRenderTextures[_currentRenderTextureIndex];
        int nextTextureIndex = (_currentRenderTextureIndex + 1) % _uvMaskRenderTextures.Length;
        CommandBuffer command = new CommandBuffer();
        command.name = "UV Space Renderer";
        if (_painter.gameObject.activeInHierarchy)
        {
            command.SetRenderTarget(uvMaskRenderTexture);
            command.DrawRenderer(_renderer, _uvMaskMaterial, 0);
        }

        if (_doFillPingPong)
        {
            command.Blit(uvMaskRenderTexture, _uvMaskRenderTextures[nextTextureIndex], _maskFillMaterial);
            _currentRenderTextureIndex = nextTextureIndex;
        }

        command.SetRenderTarget(_uvDilateRenderTexture);
        if (_applyDilation)
        {
            command.Blit(uvMaskRenderTexture, _uvDilateRenderTexture, _uvDilationMaterial);
        }
        else
        {
            command.Blit(uvMaskRenderTexture, _uvDilateRenderTexture);
        }

        Graphics.ExecuteCommandBuffer(command);
    }

    void OnDrawGizmos()
    {
        if (_painter == null || !_painter.gameObject.activeInHierarchy) return;
        Gizmos.color = Color.red * 0.8f;
        Gizmos.DrawWireSphere(_painter.transform.position, _painter.transform.lossyScale.x * 0.5f);
    }
}