using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;




[CreateAssetMenu(fileName ="newCommanderData",menuName ="Data/Commander Data")]
public class CommanderData : AbstractCreatureData
{
    [TabGroup("Setup","Combat")]
    [SerializeField] private int _baseDamage;
    [TabGroup("Setup", "Combat")]
    [SerializeField] private float _baseAtkCooldown;
    [TabGroup("Setup", "Hauling")]
    [SerializeField] private Dictionary<ResourceType, int> _baseHaulCapacity = new();

    public int GetBaseDamage() {  return _baseDamage; }
    public float GetBaseAtkCooldown() { return _baseAtkCooldown; }
    public Dictionary<ResourceType,int> GetBaseHaulCapacity() { return _baseHaulCapacity; }



}


