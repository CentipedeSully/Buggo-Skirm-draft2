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

public enum Faction
{
    unset,
    Neutral,
    Alpha,
    Beta
}

public class NestBehaviour : MonoBehaviour, IDamageable, IEntity
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
    [TabGroup("Core", "Setup")]
    [SerializeField] private Faction _faction = Faction.unset;


    [SerializeField] private int _health = 50;
    [SerializeField] [ReadOnly] private int _entityID;
    [SerializeField] private EntityType _entityType = EntityType.structure;


    [TabGroup("Debug")]
    [SerializeField] private bool _forceSpawnSoldersOverTime = false;
    [TabGroup("Debug")]
    [SerializeField] private float _forceSpawnRate = .33f;
    [TabGroup("Debug")]
    [SerializeField] [ReadOnly] private bool _isForceSpawning = false;


    //Monobehaviours
    private void Awake()
    {
        _entityID = GetInstanceID();
    }

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

    private void Die()
    {
        _health = 0;
        StopAllCoroutines();
        Destroy(gameObject);
    }

    //Externals
    [BoxGroup("Debug")]
    [Button]
    public void SpawnSoldier()
    {
        //Spawn the new Soldier
        GameObject newSoldier = Instantiate(_soldierData.GetPrefab(), _spawnPoint,false);
        
        //Put the soldier in it's proper container (for cleanliness)
        newSoldier.transform.SetParent(_soldierContainer, true);

        //setup the soldier's core behaviour
        SoldierBehaviour behaviour = newSoldier.GetComponent<SoldierBehaviour>();
        behaviour.SetFaction(_faction);

        //command the soldier!
        behaviour.SetObjective(_objectiveLocation);

    }

    public void SpawnHauler()
    {

    }

    public void SpawnCommander()
    {

    }

    public GameObject GetGameObject() { return gameObject; }

    public Faction GetFaction() { return _faction; }

    public EntityType GetEntityType() { return _entityType; }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
            Die();

    }

    public bool IsDead() {return _health <= 0;}

    public IEntity GetEntityInfo() {return this;}

    public int GetEntityID() {return _entityID;}
}
