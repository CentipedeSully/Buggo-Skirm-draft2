using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

public abstract class AbstractCreatureBehaviour : MonoBehaviour
{
    //Delcarations
    [TabGroup("Core","Info")]
    [SerializeField] [ReadOnly] protected int _entityID;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected CreatureType _creatureType;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected CreatureState _currentState;
    [TabGroup("Core", "Info")]
    [SerializeField] [ReadOnly] protected string _currentActionName;
    private string _defaultActionName = "None";


    [TabGroup("Core", "General")]
    [SerializeField] protected int _currentHp;
    [TabGroup("Core", "General")]
    [SerializeField] protected int _maxHp;
    [TabGroup("Core", "General")]
    [SerializeField] protected int _baseSpeed;





    public delegate void StateChangeFunction(CreatureState newState);
    public event StateChangeFunction OnStateChanged;



    //Monobehaviours
    private void Awake()
    {
        RunOnAwakeUtils();
    }

    private void OnDisable()
    {
        UnsubscribeAllUtilities();
    }



    //internals
    private void RunOnAwakeUtils()
    {
        _entityID = GetInstanceID();
        _currentActionName = _defaultActionName;
        ReadData();
        RunOtherUtilsOnAwake();
    }
    protected abstract void ReadData();
    protected virtual void RunOtherUtilsOnAwake() { }

    protected void SubscribeToStateChange(StateChangeFunction function)
    {
        if (!IsFunctionAlreadySubscribed(function))
            OnStateChanged += function;
        else 
            Debug.LogWarning($" Ignoring subscription request for function {function.Method.Name}. Method already subscribed to event.");
    }

    private bool IsFunctionAlreadySubscribed(StateChangeFunction function)
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

    private void UnsubscribeAllUtilities()
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


    protected void ChangeState(CreatureState newState, string actionName = "undefined")
    {
        if (newState != _currentState && newState != CreatureState.unset)
        {
            _currentState = newState;
            UpdateActionName(actionName);
            OnStateChanged?.Invoke(_currentState);
        }
    }

    private void UpdateActionName(string desiredName)
    {
        //defaults the name, if it's undefined
        if (desiredName == "undefined" || desiredName == "" || desiredName == null)
            _currentActionName = _defaultActionName;
        else
            _currentActionName = desiredName;
    }

    

    //Externals



}
