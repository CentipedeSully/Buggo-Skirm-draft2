using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


public enum ResourceType
{
    unset,
    Meat,
    Jelly
}


public class NestBehaviour : MonoBehaviour
{
    //Declarations
    [TabGroup("Core","Setup")]
    [SerializeField] private CommanderData _commanderData;
    [TabGroup("Core","Setup")]
    [SerializeField] private SoldierData _soldierData;
    [TabGroup("Core","Setup")]
    [SerializeField] private HaulerData _haulerData;

    [TabGroup("Core","Setup")]
    [SerializeField] private Transform _spawnPoint;

    [TabGroup("Core","Setup")]
    [SerializeField] private Transform _commanderContainer;
    [TabGroup("Core","Setup")]
    [SerializeField] private Transform _soldierContainer;
    [TabGroup("Core","Setup")]
    [SerializeField] private Transform _haulerContainer;


    [TabGroup("Core", "Setup")]
    [SerializeField] private Transform _objectiveLocation;

    [TabGroup("Debug")]
    [SerializeField] private bool _forceSpawnSoldersOverTime = false;
    [TabGroup("Debug")]
    [SerializeField] private float _forceSpawnRate = .33f;
    [TabGroup("Debug")]
    [SerializeField] [ReadOnly] private bool _isForceSpawning = false;

    //Monobehaviours
    private void Update()
    {
        ForceSpawnSoldiersOverTime();
    }



    //Internals
    private void ForceSpawnSoldiersOverTime()
    {
        if (_isForceSpawning)
        {
            if (!_forceSpawnSoldersOverTime)
            {
                CancelInvoke(nameof(SpawnSoldier));
                _isForceSpawning = false;
            }
        }
        else
        {
            if (_forceSpawnSoldersOverTime)
            {
                InvokeRepeating(nameof(SpawnSoldier),0,_forceSpawnRate);
                _isForceSpawning = true;
            }
        }
    }



    //Externals
    [BoxGroup("Debug")]
    [Button]
    public void SpawnSoldier()
    {
        GameObject newSoldier = Instantiate(_soldierData.GetPrefab(), _spawnPoint,false);
        newSoldier.transform.SetParent(_soldierContainer, true);

        NavMeshAgent navAgent =newSoldier.GetComponent<NavMeshAgent>();
        navAgent.SetDestination(_objectiveLocation.position);
    }

    public void SpawnHauler()
    {

    }

    public void SpawnCommander()
    {

    }




}
