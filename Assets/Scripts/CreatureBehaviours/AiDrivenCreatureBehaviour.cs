using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AiDrivenCreatureBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [TabGroup("Driver", "Driver")]
    [SerializeField][ReadOnly] protected Transform _currentObjective;
    protected Vector3 _objectiveDestination;
    [TabGroup("Driver", "Driver")]
    [SerializeField] protected bool _isObjectivePathingInterruptable = true;
    [TabGroup("Driver", "Driver")]
    [SerializeField] protected LayerMask _detectionLayerMask;
    [TabGroup("Driver", "Driver")]
    [SerializeField] protected float _detectionRadius = 3;
    [TabGroup("Driver", "Driver")]







    //Monobehaviours
    private void Update()
    {
        ManageCreatureBehaviour();
    }




    //Internals
    protected override void ManageCreatureBehaviour()
    {
        switch (_currentState)
        {
            case CreatureState.Idling:

                //continue questing towards the objective, if we have one
                if (_currentObjective != null)
                    ApproachObjective();

                //else remain vigilant
                else
                    DetectPursuableEntities();

                break;


            case CreatureState.MovingToObjective:

                //Move towards the current objective until the objective is reached
                ApproachObjective();
                break;


            case CreatureState.PursuingEntity:

                //Pursue the entity, whatever it is, however we're capable of pursuing it
                PursueEntity();
                break;


            case CreatureState.EnteringAction:
                break;


            case CreatureState.PerformingAction:
                break;


            case CreatureState.RecoveringFromAction:
                break;


            case CreatureState.Stunned:
                break;


            case CreatureState.Dead:
                break;



            default:
                break;
        }

    }



    protected override void PursueEntity()
    {
        //get at the target object if it still exists
        if (IsPursuitTargetStillValid())
        {
            UpdateTargetingData();

            //toggle drawing the pursuit line
            _isInValidPursuit = true;

            if (IsEntityInRangeForInteraction())
            {
                EndCurrentMovement();
                PerformEntityBasedInteraction();
            }

            else
                GetWithinRangeOfEntity();
        }

        //otherwise, either resume moving to the objective or idle.
        else
        {

            //toggle hiding the pursuit line
            _isInValidPursuit = false;

            ClearTargetingData();

            if (_currentObjective != null)
                ChangeState(CreatureState.MovingToObjective);
            else
                ChangeState(CreatureState.Idling);
        }
    }

    protected virtual void DetectPursuableEntities()
    {
        Collider[] detections = Physics.OverlapSphere(transform.position, _detectionRadius, _detectionLayerMask);

        foreach (Collider detection in detections)
        {
            if (IsDetectedEntityValid(detection))
            {
                _pursuitTarget = detection.gameObject;
                ChangeState(CreatureState.PursuingEntity);
            }
        }
    }
    protected abstract bool IsDetectedEntityValid(Collider detection);
    protected void UpdateObjectiveDestination()
    {
        if (_currentObjective != null)
            _objectiveDestination = _currentObjective.position;

        else
            _objectiveDestination = transform.position;

        _navAgent.SetDestination(_objectiveDestination);
    }
    protected virtual void ApproachObjective()
    {
        //track (and path towards) the objective's position this frame (if it exists)
        UpdateObjectiveDestination();

        //remain vigilant if our focus is interruptable
        if (_isObjectivePathingInterruptable)
            DetectPursuableEntities();

        //end the movement if we're close enough to the objective (assuming the pathing is completed)
        if (_navAgent.pathPending)
            return;

        else if (_navAgent.remainingDistance <= _closeEnoughDistance)
        {
            EndCurrentMovement();
            _currentObjective = null;
            ChangeState(CreatureState.Idling);
        }

    }





    //Externals
    public override void SetObjective(Transform objectiveTransform)
    {
        if (_currentState != CreatureState.Dead && _navAgent != null && objectiveTransform != null)
        {
            _currentObjective = objectiveTransform;
            ChangeState(CreatureState.MovingToObjective);
        }
    }





}
