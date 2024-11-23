using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum CreatureState
{
    unset,
    Idling,
    Moving,
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

public abstract class AbstractCreatureBehaviour : SerializedMonoBehaviour, IDamageable, IAttacker
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

    [TabGroup("Core", "General")]
    [SerializeField] protected int _currentHp;
    [TabGroup("Core", "General")]
    [SerializeField] protected int _maxHp;
    [TabGroup("Core", "General")]
    [SerializeField] protected int _baseSpeed;
    [TabGroup("Core", "Movement")]
    [SerializeField] protected float _closeEnoughDistance = .2f;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected IAttack _coreAtk;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected int _damage;
    [TabGroup("Core", "Combat")]
    [SerializeField] protected float _cooldown;
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
        if (_currentState == CreatureState.Moving)
            WatchForMovementCompletion();
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
                ChangeState(CreatureState.Idling, actionName);
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


    protected virtual void EndCurrentMovement()
    {
        if (_navAgent != null)
            _navAgent.ResetPath();
    }
    protected virtual void WatchForMovementCompletion()
    {
        if (_navAgent.remainingDistance <= _closeEnoughDistance)
        {
            EndCurrentMovement();
            ChangeState(CreatureState.Idling);
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
    public abstract void CommandMovementToPosition(Vector3 position);

    [BoxGroup("Debug")]
    [Button]
    public void CommandMovementToPosition(Transform transform)
    {
        CommandMovementToPosition(transform.position);
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
