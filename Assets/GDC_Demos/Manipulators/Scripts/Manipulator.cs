using UnityEngine;

[ExecuteInEditMode()]
public class Manipulator : MonoBehaviour
{
    [Range(0f, 1f)]
    public float hardness = .8f;
    public Transform handle;
    ManipulatorManager _manager;

    void OnEnable() 
    {
        _manager = FindObjectOfType<ManipulatorManager>();
        if (_manager != null)
        {
            _manager.Add(this);
        }
    }

    void OnDisable() 
    {
        _manager.Remove(this);
    }
    
    void OnDrawGizmos()
    {
        if (handle == null) return;
        Gizmos.color = Color.red * 0.8f;
        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.magnitude);

		Gizmos.DrawLine(transform.position, handle.position);
        Gizmos.color *= .4f;
        Gizmos.DrawWireSphere(handle.position, handle.lossyScale.magnitude);
    }
}
