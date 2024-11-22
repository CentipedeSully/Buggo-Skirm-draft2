using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class HaulerBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [SerializeField] private HaulerData _data;

    public override void CommandMovementToPosition(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    protected override void CreateCorpseYield()
    {
        _currentCorpseYield = _data.GetBaseCorpseYield();
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
