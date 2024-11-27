using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


public enum CreatureState
{
    unset,
    Idling,
    MovingToObjective,
    PursuingEntity,
    EnteringAction,
    PerformingAction,
    RecoveringFromAction,
    Stunned,
    Dead
}

public interface IDamageable
{

    GameObject GetGameObject();
    Faction GetFaction();
    int GetEntityID();
    void TakeDamage(int damage);
    bool IsDead();
}

public interface IAttacker
{
    GameObject GetGameObject();
    Faction GetFaction();
    int GetEntityID();
}

public interface IEntity
{
    int GetEntityID();
    Faction GetFaction();
    GameObject GetGameObject();
}

public abstract class AbstractCreatureBehaviour : SerializedMonoBehaviour, IDamageable, IAttacker, IEntity
{
    //Delcarations
    [TabGroup("Core","Info")]
    [SerializeField] [ReadOnly] protected int _entityID;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected CreatureType _creatureType = CreatureType.unset;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected Faction _faction = Faction.unset;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected CreatureState _currentState = CreatureState.unset;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected string _currentActionName;
    private string _defaultActionName = "None";



    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] protected Transform _currentObjective;
    protected Vector3 _objectiveDestination;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected bool _isObjectivePathingInterruptable = true;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected LayerMask _detectionLayerMask;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected float _detectionRadius = 3;
    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] protected GameObject _pursuitTarget;
    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] protected Vector3 _pursuitTargetPosition;
    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] protected float _distanceFromPursuitTarget;
    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] protected Vector3 _targetDirection;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected Color _pursuitPointerColor;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected Color _attackLineColor;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected bool _showPursuitLine = false;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected bool _showAttackLine = false;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected bool _isInValidPursuit = false;


    [TabGroup("Core", "Movement")]
    [SerializeField] protected int _baseSpeed;
    [TabGroup("Core", "Movement")]
    [SerializeField] protected float _baseTurnSpeed;
    [TabGroup("Core", "Movement")]
    [SerializeField] protected float _closeEnoughDistance = .2f;


    [TabGroup("Core", "Combat")]
    [SerializeField] protected int _currentHp;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected int _maxHp;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected int _damage;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected float _cooldown;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected IAttack _coreAtk;
    [TabGroup("Core", "Combat")]
    [SerializeField] [ReadOnly] protected Dictionary<ResourceType, int> _currentCorpseYield = new();


    [TabGroup("Core", "Death")]
    [SerializeField] protected float _despawnTime = .5f;



    protected NavMeshAgent _navAgent;

    public delegate void StateChangeFunction(CreatureState newState, string actionName);
    public event StateChangeFunction OnStateChanged;



    //Monobehaviours
    private void Awake()
    {
        RunOnAwakeUtils();
    }

    private void OnEnable()
    {
        OnStateChanged += ListenForDeath;
        SubscribeToComponentEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromComponentEvents();
        UnsubscribeAllFromOnStateChanged();
    }

    private void Update()
    {
        ManageCreatureBehaviour();
        
    }

    private void OnDrawGizmos()
    {
        DrawPursuitLine();
        DrawAttackLine();
    }



    //internals
    private void RunOnAwakeUtils()
    {
        _entityID = GetInstanceID();
        _currentActionName = _defaultActionName;
        InitializeUtilsOnAwake();
        ReadData();
        RunOtherUtilsOnAwake();
    }

    protected virtual void InitializeUtilsOnAwake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
    }
    protected abstract void ReadData();
    protected virtual void RunOtherUtilsOnAwake() { }


    protected virtual void SubscribeToComponentEvents()
    {
        _coreAtk.SubscribeToStateChange(UpdateStateViaAtkStateChangeEvent);
    }
    protected virtual void UnsubscribeFromComponentEvents()
    {
        _coreAtk.UnsubscribeFromStateChange(UpdateStateViaAtkStateChangeEvent);
    }


    protected void SubscribeToStateChange(StateChangeFunction function)
    {
        if (!IsFunctionAlreadySubscribedToOnStateChanged(function))
            OnStateChanged += function;
        else 
            Debug.LogWarning($" Ignoring subscription request for function {function.Method.Name}. Method already subscribed to event.");
    }
    protected bool IsFunctionAlreadySubscribedToOnStateChanged(StateChangeFunction function)
    {
        Delegate[] subsList = OnStateChanged?.GetInvocationList();

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
    protected void UnsubscribeFromStateChange(StateChangeFunction function)
    {
        OnStateChanged -= function;
    }
    private void UnsubscribeAllFromOnStateChanged()
    {
        Delegate[] subscriptionList = OnStateChanged?.GetInvocationList();

        if (subscriptionList != null)
        {
            for (int i = subscriptionList.Length - 1; i >= 0; i--)
            {
                OnStateChanged -= subscriptionList[i] as StateChangeFunction;
                //Debug.Log($"Unsubscribed '{subscriptionList[i].Method.Name}' from OnStateChanged event");
            }
        }
        
    }


    private void UpdateStateViaAtkStateChangeEvent(AtkState state, string actionName)
    {
        switch (state)
        {
            case AtkState.NotAtking:
                ChangeState(CreatureState.Idling, actionName);
                break;

            case AtkState.PreparingAtk:
                ChangeState(CreatureState.EnteringAction, actionName);
                break;

            case AtkState.CastingAtk:
                ChangeState(CreatureState.PerformingAction, actionName);
                break;

            case AtkState.RecovingFromAtk:
                ChangeState(CreatureState.RecoveringFromAction, actionName);
                break;

            case AtkState.CoolingAtk:
                ChangeState(CreatureState.PursuingEntity, actionName);
                break;

            default:
                break;
        }
    }


    protected void ChangeState(CreatureState newState, string actionName)
    {
        if (newState != _currentState && newState != CreatureState.unset)
        {
            _currentState = newState;
            UpdateActionName(actionName);
            OnStateChanged?.Invoke(_currentState, _currentActionName);
        }
    }
    protected void ChangeState(CreatureState newState)
    {
        ChangeState(newState, "");
    }
    private void UpdateActionName(string desiredName)
    {
        //default the name if it's empty or null
        if (desiredName == "" || desiredName == null)
            _currentActionName = _defaultActionName;
        else
            _currentActionName = desiredName;
    }



    private void ListenForDeath(CreatureState state, string actionName)
    {
        if (_currentState == CreatureState.Dead)
        {
            CreateCorpseYield();
            EndCurrentMovement();
            _coreAtk?.InterruptAttack();
            RunOtherUtilsOnDeath();

            //Be sure to despawn if the creature yields no resources
            if (_currentCorpseYield.Count == 0)
                DespawnAfterDelay();
        }
    }
    protected abstract void CreateCorpseYield();
    protected virtual void RunOtherUtilsOnDeath() { }
    protected void DespawnAfterDelay()
    {
        Invoke(nameof(Despawn), _despawnTime);
    }
    protected void Despawn()
    {
        Destroy(gameObject);
    }


    protected void UpdateObjectiveDestination()
    {
        if (_currentObjective != null)
            _objectiveDestination = _currentObjective.position;

        else 
            _objectiveDestination = transform.position;

        _navAgent.SetDestination(_objectiveDestination);
    }
    protected virtual void ApproachObjective()
    {
        //track (and path towards) the objective's position this frame (if it exists)
        UpdateObjectiveDestination();

        //remain vigilant if our focus is interruptable
        if (_isObjectivePathingInterruptable)
            DetectPursuableEntities();

        //end the movement if we're close enough to the objective (assuming the pathing is completed)
        if (_navAgent.pathPending)
            return;

        else if (_navAgent.remainingDistance <= _closeEnoughDistance)
        {
            EndCurrentMovement();
            _currentObjective = null;
            ChangeState(CreatureState.Idling);
        }

    }
    protected void EndCurrentMovement()
    {
        if (_navAgent != null)
            _navAgent.ResetPath();
    }


    protected virtual void DetectPursuableEntities()
    {
        Collider[] detections = Physics.OverlapSphere(transform.position,_detectionRadius, _detectionLayerMask);

        foreach (Collider detection in detections)
        {
            if (IsDetectedEntityValid(detection))
            {
                _pursuitTarget = detection.gameObject;
                ChangeState(CreatureState.PursuingEntity);
            }
        }
    }

    protected abstract bool IsDetectedEntityValid(Collider detection);

    private void DrawPursuitLine()
    {
        if (_isInValidPursuit && _showPursuitLine)
        {
            Gizmos.color = _pursuitPointerColor;
            Gizmos.DrawLine(transform.position, transform.position + _targetDirection);
        }
    }
    private void DrawAttackLine()
    {
        if (_coreAtk != null && _showAttackLine)
        {
            Gizmos.color = _attackLineColor;
            Gizmos.DrawLine(transform.position, transform.position + (_coreAtk.GetAtkDirection() * _coreAtk.GetMaxAtkRange()));
        }
    }

    private void UpdateTargetingData()
    {
        //ignore the y axis
        _pursuitTargetPosition = new Vector3(_pursuitTarget.transform.position.x,0, _pursuitTarget.transform.position.z);
        Vector3 currentPositionNoY = new Vector3(transform.position.x, 0, transform.position.z);
        
        //update distance
        _distanceFromPursuitTarget = Vector3.Distance(currentPositionNoY, _pursuitTargetPosition);

        //calculate the target's local position in repect to our position
        _targetDirection = _pursuitTargetPosition - currentPositionNoY;
    }

    private void ClearTargetingData()
    {
        _pursuitTarget= null;
        _pursuitTargetPosition = Vector3.zero;
        _distanceFromPursuitTarget = 0;
        _targetDirection= Vector3.zero;
    }

    protected void PursueEntity()
    {
        //get at the target object if it still exists
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
        
        //otherwise, either resume moving to the objective or idle.
        else
        {

            //toggle hiding the pursuit line
            _isInValidPursuit = false;

            ClearTargetingData();

            if (_currentObjective != null)
                ChangeState(CreatureState.MovingToObjective);
            else
                ChangeState(CreatureState.Idling);
        }
    }
    protected abstract bool IsEntityInRangeForInteraction();
    protected abstract void PerformEntityBasedInteraction();
    protected abstract void GetWithinRangeOfEntity();
    protected abstract bool IsPursuitTargetStillValid();


    protected void ManageCreatureBehaviour()
    {
        switch (_currentState)
        {
            case CreatureState.Idling:

                //continue questing towards the objective, if we have one
                if (_currentObjective != null)
                    ApproachObjective();

                //else remain vigilant
                else
                    DetectPursuableEntities();
                
                break;


            case CreatureState.MovingToObjective:

                //Move towards the current objective until the objective is reached
                ApproachObjective();
                break;


            case CreatureState.PursuingEntity:
                
                //Pursue the entity, whatever it is, however we're capable of pursuing it
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



            default:
                break;
        }
            
    }




    //Externals
    public void SetFaction(Faction newFaction)
    {
        if (_faction != newFaction)
            _faction = newFaction;
    }
    public Faction GetFaction() { return _faction; }
    public GameObject GetGameObject() { return gameObject; }
    public int GetEntityID() { return _entityID; }

    [BoxGroup("Debug")]
    [Button]
    public void TakeDamage(int damage)
    {
        if (_currentState != CreatureState.Dead)
        {
            _currentHp -= damage;

            if (_currentHp <= 0)
            {
                ChangeState(CreatureState.Dead);
            }
        }
    }

    [BoxGroup("Debug")]
    [Button]
    public void Die()
    {
        if (_currentState != CreatureState.Dead)
        {
            _currentHp = 0;
            ChangeState(CreatureState.Dead);
        }

    }

    [BoxGroup("Debug")]
    [Button]
    public void SetObjective(Transform objectiveTransform)
    {
        if (_currentState != CreatureState.Dead && _navAgent != null && objectiveTransform != null)
        {
            _currentObjective = objectiveTransform;
            ChangeState(CreatureState.MovingToObjective);
        }
    }

    [BoxGroup("Debug")]
    [Button]
    public Dictionary<ResourceType, int> CollectAnyResourcesFromCorpse(Dictionary<ResourceType, int> decrementList)
    {
        if (_currentState == CreatureState.Dead && decrementList != null)
        {
            Dictionary<ResourceType, int> returnsList = new Dictionary<ResourceType, int>();

            foreach (ResourceType desiredResource in decrementList.Keys)
            {
                //does this resource exist on the corpse?
                if (_currentCorpseYield.ContainsKey(desiredResource))
                {
                    int requestedAmount = decrementList[desiredResource];
                    int existingAmount = _currentCorpseYield[desiredResource];

                    //is there enough to fulfill the request?
                    if (requestedAmount <= existingAmount)
                    {
                        //remove the full amount from the corpse
                        _currentCorpseYield[desiredResource] = existingAmount - requestedAmount;

                        //add the taken amount to the returnsList
                        returnsList.Add(desiredResource, requestedAmount);
                    }

                    //take what we can
                    else
                    {
                        //remove everything
                        _currentCorpseYield[desiredResource] = 0;

                        //add what was there to the returnsList
                        returnsList.Add(desiredResource, existingAmount);
                    }

                    //remove the key if this resource is all gone
                    if (_currentCorpseYield[desiredResource] == 0)
                        _currentCorpseYield.Remove(desiredResource);
                }
            }

            if (_currentCorpseYield.Count == 0)
                DespawnAfterDelay();

            return returnsList;
        }

        else
            return null;
    }

    public bool IsDead() { return _currentState == CreatureState.Dead; }
}
