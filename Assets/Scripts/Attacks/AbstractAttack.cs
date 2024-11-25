using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;



public enum AtkState
{
    unset,
    NotAtking,
    PreparingAtk,
    CastingAtk,
    RecovingFromAtk,
    CoolingAtk
}

public interface IAttack
{
    void PerformAttack();
    void InterruptAttack();
    void EndCooldown();
    
    AtkState GetAtkState();
    void SetAttacker(IAttacker attacker);
    void SetDamage(int damage);
    void SetCooldown(float cooldown);


    bool IsAtkReady();
    bool IsAttacking();
    bool IsCoolingDown();
    float GetMinAtkRange();
    float GetMaxAtkRange();
    Vector3 GetAtkDirection();

    void SubscribeToStateChange(AbstractAttack.StateChangeFunction function);
    bool IsFunctionAlreadySubscribedToOnStateChanged(AbstractAttack.StateChangeFunction function);
    public void UnsubscribeFromStateChange(AbstractAttack.StateChangeFunction function);

    

}


public abstract class AbstractAttack : SerializedMonoBehaviour, IAttack
{
    //Declarations
    [SerializeField] [ReadOnly] protected AtkState _atkState = AtkState.unset;
    [SerializeField] protected Transform _atkOrigin;
    [SerializeField] [ReadOnly] protected Vector3 _localAtkDirection;
    [SerializeField] protected Dictionary<AtkState,string> _actionNames = new Dictionary<AtkState,string>();
    [SerializeField] protected float _prepDuration = .3f;
    [SerializeField] protected float _castDuration = .2f;
    [SerializeField] protected float _recoverDuration = .5f;
    [SerializeField] protected float _coolDuration = .5f;
    [SerializeField] protected int _damage = 0;
    [SerializeField] protected float _minAtkRange;
    [SerializeField] protected float _maxAtkRange;

    protected IAttacker _attackerBehaviour;
    protected IEnumerator _atkController;

    public delegate void StateChangeFunction(AtkState atkState, string actionName);
    public event StateChangeFunction OnStateChanged;


    //Monobehaviours
    private void Awake()
    {
        InitializeUtils();
    }

    private void OnDisable()
    {
        UnsubscribeAllListenersFromOnStateChanged();
    }


    //internals
    protected virtual void InitializeUtils()
    {
        _atkState = AtkState.NotAtking;
        _attackerBehaviour = GetComponent<IAttacker>();

        //default any missing atkState names into empty Strings
        //the creatureBehaviour is expected to know how to handle empty actionNames
        DefaultActionNameIfNecessary(AtkState.NotAtking);
        DefaultActionNameIfNecessary(AtkState.PreparingAtk);
        DefaultActionNameIfNecessary(AtkState.CastingAtk);
        DefaultActionNameIfNecessary(AtkState.RecovingFromAtk);
        DefaultActionNameIfNecessary(AtkState.CoolingAtk);

        //calculate the AtkDirection.
        //Used to orient the attacker's rotation into a favorable attack angle
        _localAtkDirection = (transform.position - _atkOrigin.position).normalized;
    }

    protected void DefaultActionNameIfNecessary(AtkState state)
    {
        if (!_actionNames.ContainsKey(state))
            _actionNames.Add(state, "");
    }

    protected IEnumerator EnterAttackSequence()
    {

        ChangeState(AtkState.PreparingAtk);
        yield return new WaitForSeconds(_prepDuration);

        ChangeState(AtkState.CastingAtk);
        yield return new WaitForSeconds(_castDuration);

        ChangeState(AtkState.RecovingFromAtk);
        yield return new WaitForSeconds(_recoverDuration);

        //chain the cooldown sequence. Kept separate to simplify force-entering or bypassing Cooldowns
        _atkController = EnterCooldownSequence();
        StartCoroutine(_atkController);
    }

    protected IEnumerator EnterCooldownSequence()
    {
        ChangeState(AtkState.CoolingAtk);
        yield return new WaitForSeconds(_coolDuration);

        _atkController = null;
        ChangeState(AtkState.NotAtking);
    }

    protected void ChangeState(AtkState newState)
    {
        if (_atkState != newState && newState != AtkState.unset)
        {
            _atkState = newState;
            OnStateChanged?.Invoke(_atkState, _actionNames[_atkState]);
        }
    }

    protected void UnsubscribeAllListenersFromOnStateChanged()
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






    //Externals
    [BoxGroup("Debug")]
    [Button]
    public void PerformAttack()
    {
        if (IsAtkReady())
        {
            _atkController = EnterAttackSequence();
            StartCoroutine(_atkController);
        }
    }

    [BoxGroup("Debug")]
    [Button]
    public void InterruptAttack()
    {
        if (IsAttacking())
        {
            //leave the attackSequence
            StopCoroutine(_atkController);

            //enter the cooldownSequence
            _atkController = EnterCooldownSequence();
            StartCoroutine(_atkController);
        }
    }

    [BoxGroup("Debug")]
    [Button]
    public void EndCooldown()
    {
        if (IsCoolingDown())
        {
            //leave the cooldownSequence
            StopCoroutine(_atkController );

            //reset the atk
            _atkController = null;
            ChangeState(AtkState.NotAtking);
        }
    }



    public AtkState GetAtkState() { return _atkState; }

    [BoxGroup("Debug")]
    [Button]
    public bool IsAtkReady() { return _atkState == AtkState.NotAtking; }

    [BoxGroup("Debug")]
    [Button]
    public bool IsAttacking() 
    { 
        return (
            _atkState == AtkState.PreparingAtk ||
            _atkState == AtkState.CastingAtk ||
            _atkState == AtkState.RecovingFromAtk); 
    }

    [BoxGroup("Debug")]
    [Button]
    public bool IsCoolingDown() { return _atkState == AtkState.CoolingAtk; }



    public void SubscribeToStateChange(StateChangeFunction function)
    {
        if (!IsFunctionAlreadySubscribedToOnStateChanged(function))
            OnStateChanged += function;
        else
            Debug.LogWarning($" Ignoring subscription request for function {function.Method.Name}. Method already subscribed to event.");
    }

    public bool IsFunctionAlreadySubscribedToOnStateChanged(StateChangeFunction function)
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

    public void UnsubscribeFromStateChange(StateChangeFunction function)
    {
        OnStateChanged -= function;
    }

    public void SetDamage(int damage) { _damage = damage; }

    public void SetCooldown(float cooldown) { _coolDuration = cooldown; }

    public void SetAttacker(IAttacker attacker) { _attackerBehaviour = attacker; }

    public float GetMinAtkRange() { return _minAtkRange; }
    public float GetMaxAtkRange() { return _maxAtkRange; }

    public Vector3 GetAtkDirection() { return _localAtkDirection; }
}
