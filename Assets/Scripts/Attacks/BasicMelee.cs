using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;



public class BasicMelee : AbstractAttack
{
    //Declarations
    [BoxGroup("Atack Casting")]
    [SerializeField] private float _atkRadius;
    [BoxGroup("Atack Casting")]
    [SerializeField] private LayerMask _entityLayerMask;
    [BoxGroup("Atack Casting")]
    [SerializeField] private bool _doesAtkHitMultipleEntities = false;
    [BoxGroup("Atack Casting")]
    [SerializeField][ReadOnly] private bool _isHitDetectionSatisfied = false;
    [BoxGroup("Atack Casting")]
    [SerializeField][ReadOnly] private List<int> _entitiesHitByLatestAttack = new();
    [SerializeField] private Color _gizmoColor = Color.red;
    [SerializeField] private bool _showGizmo = false;




    //Monobehaviours
    private void OnEnable()
    {
        SubscribeToStateChange(ResetHitDataOnAtkCastEntered);
    }

    private void OnDisable()
    {
        UnsubscribeFromStateChange(ResetHitDataOnAtkCastEntered);
    }

    private void Update()
    {
        if (_atkState == AtkState.CastingAtk && !_isHitDetectionSatisfied)
            CheckForDetections();
    }

    private void OnDrawGizmosSelected()
    {
        if (_showGizmo)
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(_atkOrigin.position, _atkRadius);
        }
            
    }



    //Internals
    private void CheckForDetections()
    {
        Collider[] hitResults = Physics.OverlapSphere(_atkOrigin.position,_atkRadius, _entityLayerMask);

        foreach (Collider collider in hitResults)
        {
            IDamageable damageableBehaviour = collider.GetComponent<IDamageable>();

            //does the detection have a damageableBehaviour
            if (damageableBehaviour != null)
            {
                //make sure we're not damaging allies,
                //nor attacking something we've already hit during this attack,
                //NOR attacking an entity that's already dead!
                if (
                    damageableBehaviour.GetFaction() != _attackerBehaviour.GetFaction() &&
                    !_entitiesHitByLatestAttack.Contains(damageableBehaviour.GetEntityInfo().GetEntityID()) &&
                    !damageableBehaviour.IsDead())
                {
                    //apply the damage
                    damageableBehaviour.TakeDamage(_damage);

                    //track what we hit (for debugging)
                    _entitiesHitByLatestAttack.Add(damageableBehaviour.GetEntityInfo().GetEntityID());

                    //end the cast if damage piercing is disabled
                    if (!_doesAtkHitMultipleEntities)
                    {
                        _isHitDetectionSatisfied = true;
                        return;
                    }
                }
            }
        }
    }

    private void ResetHitDataOnAtkCastEntered(AtkState state, string actionName)
    {
        if (state == AtkState.CastingAtk)
        {
            _isHitDetectionSatisfied = false;
            _entitiesHitByLatestAttack.Clear();
        }
    }

    //Externals





}
