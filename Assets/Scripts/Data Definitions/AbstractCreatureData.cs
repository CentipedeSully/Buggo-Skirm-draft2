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
    [SerializeField] protected float _baseTurnSpeed = 120f;
    [TabGroup("Setup", "Base")]
    [SerializeField] protected ResourceType[] _corpseResources;
    [TabGroup("Setup", "Base")]
    [SerializeField] protected int[] _corpseResourceYields;
    protected Dictionary<ResourceType, int> _baseCorpseYield = new();


    [TabGroup("Setup", "Base")]
    [SerializeField] protected ResourceType[] _spawnResources;
    [TabGroup("Setup", "Base")]
    [SerializeField] protected int[] _spawnResourceCosts;
    protected Dictionary<ResourceType, int> _baseSpawnCost = new();

    [TabGroup("Setup", "Base")]
    [SerializeField] [ReadOnly] protected bool _corpseYieldDictionaryBuilt = false;

    [TabGroup("Setup", "Base")]
    [SerializeField][ReadOnly] protected bool _spawnCostDictionaryBuilt = false;




    private void OnEnable()
    {
        RebuildDictionaries();
    }





    [TabGroup("Setup", "Base")]
    [Button]
    public void RebuildDictionaries()
    {
        //attempt to build the corpse yield dictionary, if the data object is built properly
        if (_corpseResources == null && _corpseResourceYields == null)
        {
            //clear any old data
            _baseCorpseYield.Clear();

            _corpseYieldDictionaryBuilt = true;
            Debug.Log($"{name} CorpseYield Data Successfully cleared.");
        }

        else if (_corpseResources.Length == _corpseResourceYields.Length)
        {
            //clear any old data
            _baseCorpseYield.Clear();

            for (int i = 0; i <_corpseResources.Length; i++)
                _baseCorpseYield.Add(_corpseResources[i], _corpseResourceYields[i]);

            _corpseYieldDictionaryBuilt = true;

            Debug.Log($"{name} CorpseYield Data Successfully rebuilt.");

        }

        else Debug.LogError($"'Available Resources' array length doesn't match 'Resource Yields' array length." +
            $" Failed to rebuild the Base Corpse Yield dictionary.");




        //attempt to build the spawn cost dictionary, if the data object is built properly
        if (_spawnResources == null && _spawnResourceCosts == null)
        {
            //clear the old data 
            _baseSpawnCost.Clear();

            _spawnCostDictionaryBuilt = true;
            Debug.Log($"{name} SpawnCost Data Successfully cleared.");

        }

        else if (_spawnResources.Length == _spawnResourceCosts.Length)
        {
            //clear any old data
            _baseSpawnCost.Clear();

            //match up the arrays
            for (int i = 0; i < _spawnResources.Length; i++)
                _baseSpawnCost.Add(_spawnResources[i], _spawnResourceCosts[i]);

            _spawnCostDictionaryBuilt = true;
            Debug.Log($"{name} SpawnCost Data Successfully rebuilt.");

        }

        else Debug.LogError($"'Required Resources' array length doesn't match 'Resource Costs' array length." +
            $" Failed to rebuild the Base Spawn Cost dictionary.");

    }

    public CreatureType GetCreatureType() {  return _creatureType; }
    public int GetBaseHealth() { return _baseHealth; }
    public float GetBaseTurnSpeed() { return _baseTurnSpeed; }
    public int GetBaseMoveSpeed() { return _baseMoveSpeed; }
    public Dictionary<ResourceType, int> GetBaseCorpseYield() { return _baseCorpseYield; }
    public Dictionary<ResourceType, int> GetBaseSpawnCost() { return _baseSpawnCost; }
    public GameObject GetPrefab() { return _prefab; }

}
