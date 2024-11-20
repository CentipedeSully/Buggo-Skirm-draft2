using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;




[CreateAssetMenu(fileName = "newSoldierData", menuName = "Data/Soldier Data")]
public class SoldierData : AbstractCreatureData
{
    [TabGroup("Setup", "Combat")]
    [SerializeField] private int _baseDamage;

    [TabGroup("Setup", "Combat")]
    [SerializeField] private float _baseAtkCooldown;


    public int GetBaseDamage() { return _baseDamage; }
    public float GetBaseAtkCooldown() { return _baseAtkCooldown; }
}
