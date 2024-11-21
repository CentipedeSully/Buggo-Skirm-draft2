using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;


public class SoldierBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [SerializeField] private SoldierData _data;
    private NavMeshAgent _navAgent;



    //Monobehaviours



    //internals
    protected override void ReadData()
    {
        _maxHp = _data.GetBaseHealth();
        _currentHp = _maxHp;

        _baseSpeed = _data.GetBaseMoveSpeed();

        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.speed = _baseSpeed;
    }

    protected override void RunUtilsOnDeath()
    {
        EndCurrentMovement();
    }

    private void EndCurrentMovement()
    {
        if (_navAgent !=null)
            _navAgent.ResetPath();
    }


    //Externals
    

}
