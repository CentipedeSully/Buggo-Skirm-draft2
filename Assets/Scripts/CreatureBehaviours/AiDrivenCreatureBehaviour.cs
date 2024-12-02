using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AiDrivenCreatureBehaviour : AbstractCreatureBehaviour
{
    //Declarations
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected Transform _currentObjective;
    protected Vector3 _objectiveDestination;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected bool _isObjectivePathingInterruptable = true;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected LayerMask _detectionLayerMask;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected float _detectionRadius = 3;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected GameObject _pursuitTarget;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected Vector3 _pursuitTargetPosition;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected float _distanceFromPursuitTarget;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected Vector3 _targetDirection;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected Color _pursuitPointerColor;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected Color _attackLineColor;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected bool _showPursuitLine = false;
    [TabGroup("Core", "Ai")]
    [SerializeField] protected bool _showAttackLine = false;
    [TabGroup("Core", "Ai")]
    [SerializeField][ReadOnly] protected bool _isInValidPursuit = false;





    //Monobehaviours
    private void OnDrawGizmos()
    {
        DrawPursuitLine();
        DrawAttackLine();
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


    private void UpdateTargetingData()
    {
        //ignore the y axis
        _pursuitTargetPosition = new Vector3(_pursuitTarget.transform.position.x, 0, _pursuitTarget.transform.position.z);
        Vector3 currentPositionNoY = new Vector3(transform.position.x, 0, transform.position.z);

        //update distance
        _distanceFromPursuitTarget = Vector3.Distance(currentPositionNoY, _pursuitTargetPosition);

        //calculate the target's local position in repect to our position
        _targetDirection = _pursuitTargetPosition - currentPositionNoY;
    }
    private void ClearTargetingData()
    {
        _pursuitTarget = null;
        _pursuitTargetPosition = Vector3.zero;
        _distanceFromPursuitTarget = 0;
        _targetDirection = Vector3.zero;
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


    private void DrawPursuitLine()
    {
        if (_isInValidPursuit && _showPursuitLine)
        {
            Gizmos.color = _pursuitPointerColor;
            Gizmos.DrawLine(transform.position, transform.position + _targetDirection);
        }
    }
    private void DrawAttackLine()
    {
        if (_coreAtk != null && _showAttackLine)
        {
            Gizmos.color = _attackLineColor;
            Gizmos.DrawLine(transform.position, transform.position + (_coreAtk.GetAtkDirection() * _coreAtk.GetMaxAtkRange()));
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
