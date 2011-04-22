//using UnityEngine;
//using System.Collections;

//public class CreateBuildingTest1 : MonoBehaviour
//{
//    public Transform buildingPrefab; //the building that will be built
//    public Transform buildingPrefab_high; //the building in which we dont meet the specifications
//    private Transform building; //the instance of the new building

//    //obstacles used to test pre-condition of building creation placement
//    public Transform unitObstacle; 
//    public Transform buildingObstacle;

//    bool run;

//    // Use this for initialization
//    void Start()
//    {
//        run = true;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (run)
//        {
//            //first test, place building in a clear location
//            building = Instantiate(buildingPrefab, transform.position, transform.rotation) as Transform;
//            if (IsBuildValid(building.position)) 
//                print("Building was placed successfully");
//            Destroy(building.gameObject);

//            //second test, place building over an existing unit
//            building = Instantiate(buildingPrefab, unitObstacle.position, unitObstacle.rotation) as Transform;
//            if (IsBuildValid(building.position))
//                print("Building was placed successfully");
//            Destroy(building.gameObject);

//            //third test, place building over an existing building
//            building = Instantiate(buildingPrefab, buildingObstacle.position, buildingObstacle.rotation) as Transform;
//            if (IsBuildValid(building.position))
//                print("Building was placed successfully");
//            Destroy(building.gameObject);

//            //fourth test, attempt to build a building without proper requirements
//            building = Instantiate(buildingPrefab_high, buildingObstacle.position, buildingObstacle.rotation) as Transform;
//            if (IsBuildValid(building.position))
//                print("Building was placed successfully");
//            Destroy(building.gameObject);

//            //fifth test, attempt to build a building with too low of resources
//            PlayerData.minerals = 0;
//            PlayerData.manPower = 0;
//            building = Instantiate(buildingPrefab, transform.position, transform.rotation) as Transform;
//            if (IsBuildValid(building.position))
//                print("Building was placed successfully");
//            Destroy(building.gameObject);

//            run = false;
//        }
//    }

//    private bool IsBuildValid(Vector3 _position)
//    {
//        //check if a unit or building is in the way
//        Collider[] colliders = Physics.OverlapSphere(_position, 10); //get all objects in a 10 unit radius
//        foreach (Collider target in colliders)
//        {
//            if (target.tag == "Building" || target.tag == "Unit") //in range of unit, cannot build
//            {
//                print("Cannot place Building, a unit or building is in the way");
//                return false;
//            }
//        }

//        //check if you have sufficient funds
//        if (PlayerData.minerals - building.GetComponent<Building>().MineralCost < 0) //dont have enough rescources
//        {
//            print("Cannot place Building, insufficient resources");
//            return false;
//        }

//        //check if you have sufficient funds
//        if (PlayerData.manPower - building.GetComponent<Building>().ManPowerCost < 0) //dont have enough rescources
//        {
//            print("Cannot place Building, insufficient resources");
//            return false;
//        }

//        //check if requirements are met to build this tower
//        if(!building.GetComponent<Building>().RequirementsMet())
//        {
//            print("Cannot place Building, requirements for this Building have not been met");
//            return false;
//        }

//        return true;
//    }
//}
