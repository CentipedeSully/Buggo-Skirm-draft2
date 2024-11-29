using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //Declarations

    [TabGroup("Core", "Setup")]
    [SerializeField] private Camera _mapCamera;
    [TabGroup("Core", "Setup")]
    [SerializeField] private PlayerInput _playerInputComponent;
    [TabGroup("Core", "Setup")]
    [SerializeField] private string _depthActionName = "Adjust Camera Depth";
    [TabGroup("Core", "Setup")]
    [SerializeField] private string _positionActionName = "Adjust Camera Position";
    [TabGroup("Core", "Setup")]
    [SerializeField] private string _rotationActionName = "Adjust Camera Angle";
    [TabGroup("Core", "Setup")]
    [SerializeField] private float _camDepthSpeed = 2;
    [TabGroup("Core", "Setup")]
    [SerializeField] private float _camMoveSpeed = 5;
    [TabGroup("Core", "Setup")]
    [SerializeField] private float _camRotateSpeed = 90;
    [TabGroup("Core", "Setup")]
    [SerializeField] private Vector3 _relativeCamAxisX;
    [TabGroup("Core", "Setup")]
    [SerializeField] private Vector3 _relativeCamAxisZ;
    [TabGroup("Core", "Setup")]
    [SerializeField] private float _maxCamDistance = 20;
    [TabGroup("Core", "Setup")]
    [SerializeField] private float _minCamDistance = 4;

    [TabGroup("Core", "Setup")]
    [SerializeField] private bool _invertZoomControl = true;
    [TabGroup("Core", "Setup")]
    [SerializeField] private bool _invertRotationControl = false;


    private InputAction _changeDepthAction;
    private InputAction _changePositionAction;
    private InputAction _changeRotationAction;
    private int _zoomInvertValue;
    private int _rotationInvertValue;
    private Vector3 _rotationAxis = Vector3.up;


    [TabGroup("Core", "Status")]

    [SerializeField] [ReadOnly]private bool _errorDetected = false;
    [TabGroup("Core", "Status")]

    [SerializeField] private bool _isCamControlEnabled = true;
    [TabGroup("Core", "Status")]
    [SerializeField] [ReadOnly] private Vector3 _originalRotation;
    [TabGroup("Core", "Status")]
    [SerializeField] [ReadOnly] private Vector3 _instanceHorizontalAxis;
    [TabGroup("Core", "Status")]
    [SerializeField][ReadOnly] private Vector3 _instanceVerticalAxis;
    [TabGroup("Core", "Status")]
    [SerializeField][ReadOnly] private Vector3 _zoomAxis;
    [TabGroup("Core", "Status")]
    [SerializeField] [ReadOnly] private Vector2 _zoomInput;
    [TabGroup("Core", "Status")]
    [SerializeField] [ReadOnly] private Vector2 _positionInput;
    [TabGroup("Core", "Status")]
    [SerializeField] [ReadOnly] private float _rotationInput;
    [TabGroup("Core", "Status")]
    [SerializeField][ReadOnly] private float _currentDistance;






    //Monobehaviours
    private void Awake()
    {
        InitializeUtils();
    }

    private void Update()
    {
        ReadInput();

        if (_isCamControlEnabled)
            ControlCamera();
    }


    //Internals
    private void InitializeUtils()
    {

        //save the original rotation
        _originalRotation = transform.localRotation.eulerAngles;

        //calculate or instance movement axis
        CalculateCameraMovementAxes();

        //validate the input actions
        if (_playerInputComponent != null)
        {
            _changeDepthAction = _playerInputComponent.actions.FindAction(_depthActionName);
            _changePositionAction = _playerInputComponent.actions.FindAction(_positionActionName);
            _changeRotationAction = _playerInputComponent.actions.FindAction(_rotationActionName);

            if (_changeDepthAction == null)
            {
                Debug.LogError("Camera Depth Input Action is missing from actionMap");
                _errorDetected = true;
            }
                
            if (_changePositionAction == null)
            {
                Debug.LogError("Camera Position Input Action is missing from actionMap");
                _errorDetected = true;
            }

            if (_changeRotationAction == null)
            {
                Debug.LogError("Camera Rotation Input Action is missing from actionMap");
                _errorDetected = true;
            }
                
        }

        else
        {
            Debug.LogError("Input Asset Reference Missing.");
            _errorDetected = true;
        }

    }

    private void CalculateCameraMovementAxes()
    {
        //initialize the current distance from the camera
        _currentDistance = Vector3.Distance(_mapCamera.transform.position, transform.position);

        //initialize the zoom axis
        _zoomAxis = transform.position - _mapCamera.transform.position;

        //calcualte x axis
        _instanceHorizontalAxis = transform.TransformDirection(_relativeCamAxisX);

        //calculate z axis
        _instanceVerticalAxis = transform.TransformDirection(_relativeCamAxisZ);
    }

    private void ReadInput()
    {
        if (!_errorDetected)
        {
            //read current zoom input
            if (_changeDepthAction.phase == InputActionPhase.Performed ||
                _changeDepthAction.phase == InputActionPhase.Started)
            {
                _zoomInput = _changeDepthAction.ReadValue<Vector2>();
            }
                
            else _zoomInput = Vector2.zero;


            //read current camMovement input
            if (_changePositionAction.phase == InputActionPhase.Performed ||
                _changePositionAction.phase == InputActionPhase.Started)
            {
                _positionInput = _changePositionAction.ReadValue<Vector2>();
            }
                
            else _positionInput = Vector2.zero;


            //read current camRotation input
            if (_changeRotationAction.phase == InputActionPhase.Started ||
                _changeRotationAction.phase == InputActionPhase.Performed)
            {
                _rotationInput = _changeRotationAction.ReadValue<float>();
            }

            else _rotationInput = 0;
        }
    }

    private void ControlCamera()
    {

        //invert zoom if necessary
        if (_invertZoomControl)
            _zoomInvertValue = -1;
        else 
            _zoomInvertValue = 1;

        _zoomInput.y = _zoomInput.y * _zoomInvertValue;


        //invert rotation if necessary
        if(_invertRotationControl)
            _rotationInvertValue = -1;
        else 
            _rotationInvertValue = 1;

        _rotationInput = _rotationInput * _rotationInvertValue;



        //update camera zoom
        if (_zoomInput.y != 0)
        {
            Vector3 zoomOffset = _camDepthSpeed * Mathf.Sign(_zoomInput.y) * Time.deltaTime * _zoomAxis.normalized;

            if (_currentDistance < _maxCamDistance && zoomOffset.y > 0 ||
                _currentDistance > _minCamDistance && zoomOffset.y < 0)
            {
                //zoom in/out the camera
                _mapCamera.transform.position += zoomOffset;

                //update the camera's distance
                _currentDistance = Vector3.Distance(_mapCamera.transform.position,transform.position);
            }
            
        }


        //update camera horizontal position
        if (_positionInput.x != 0)
        {
            Vector3 hMoveOffset = _camMoveSpeed * Mathf.Sign(_positionInput.x) * Time.deltaTime * _instanceHorizontalAxis.normalized;
            transform.localPosition = transform.localPosition + hMoveOffset;
        }
        

        //update camera vertical position
        if (_positionInput.y != 0)
        {
            Vector3 vOffset = _camMoveSpeed * Mathf.Sign(_positionInput.y) * Time.deltaTime * _instanceVerticalAxis.normalized;
            transform.localPosition = transform.localPosition + vOffset;
        }

        //update camera rotation
        if (_rotationInput != 0)
        {
            Vector3 angleOffset = _camRotateSpeed * Mathf.Sign(_rotationInput) * Time.deltaTime * _rotationAxis;
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + angleOffset);

            //recalculate the movement angles to relate to the new rotation
            CalculateCameraMovementAxes();
        }

    }


    //Externals
    [BoxGroup("Debug")]
    [Button]
    public void ResetCameraRotation()
    {
        _isCamControlEnabled = false;
        transform.localRotation = Quaternion.Euler(_originalRotation);

        CalculateCameraMovementAxes();
        _isCamControlEnabled = true;
    }





}
