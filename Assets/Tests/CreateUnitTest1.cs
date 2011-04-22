//using UnityEngine;
//using System.Collections;

//public class CreateUnitTest1 : MonoBehaviour
//{
//    public Transform unitPrefab; //the unit that will be built
//    public Transform unitPrefab_high; //the unit in which we dont meet the specifications
//    public Transform building; //the buildint in which the unit will be created from
//    private Transform[] unit; //the instance of the new building

//    //obstacles used to test pre-condition of building creation placement
//    public Transform unitObstacle;
//    public Transform buildingObstacle;

//    bool run;

//    // Use this for initialization
//    void Start()
//    {
//        run = true;
//        unit = new Transform[10];
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (run)
//        {
//            //first test, build 10 units
//            bool firstTest = true;
//            for (int i = 0; i < 10; i++)
//            {
//                unit[i] = building.GetComponent<Building>().CreateUnit(unitPrefab);
//                unit[i].position = unit[i].GetComponent<Unit>().SpawnLocation();
//                if(!IsBuildValid(unit[i]))
//                    firstTest = false;
//            }
//            if (firstTest)
//                print("10 units were created successfully");

//            //clear the 10 units that were created
//            for (int i = 0; i < 10; i++)
//                Destroy(unit[i].gameObject);

//            //second test, attempt to create units in which the requirements have not been met
//            unit[0] = building.GetComponent<Building>().CreateUnit(unitPrefab_high);
//            unit[0].position = unit[0].GetComponent<Unit>().SpawnLocation();
//            if (IsBuildValid(unit[0]))
//                print("Building was placed successfully");
//            Destroy(unit[0].gameObject);

//            //third test, attempt to create units in which player has insufficient resources
//            PlayerData.manPower = 0;
//            PlayerData.minerals = 0;
//            unit[0] = building.GetComponent<Building>().CreateUnit(unitPrefab);
//            unit[0].position = unit[0].GetComponent<Unit>().SpawnLocation();
//            if (IsBuildValid(unit[0]))
//                print("Building was placed successfully");
//            Destroy(unit[0].gameObject);

//            run = false;
//        }
//    }

//    private bool IsBuildValid(Transform _unit)
//    {
//        //check if you have sufficient funds
//        if (PlayerData.minerals - _unit.GetComponent<Building>().MineralCost < 0) //dont have enough rescources
//        {
//            print("Unit cannot be created, insufficient resources");
//            return false;
//        }

//        //check if you have sufficient funds
//        if (PlayerData.manPower - _unit.GetComponent<Building>().ManPowerCost < 0) //dont have enough rescources
//        {
//            print("Unit cannot be created, insufficient resources");
//            return false;
//        }

//        //check if requirements are met to build this tower
//        if (!_unit.GetComponent<Building>().RequirementsMet())
//        {
//            print("Unit cannot be created, requirements for this unit have not been met");
//            return false;
//        }

//        return true;
//    }
//}
