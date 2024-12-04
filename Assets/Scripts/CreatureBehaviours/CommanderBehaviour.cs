using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommanderBehaviour : PlayerDrivenCreatureBehaviour
{
    //Declarations
    [SerializeField] private CommanderData _data;







    //Monobehaviours




    //internals
    protected override void ReadData()
    {
        _maxHp = _data.GetBaseHealth();
        _currentHp = _maxHp;
        _creatureType = _data.GetCreatureType();

        _damage = _data.GetBaseDamage();
        _cooldown = _data.GetBaseAtkCooldown();
        _coreAtk.SetDamage(_damage);
        _coreAtk.SetCooldown(_cooldown);

        _baseSpeed = _data.GetBaseMoveSpeed();
        _baseTurnSpeed = _data.GetBaseTurnSpeed();

        _navAgent.speed = _baseSpeed;
        _navAgent.angularSpeed = _baseTurnSpeed;
    }

    protected override void CreateCorpseYield()
    {
        _currentCorpseYield = _data.GetBaseCorpseYield();
    }

    protected override bool IsEntityInRangeForInteraction()
    {
        return _distanceFromPursuitTarget <= _closeEnoughDistance;
    }

    protected override void PerformEntityBasedInteraction()
    {
        Debug.Log("Performing Interaction");
        EntityType type = _pursuedEntity.GetEntityType();

        switch (type)
        {
            case EntityType.creature:
                _coreAtk?.PerformAttack();
                break;


            case EntityType.structure:
                _coreAtk?.PerformAttack();
                break;


            case EntityType.pickup:
                break;


            default:
                break;
        }

    }

    protected override void GetWithinRangeOfEntity()
    {
        Debug.Log("Getting within range...");
        EntityType type = _pursuedEntity.GetEntityType();

        switch (type)
        {
            case EntityType.creature:
                if (IsMovementAvailable())
                    GetWithinAttackRangeOfEntity();
                break;


            case EntityType.structure:
                if (IsMovementAvailable())
                    GetWithinAttackRangeOfEntity();
                break;


            case EntityType.pickup:
                if (_navAgent.destination != _pursuitTargetPosition && IsMovementAvailable())
                    _navAgent.SetDestination(_pursuitTargetPosition);
                break;


            default:
                break;
        }
    }

    protected override bool IsPursuitTargetStillValid()
    {
        if (_pursuitTarget == null)
            return false;

        else
        {
            EntityType type = _pursuedEntity.GetEntityType();

            switch (type)
            {
                case EntityType.creature:
                    if (_pursuedEntity.IsDead())
                        return false;
                    else return true;


                case EntityType.structure:
                    if (_pursuedEntity.IsDead())
                        return false;
                    else return true;


                case EntityType.pickup:
                    return false;


                default:
                    return false;
            }
        }
        
    }




    //Externals


}
