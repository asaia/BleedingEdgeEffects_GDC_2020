using UnityEngine;
using UnityEngine.Rendering;

public class Painter : MonoBehaviour
{
    public enum Mode { Additive, Subtractive };
    public Mode mode;
    [Range(0, 1f)]
    public float hardness = 0.2f;
    [Range(0, 1f)]
    public float strength = 0.25f;
    [Range(0.0f, 2.0f)]
    public float fillMultiplier = 1.0f;
}
