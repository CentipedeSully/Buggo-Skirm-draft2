using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerDrivenCreatureBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] protected IEntity _pursuedEntity;

    [TabGroup("Driver", "Driver")]
    [SerializeField] protected PlayerInput _playerInput;
    [TabGroup("Driver", "Driver")]
    [SerializeField] protected MapPointer _playerMapPointer;
    [TabGroup("Driver", "Driver")]
    [SerializeField] protected bool _controlEnabled = true;
    [TabGroup("Driver", "Driver")]
    [SerializeField] [ReadOnly] protected Vector3 _clickedPosition;
    [TabGroup("Driver", "Driver")]
    [SerializeField] [ReadOnly] protected GameObject _selectedObject;

    public delegate void InputDelegate();
    public event InputDelegate OnLeftClick;
    public event InputDelegate OnRightClick;
    public event InputDelegate OnMiddleClick;
    
    protected InputAction _lClickAction;
    protected InputAction _rClickAction;
    protected InputAction _mClickAction;



    public delegate void MoveDelegate();
    public event MoveDelegate OnPositionClicked;

    public delegate void SelectionDelegate();
    public event SelectionDelegate OnObjectSelected;

    private Dictionary<GameObject, float> _detectionsViaDistance = new();


    //monobehaviours
    private void OnEnable()
    {
        SubscribeUtilsToInputEvents();
        SubscribeUtilsToSelectionEvents();
        SubscribeUtilsToMoveEvents();
    }

    private void OnDisable()
    {
        UnsubFromAllEvents();
    }

    private void Update()
    {
        ListenForInput();
        ManageCreatureBehaviour();
    }




    //internals
    protected override void RunOtherUtilsOnAwake()
    {
        base.RunOtherUtilsOnAwake();

        //init the input references
        _lClickAction = _playerInput.actions.FindAction("LClick");
        _rClickAction = _playerInput.actions.FindAction("RClick");
        _mClickAction = _playerInput.actions.FindAction("MiddleClick");
    }

    private void SubscribeUtilsToInputEvents()
    {
        OnLeftClick += RespondToLeftClick;
        OnRightClick += RespondToRightClick;
        OnMiddleClick += RespondToMiddleClick;

        SubscribeOtherUtilsToInputEvents();

        //Test for the expected number of listeners. Make sure we aren't multisubscribing
        //Delegate[] Lsubs = OnLeftClick?.GetInvocationList();
        //Debug.Log($"Left Click subs onEnable: {Lsubs.Length}");

        //Delegate[] Rsubs = OnRightClick?.GetInvocationList();
        //Debug.Log($"Right Click subs onEnable: {Rsubs.Length}");

        //Delegate[] Msubs = OnMiddleClick?.GetInvocationList();
        //Debug.Log($"Middle Click subs onEnable: {Msubs.Length}");
    }
    protected virtual void SubscribeOtherUtilsToInputEvents() { }

    private void SubscribeUtilsToMoveEvents()
    {
        OnPositionClicked += EnterMovement;
    }
    private void SubscribeUtilsToSelectionEvents()
    {
        OnObjectSelected += EnterObjectPursuit;
    }


    private void UnsubscribeAllListeners(ref InputDelegate eventDelegate)
    {
        Delegate[] subscriptionList = eventDelegate?.GetInvocationList();

        if (subscriptionList == null)
            return;

        for (int i = subscriptionList.Length - 1; i >= 0; i--)
        {
            eventDelegate -= subscriptionList[i] as InputDelegate;
            Debug.Log($"Unsubscribed '{subscriptionList[i].Method.Name}' from event");
        }

        Delegate[] subs = eventDelegate?.GetInvocationList();
        if (subs != null)
            Debug.Log($"subs left: {subs.Length}");

    }
    private void UnsubscribeAllListeners(ref MoveDelegate eventDelegate)
    {
        Delegate[] subscriptionList = eventDelegate?.GetInvocationList();

        if (subscriptionList == null)
            return;

        for (int i = subscriptionList.Length - 1; i >= 0; i--)
        {
            eventDelegate -= subscriptionList[i] as MoveDelegate;
            Debug.Log($"Unsubscribed '{subscriptionList[i].Method.Name}' from event");
        }

        Delegate[] subs = eventDelegate?.GetInvocationList();
        if (subs != null)
            Debug.Log($"subs left: {subs.Length}");

    }
    private void UnsubscribeAllListeners(ref SelectionDelegate eventDelegate)
    {
        Delegate[] subscriptionList = eventDelegate?.GetInvocationList();

        if (subscriptionList == null)
            return;

        for (int i = subscriptionList.Length - 1; i >= 0; i--)
        {
            eventDelegate -= subscriptionList[i] as SelectionDelegate;
            Debug.Log($"Unsubscribed '{subscriptionList[i].Method.Name}' from event");
        }

        Delegate[] subs = eventDelegate?.GetInvocationList();
        if (subs != null)
            Debug.Log($"subs left: {subs.Length}");

    }
    private void UnsubFromAllEvents()
    {
        UnsubscribeAllListeners(ref OnLeftClick);
        UnsubscribeAllListeners(ref OnRightClick);
        UnsubscribeAllListeners(ref OnMiddleClick);
        UnsubscribeAllListeners(ref OnObjectSelected);
        UnsubscribeAllListeners(ref OnPositionClicked);
    }

    protected void SubscribeToInputEvent(ref InputDelegate inputEvent, InputDelegate newListener)
    {
        if (newListener == null)
            return;

        if (!IsFunctionAlreadySubscribed(newListener, inputEvent))
            inputEvent += newListener;
        else
            Debug.LogWarning($"Attempted To Subscribe {newListener.Method.Name} but is already subscribed");
    }
    protected bool IsFunctionAlreadySubscribed(InputDelegate function, InputDelegate inputEvent)
    {
        Delegate[] subsList = inputEvent?.GetInvocationList();

        if (subsList == null)
            return false;

        else
        {
            for (int i = 0; i < subsList.Length; i++)
            {
                if (subsList[i].Method == function.Method)
                    return true;
            }

            return false;
        }
    }



    protected void ListenForInput()
    {
        if (_lClickAction.phase == InputActionPhase.Performed)
            OnLeftClick?.Invoke();

        if (_rClickAction.phase == InputActionPhase.Performed)
            OnRightClick?.Invoke();

        if (_mClickAction.phase == InputActionPhase.Performed)
            OnMiddleClick?.Invoke();
    }
    protected void RespondToLeftClick() 
    {
        Debug.Log("LClick Detected");
        CapturePointerDetection();
    }
    protected void RespondToRightClick() 
    { 
        Debug.Log("RClick Detected");

    }
    protected void RespondToMiddleClick() 
    { 
        Debug.Log("MiddleClick Detected");

    }

    protected void CapturePointerDetection(bool ignoreEntities =false)
    {

        RaycastHit[] hits = _playerMapPointer.CaptureDetectionsOnPointer();

        if (ignoreEntities)
            CollectGroundDetectionData(hits);
        else
        {
            //prioritize detecting hostile entities
            CollectObjectDetectionData(hits);

            //otherwise, detect the ground
            if (_detectionsViaDistance.Count == 0)
                CollectGroundDetectionData(hits);
        }



    }
    private void CollectObjectDetectionData(RaycastHit[] hits)
    {
        _detectionsViaDistance.Clear();

        if (hits.Length == 0)
            return;

        //Only save valid entities
        foreach (RaycastHit hit in hits)
        {
            IEntity possibleEntity = hit.collider.GetComponent<IEntity>();

            if (possibleEntity != null)
            {
                //calculate the distance from the camera to this object
                float distanceFromPointerOrigin = _playerMapPointer.GetDistanceFromCamera(possibleEntity.GetGameObject());

                //save the detection data
                _detectionsViaDistance.Add(possibleEntity.GetGameObject(), distanceFromPointerOrigin);
            }
        }

        //select the one closest to the camera, if any exist
        if (_detectionsViaDistance.Count > 0)
        {
            _selectedObject = GetClosestDetection();
            _clickedPosition = _selectedObject.transform.position;

            //a valid entity object has been selected
            OnObjectSelected?.Invoke();
        }
    }
    private GameObject GetClosestDetection()
    {
        float closestDistance = float.MaxValue;
        GameObject closestObject = null;

        foreach (KeyValuePair<GameObject, float> entry in _detectionsViaDistance)
        {
            if (entry.Value < closestDistance)
            {
                closestDistance = entry.Value;
                closestObject = entry.Key;
            }

        }

        return closestObject;
    }
    private void CollectGroundDetectionData(RaycastHit[] hits)
    {
        _detectionsViaDistance.Clear();

        if (hits.Length == 0)
            return;

        //save the first valid ground position that's been detected
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                Debug.Log($"collider object: {hit.collider.name}");
                _selectedObject = null;
                _clickedPosition = new Vector3(hit.point.x,transform.position.y, hit.point.z);


                //the map has been clicked
                OnPositionClicked?.Invoke();
                return;
            }
        }
    }


    protected override void ManageCreatureBehaviour()
    {
        switch (_currentState)
        {
            case CreatureState.unset:
                break;
            case CreatureState.Idling:
                break;

            case CreatureState.MovingToObjective:
                WatchForMovementCompletion();
                break;

            case CreatureState.PursuingEntity:
                PursueEntity();
                break;

            case CreatureState.EnteringAction:
                break;
            case CreatureState.PerformingAction:
                break;
            case CreatureState.RecoveringFromAction:
                break;
            case CreatureState.Stunned:
                break;
            case CreatureState.Dead:
                break;
        }
    }


    protected override void ClearTargetingData()
    {
        _pursuedEntity = null;
        base.ClearTargetingData();
    }
    protected override void PursueEntity()
    {
        Debug.Log($"PursueEntity Behaviour: {this.name}");
        if (IsPursuitTargetStillValid())
        {
            UpdateTargetingData();

            //toggle drawing the pursuit line
            _isInValidPursuit = true;

            if (IsEntityInRangeForInteraction())
            {
                EndCurrentMovement();
                PerformEntityBasedInteraction();
            }

            else
                GetWithinRangeOfEntity();
        }

        //otherwise go idle.
        else
        {
            //toggle hiding the pursuit line
            _isInValidPursuit = false;

            ClearTargetingData();

            _selectedObject = null;
            ChangeState(CreatureState.Idling);
        }
    }

    protected void EnterMovement()
    {
        if (IsMovementAvailable())
        {
            if (_currentState == CreatureState.PursuingEntity)
                ClearTargetingData();

            _navAgent.ResetPath();
            _navAgent.SetDestination(_clickedPosition);
            ChangeState(CreatureState.MovingToObjective);
        }
    }
    protected bool IsMovementAvailable()
    {
        return _currentState != CreatureState.Dead &&
            _currentState != CreatureState.EnteringAction &&
            _currentState != CreatureState.PerformingAction &&
            _currentState != CreatureState.RecoveringFromAction &&
            _currentState != CreatureState.Stunned;
    }
    protected void WatchForMovementCompletion()
    {
        if (_navAgent.remainingDistance <= _closeEnoughDistance)
        {
            EndMovement();
            ChangeState(CreatureState.Idling);
        }
    }
    protected void EndMovement()
    {
        _navAgent.ResetPath();
    }

    protected void EnterObjectPursuit()
    {
        if (IsMovementAvailable())
        {
            IEntity entity = _selectedObject.GetComponent<IEntity>();

            switch (entity.GetEntityType())
            {
                case EntityType.creature:
                    //only pursue hostile creatures
                    if (entity.GetFaction() != _faction)
                    {
                        _pursuitTarget = _selectedObject;
                        _pursuedEntity = entity;
                        ChangeState(CreatureState.PursuingEntity);
                    }
                    break;

                case EntityType.structure:
                    //only pursue hostile creatures
                    if (entity.GetFaction() != _faction)
                    {
                        _pursuitTarget = _selectedObject;
                        _pursuedEntity = entity;
                        ChangeState(CreatureState.PursuingEntity);
                    }
                        

                    break;

                case EntityType.pickup:
                    _pursuitTarget = _selectedObject;
                    _pursuedEntity = entity;
                    ChangeState(CreatureState.PursuingEntity);
                    break;



                //Ignore the selection if we cant target the thing
                default:
                    break;
            }
            
        }
    }


    //externals
    public override void SetObjective(Transform newObjective)
    {
        _selectedObject = newObjective.gameObject;
        OnObjectSelected?.Invoke();
    }



    //Debug


}
