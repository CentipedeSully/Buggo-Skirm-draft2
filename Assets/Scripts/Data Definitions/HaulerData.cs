using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;




[CreateAssetMenu(fileName = "newHaulerData", menuName = "Data/Hauler Data")]
public class HaulerData : AbstractCreatureData
{
    [TabGroup("Setup", "Hauling")]
    [SerializeField] private Dictionary<ResourceType, int> _baseHaulCapacity = new();

    public Dictionary<ResourceType, int> GetBaseHaulCapacity() { return _baseHaulCapacity; }

}
