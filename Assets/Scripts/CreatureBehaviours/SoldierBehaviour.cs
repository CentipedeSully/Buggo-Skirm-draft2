using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;


public class SoldierBehaviour : AiDrivenCreatureBehaviour
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
        _baseTurnSpeed = _data.GetBaseTurnSpeed();

        _navAgent.speed = _baseSpeed;
        _navAgent.angularSpeed = _baseTurnSpeed;
    }

    protected override bool IsEntityInRangeForInteraction()
    {
        if (_coreAtk == null)
            return false;
        else return IsEntityInAttackRange();


    }

    protected override void PerformEntityBasedInteraction()
    {
        if (_coreAtk != null && _coreAtk.IsAtkReady())
            _coreAtk.PerformAttack();
    }

    protected override void GetWithinRangeOfEntity()
    {
        if (_coreAtk != null)
            GetWithinAttackRangeOfEntity();
    }

    

    protected override void CreateCorpseYield()
    {
        _currentCorpseYield = new();

        //create a personal copy of the data's corpse-yield dictionary
        foreach (KeyValuePair<ResourceType, int> entry in _data.GetBaseCorpseYield())
            _currentCorpseYield.Add(entry.Key, entry.Value);
    }

    protected override bool IsDetectedEntityValid(Collider detection)
    {
        if (detection != null)
        {
            IDamageable behaviour = detection.GetComponent<IDamageable>();

            if (behaviour != null)
            {
                //only living, non-allies are valid targets
                if (behaviour.GetFaction() != _faction && !behaviour.IsDead())
                    return true;
            }
        }

        //any null or undamageable detections aren't valid
        return false;
    }

    protected override bool IsPursuitTargetStillValid()
    {
        //invalid if the target got deleted unexpectedly
        if (_pursuitTarget == null)
            return false;


        else
        {
            IDamageable behaviour = _pursuitTarget.GetComponent<IDamageable>();

            //it's only valid if the target is alive
            //AND WITHIN DETECTION RANGE!
            if (behaviour != null)
                return !behaviour.IsDead() && _distanceFromPursuitTarget <= _detectionRadius;

            //invalid if we couldn't find the proper behaviour
            else 
                return false;
        }
    }



    //Externals


}
