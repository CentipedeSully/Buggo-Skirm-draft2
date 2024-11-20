using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;



public enum CreatureType
{
    unset,
    Commander,
    Soldier,
    Hauler
}



public abstract class AbstractCreatureData : SerializedScriptableObject
{

    [TabGroup("Setup", "Base")]
    [SerializeField] protected GameObject _prefab;
    [TabGroup("Setup", "Base")]
    [SerializeField] protected CreatureType _creatureType = CreatureType.unset;
    [TabGroup("Setup", "Base")]
    [SerializeField] protected int _baseHealth = 1;
    [TabGroup("Setup", "Base")]
    [SerializeField] protected int _baseMoveSpeed = 10;

    [TabGroup("Setup", "Base")]
    [SerializeField] protected Dictionary<ResourceType, int> _baseCorpseYield = new();
    [TabGroup("Setup", "Base")]
    [SerializeField] protected Dictionary<ResourceType, int> _baseSpawnCost = new();



    public CreatureType GetCreatureType() {  return _creatureType; }
    public int GetBaseHealth() { return _baseHealth; }
    public int GetBaseMoveSpeed() { return _baseMoveSpeed; }
    public Dictionary<ResourceType, int> GetBaseCorpseYield() { return _baseCorpseYield; }
    public Dictionary<ResourceType, int> GetBaseSpawnCost() { return _baseSpawnCost; }
    public GameObject GetPrefab() { return _prefab; }

}
