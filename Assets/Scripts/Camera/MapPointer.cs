using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Sirenix.OdinInspector;


public class MapPointer : MonoBehaviour
{
    //Declarations
    [SerializeField] private Camera _mapCamera;
    [SerializeField] private float _castDistance = 200;
    [SerializeField] [ReadOnly] private float _detectionDistance = -1;
    [SerializeField] private LayerMask _layermask;
    [SerializeField] private List<GameObject> _detectedObjects;
    [SerializeField] private Color _pointerColor;
    [SerializeField] [ReadOnly] private Vector2 _mousePosition;
    private Ray _castRay;




    //Monobehaviours
    private void Update()
    {
        DetectHoveredObjects();
    }

    private void OnDrawGizmos()
    {
        DrawPointerGizmo();
    }




    //Internals
    private void DetectHoveredObjects()
    {
        _detectedObjects.Clear();

        RaycastHit[] detections = CastDetections();

        foreach (RaycastHit hit in detections)
            _detectedObjects.Add(hit.collider.gameObject);

    }

    private void BuildCastRay()
    {
        _mousePosition = Mouse.current.position.ReadValue();
        _castRay = _mapCamera.ScreenPointToRay(_mousePosition);
    }

    private RaycastHit[] CastDetections()
    {
        BuildCastRay();
        return Physics.RaycastAll(_castRay, _castDistance, _layermask);
    }

    private void DrawPointerGizmo()
    {
        Gizmos.color = _pointerColor;
        Gizmos.DrawLine(_castRay.origin,_castRay.direction * _castDistance);
    }


    //Externals
    public RaycastHit[] CaptureDetectionsOnPointer()
    {
        return CastDetections();
    }

    public float GetDistanceFromCamera(GameObject queryObject)
    {
        if (queryObject == null)
            return -1;

        else
        {
            BuildCastRay();
            return (Vector3.Distance(_castRay.origin, queryObject.transform.position));
        }
    }




}
