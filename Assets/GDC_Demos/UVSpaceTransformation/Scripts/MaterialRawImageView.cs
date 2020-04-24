using UnityEngine;
using UnityEngine.UI;

public class MaterialRawImageView : MonoBehaviour
{
    [SerializeField] Renderer _renderer;
    [SerializeField] string _textureName;
    [SerializeField] RawImage _dilatedImage;
    
    void Start()
    {
        _dilatedImage.texture = _renderer.material.GetTexture(_textureName);
    }
}
