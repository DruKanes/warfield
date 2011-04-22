using UnityEngine;
using System.Collections;

public class BuildingParent : MonoBehaviour
{

    public Transform[] Buildings; //list of all the buildings that can be built 

    

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Create a building
    /// </summary>
    /// <param name="name">The name of the building you want to create</param>
    /// <returns>Returns the building prefab to be created, returns null if building could not be created</returns>
    public Transform CreateBuilding(string name)
    {
        //find the requested building
        for (int i = 0; i < Buildings.Length; i++)
        {
            if (name == Buildings[i].name && BuildingPreCondition(Buildings[i])) //found the building and the building can be built
            {
                //TO DO: set that this building has been built for requirements met purposes
                
                //lower resources
                PlayerData.manPower -= Buildings[i].GetComponent<Building>().ManPowerCost;
                PlayerData.minerals -= Buildings[i].GetComponent<Building>().MineralCost;
                return Buildings[i];                
            }
        }

        print("INVALID BUILDING NAME");
        return null;
    }

    /// <summary>
    /// Checks if the pre conditions for creating the building are met
    /// </summary>
    /// <param name="_num">The array number that the building resides at</param>
    /// <returns>True if the building can be made, false if a condition is not met</returns>
    private bool BuildingPreCondition(Transform _building)
    {
        //check resources
        if (PlayerData.manPower - _building.GetComponent<Building>().ManPowerCost < 0) //failed
        {
            print("CANNOT CREATE BUILDING: NOT ENOUGH MAN POWER");
            return false;
        }
        if (PlayerData.minerals - _building.GetComponent<Building>().MineralCost < 0) //failed
        {
            print("CANNOT CREATE BUILDING: NOT ENOUGH MINERALS");
            return false;
        }

        //check requirements
        if(!_building.GetComponent<Building>().RequirementsMet()) //failed
        {
            print("CANNOT CREATE BUILDING: REQUIREMENTS NOT MET");
            return false;
        }

        return true;
    }
}
