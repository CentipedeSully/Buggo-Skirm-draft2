using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;


public class SoldierBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [SerializeField] private SoldierData _data;

    [TabGroup("Core", "Ai")]
    [SerializeField] [Min(0)]private float _targetingAngleForgiveness = 1f;
    [TabGroup("Core", "Ai")]
    [SerializeField] private float _targetingTurnSpeed = 50;
    [TabGroup("Core", "Ai")]
    [SerializeField] [ReadOnly] private float _signedAngularDifference;


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

    private float CalculateAngularDifferenceFromAtkDirectionToTarget()
    {
        //calculate the angular difference btwn the atkDirection and the target's current relational direction
        return Vector3.SignedAngle(_coreAtk.GetAtkDirection(), new Vector3(_targetDirection.x,0,_targetDirection.z), Vector3.up);
    }

    protected override bool IsEntityInRangeForInteraction()
    {
        bool isEntityWithinAppropriateDistance = _distanceFromPursuitTarget >= _coreAtk.GetMinAtkRange() &&
                                                 _distanceFromPursuitTarget <= _coreAtk.GetMaxAtkRange();

        //calculate the angular difference
        _signedAngularDifference = CalculateAngularDifferenceFromAtkDirectionToTarget();

        //Ignore the sign. Only care about whether or not we're misaligned--
        bool isEntityAligned = Mathf.Abs(_signedAngularDifference) <= _targetingAngleForgiveness;

        return isEntityWithinAppropriateDistance && isEntityAligned;


    }

    protected override void PerformEntityBasedInteraction()
    {
        if (_coreAtk != null && _coreAtk.IsAtkReady())
            _coreAtk.PerformAttack();
    }

    protected override void GetWithinRangeOfEntity()
    {
        //move towards the target if too far
        if (_distanceFromPursuitTarget > _coreAtk.GetMaxAtkRange())
            _navAgent.SetDestination(_pursuitTarget.transform.position);

        //back away from the target if too close
        else if (_distanceFromPursuitTarget < _coreAtk.GetMinAtkRange())
            _navAgent.Move(transform.position - _pursuitTargetPosition);
        
        else
        {
            //Stop Moving towards the target
            EndCurrentMovement();

            //calculate our rotation offset
            float frameRotation = _targetingTurnSpeed * Time.deltaTime;

            //create our rotation vector
            Vector3 rotationalAdditive = new Vector3(0,frameRotation,0);

            //get our current rotataion
            Vector3 currentRotation = transform.rotation.eulerAngles;


            //turn to the left if necessary
            if (_signedAngularDifference < 0)
                transform.rotation = Quaternion.Euler(currentRotation - rotationalAdditive);

            //else turn to the right if necessary
            else if (_signedAngularDifference > 0)
                transform.rotation = Quaternion.Euler(currentRotation + rotationalAdditive);

        }
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
