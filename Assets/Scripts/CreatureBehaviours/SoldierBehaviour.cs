using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;


public class SoldierBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [SerializeField] private SoldierData _data;

    

    //Monobehaviours




    //internals
    protected override void ReadData()
    {
        _maxHp = _data.GetBaseHealth();
        _currentHp = _maxHp;

        _baseSpeed = _data.GetBaseMoveSpeed();
        GetComponent<NavMeshAgent>().speed = _baseSpeed;
    }

    private void RespondToIdling(CreatureState newState) 
    {
        if (newState == CreatureState.Idling)
            Debug.Log("Idling Detected.");
    }

    private void RespondToActionPerforming(CreatureState newState)
    {
        if (newState == CreatureState.PerformingAction)
            Debug.Log($"ActionPerforming Detected");
    }

    private void RespondToMoving(CreatureState newState)
    {
        if (newState == CreatureState.Moving)
            Debug.Log($"Moving Detected");
    }



    //Externals
    [Button]
    public void SubscribeResponsesToStateChangeEvent()
    {
        SubscribeToStateChange(RespondToIdling);
        SubscribeToStateChange(RespondToActionPerforming);
        SubscribeToStateChange(RespondToMoving);
    }

    [Button]
    public void UpdateState(CreatureState newState, string desiredActionName)
    {
        ChangeState(newState, desiredActionName);
    }

    [Button]
    public void UnsubscribeResponsesFromStateChangeEvent()
    {
        UnsubscribeFromStateChange(RespondToIdling);
        UnsubscribeFromStateChange(RespondToActionPerforming);
        UnsubscribeFromStateChange(RespondToMoving);
    }


}
