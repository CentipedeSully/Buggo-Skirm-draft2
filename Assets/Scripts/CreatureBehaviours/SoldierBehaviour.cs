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
        _creatureType = _data.GetCreatureType();

        _damage = _data.GetBaseDamage();
        _cooldown = _data.GetBaseAtkCooldown();
        _coreAtk.SetDamage( _damage );
        _coreAtk.SetCooldown( _cooldown);

        _baseSpeed = _data.GetBaseMoveSpeed();
        _navAgent.speed = _baseSpeed;
    }

    protected override void RunOtherUtilsOnDeath()
    {
        EndCurrentMovement();
    }

    

    

    //Externals
    public override void CommandMovementToPosition(Vector3 position)
    {
        if (_currentState != CreatureState.Dead && _navAgent != null)
        {
            ChangeState(CreatureState.Moving);

            _navAgent.SetDestination(position);
        }
    }

    protected override void CreateCorpseYield()
    {
        _currentCorpseYield = new();

        //create a personal copy of the data's corpse-yield dictionary
        foreach (KeyValuePair<ResourceType,int> entry in _data.GetBaseCorpseYield())
            _currentCorpseYield.Add(entry.Key, entry.Value);
    }
}
