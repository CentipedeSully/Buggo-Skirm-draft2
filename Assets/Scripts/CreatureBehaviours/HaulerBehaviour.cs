using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class HaulerBehaviour : AiDrivenCreatureBehaviour
{
    //Declarations
    [SerializeField] private HaulerData _data;


    protected override void CreateCorpseYield()
    {
        _currentCorpseYield = _data.GetBaseCorpseYield();
    }

    protected override void GetWithinRangeOfEntity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsDetectedEntityValid(Collider detection)
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


    //Monobehaviours




    //internals
    protected override void ReadData()
    {
        _maxHp = _data.GetBaseHealth();
        _currentHp = _maxHp;

        _baseSpeed = _data.GetBaseMoveSpeed();
        GetComponent<NavMeshAgent>().speed = _baseSpeed;
    }



    //Externals

}
