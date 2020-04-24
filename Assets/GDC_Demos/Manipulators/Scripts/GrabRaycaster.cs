using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GrabRaycaster : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Manipulator _moveManipulator;
    [SerializeField] LayerMask _mask;
    Manipulator _currentManipulator = null;
    List<SpringData> _releasedManipulators = new List<SpringData>();
    [SerializeField] float _springStrength = 200.0f;
    [SerializeField] float _springDampening = 0.9f;
    [SerializeField] float _springDuration = 10.0f;
    [SerializeField] float _scalePullMultipler = 2.0f;
    [SerializeField] float _scalePullMaxDistance = 2.2f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = _camera.transform.position.z;
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _camera.focalLength, _mask))
            {
                if (_currentManipulator == null && _releasedManipulators.Count + 1 < ManipulatorManager.MAX_MANIPULATORS)
                {
                    Vector3 pos = _camera.ScreenToWorldPoint(screenPoint);
                    Quaternion rot = Quaternion.LookRotation(Vector3.forward, hit.normal); 
                    _currentManipulator = Instantiate<Manipulator>(_moveManipulator, pos, rot);
                    _currentManipulator.gameObject.SetActive(true);
                }
            }

            Debug.DrawRay(ray.origin, ray.direction * 100.0f, hit.transform ? Color.green : Color.red);

            if (_currentManipulator)
            {
                _currentManipulator.handle.position = _camera.ScreenToWorldPoint(screenPoint);

                //scale down handle as you pull farther away
                float maxDistance = _scalePullMaxDistance;
                float t = math.clamp(math.distancesq(_currentManipulator.handle.position, _currentManipulator.transform.position) / (maxDistance * maxDistance), 0.0f, 1.0f);

                _currentManipulator.handle.localScale = Vector3.one + (Vector3.one * t * _scalePullMultipler);
            }
        }

        if (Input.GetMouseButtonUp(0) && _currentManipulator)
        {
            SpringData springData = new SpringData
            {
                handle = _currentManipulator.handle,
                origin = _currentManipulator.transform,
                linearVelocity = Vector3.zero,
                scaleVelocity = Vector3.zero,
                startTime = Time.time    
            };

            _releasedManipulators.Add(springData);
            _currentManipulator = null;
        }

        for (int i = _releasedManipulators.Count - 1; i >= 0; i--)
        {
            SpringData springData = _releasedManipulators[i];
            if (Time.time > springData.startTime + _springDuration)
            {
                Destroy(_releasedManipulators[i].origin.gameObject);
                _releasedManipulators.RemoveAt(i);
                break;
            }

            _releasedManipulators[i].handle.position += springData.linearVelocity * Time.deltaTime;
            _releasedManipulators[i].handle.localScale += springData.scaleVelocity * Time.deltaTime;

            Vector3 linearAcceleration = (-_springStrength * (_releasedManipulators[i].handle.position - _releasedManipulators[i].origin.position)) * Time.deltaTime;
            springData.linearVelocity += linearAcceleration;
            springData.linearVelocity *= _springDampening;

            Vector3 scaleAcceleration = (-_springStrength * (_releasedManipulators[i].handle.localScale - Vector3.one)) * Time.deltaTime;
            springData.scaleVelocity += scaleAcceleration;
            springData.scaleVelocity *= _springDampening;

            _releasedManipulators[i] = springData;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float slowMotionTime = 0.2f;
            Time.timeScale = Time.timeScale == slowMotionTime ? 1.0f : slowMotionTime;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }
    }
}

struct SpringData
{
    public Transform handle;
    public Transform origin;
    public Vector3 linearVelocity;
    public Vector3 scaleVelocity;
    public float startTime;
}