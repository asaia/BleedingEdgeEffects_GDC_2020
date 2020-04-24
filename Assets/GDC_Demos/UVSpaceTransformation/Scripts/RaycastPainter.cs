using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPainter : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Painter _painter;
    
    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Vector3 position = (Vector3)Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10.0f))
            {
                _painter.transform.position = hit.point;
            }

            bool didHit = hit.transform != null;

            _painter.gameObject.SetActive(didHit);
            if (Input.GetMouseButton(0))
            {
                _painter.mode = Painter.Mode.Additive;
            }
            else if (Input.GetMouseButton(1))
            {
                _painter.mode = Painter.Mode.Subtractive;
            }

            if (didHit)
            {
                Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.red);
            }
        }
        else
        {
            _painter.gameObject.SetActive(false);
        }
    }
}
