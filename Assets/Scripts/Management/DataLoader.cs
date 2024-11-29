using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class DataLoader : MonoBehaviour
{
    //Declarations

    [TabGroup("Core","Data")]
    [SerializeField] private CommanderData _commanderData;
    [TabGroup("Core", "Data")]
    [SerializeField] private SoldierData _soldierData;
    [TabGroup("Core", "Data")]
    [SerializeField] private HaulerData _haulerData;
    [TabGroup("Core", "Data")]
    [SerializeField] private Transform _commanderContainer;
    [TabGroup("Core", "Data")]
    [SerializeField] private Transform _soldierContainer;
    [TabGroup("Core", "Data")]
    [SerializeField] private Transform _haulerContainer;

    [TabGroup("Core", "Nest Settings")]
    [SerializeField] private NestBehaviour _teamAlphaNest;
    [TabGroup("Core", "Nest Settings")]
    [SerializeField] private NestBehaviour _teamBetaNest;




    //Monobehaviours





    //Internals





    //Externals






}
