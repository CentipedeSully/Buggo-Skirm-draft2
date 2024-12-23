using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Sirenix.OdinInspector;

public class RemoteHpLink : SerializedMonoBehaviour, IDamageable, IEntity
{
    [SerializeField] private IDamageable _coreDamageableScript;

    private void Awake()
    {
        if (_coreDamageableScript == null)
            Debug.LogError("CoreDamageableScript is unset for a remote HP Link");
    }

    public EntityType GetEntityType() { return _coreDamageableScript.GetEntityInfo().GetEntityType();}

    public Faction GetFaction() {  return _coreDamageableScript.GetFaction();}

    public GameObject GetGameObject() { return _coreDamageableScript.GetGameObject();}

    public void TakeDamage(int damage) { _coreDamageableScript.TakeDamage(damage);}

    public bool IsDead() {  return _coreDamageableScript.IsDead();}

    public int GetEntityID()
    {
        return _coreDamageableScript.GetEntityInfo().GetEntityID();
    }

    public IEntity GetEntityInfo()
    {
        return _coreDamageableScript.GetEntityInfo();
    }
}
