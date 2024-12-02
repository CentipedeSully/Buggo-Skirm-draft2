using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommanderBehaviour : PlayerDrivenCreatureBehaviour
{
    //Declarations
    [SerializeField] private CommanderData _data;

    [TabGroup("Core", "Ai")]
    [SerializeField][Min(0)] private float _targetingAngleForgiveness = 1f;
    [TabGroup("Core", "Ai")]
    [SerializeField] private float _targetingTurnSpeed = 50;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] private float _signedAngularDifference;



    


    //Monobehaviours




    //internals
    protected override void ReadData()
    {
        _maxHp = _data.GetBaseHealth();
        _currentHp = _maxHp;

        _baseSpeed = _data.GetBaseMoveSpeed();
        GetComponent<NavMeshAgent>().speed = _baseSpeed;
    }

    protected override void CreateCorpseYield()
    {
        _currentCorpseYield = _data.GetBaseCorpseYield();
    }

    protected override void GetWithinRangeOfEntity()
    {
        throw new System.NotImplementedException();
    }


    protected override bool IsEntityInRangeForInteraction()
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsPursuitTargetStillValid()
    {
        throw new System.NotImplementedException();
    }

    protected override void PerformEntityBasedInteraction()
    {
        throw new System.NotImplementedException();
    }

    protected override void PursueEntity()
    {
        throw new System.NotImplementedException();
    }

    protected override void ManageCreatureBehaviour()
    {
        throw new System.NotImplementedException();
    }

    public override void SetObjective(Transform objectiveTransform)
    {
        throw new System.NotImplementedException();
    }



    //Externals




}
