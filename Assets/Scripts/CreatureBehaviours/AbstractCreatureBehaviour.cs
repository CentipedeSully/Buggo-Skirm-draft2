using Sirenix.OdinInspector;
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

    [TabGroup("Core", "General")]
    [SerializeField] protected int _currentHp;
    [TabGroup("Core", "General")]
    [SerializeField] protected int _maxHp;
    [TabGroup("Core", "General")]
    [SerializeField] protected int _baseSpeed;





    public delegate void CreatureStateEvent(CreatureState newState);
    public event CreatureStateEvent OnStateChanged;



    //Monobehaviours
    private void Awake()
    {
        RunOnAwakeUtils();
    }



    //internals
    protected void RunOnAwakeUtils()
    {
        _entityID = GetInstanceID();
        ReadData();
        RunOtherUtilsOnAwake();
    }
    protected abstract void ReadData();
    protected virtual void RunOtherUtilsOnAwake() { }



    protected void ChangeState(CreatureState newState)
    {
        if (newState != _currentState)
        {
            _currentState = newState;
            OnStateChanged(newState);
        }
    }

    

    //Externals



}
