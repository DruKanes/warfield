using UnityEngine;
using System.Collections;

public class BuildingParent : MonoBehaviour
{
    private enum stateGUI { DEFAULT, OPEN }
    private stateGUI StateGUI;

    //build Menu variables
    private int buildingNum;
    public int menuButtonWidth;
    public int menuButtonHeight;
    public int menuButtonBufferY;
    public int menuButtonBufferX;

    [System.Serializable]
    public class BuildingMenu { public string[] ButtonStrings; };
    public BuildingMenu[] BuildingStrings; // [Building Name, Array of Strings for each building]

    private int menuWidth;
    private int menuHeight;
    private int[,] menuButtonPos; //[icon number, {x,y}]
    private int[] menuPos = new int[2]; //[0] = x coordinate, [1]= y coordinate

    public Transform[] Buildings; //list of all the buildings that can be built 
    public Transform userControl; //the UserControl object
    public Transform hud; //heads up display object
    public Transform UnitParent; 
    private Transform unit;
    private Transform building;

    // Use this for initialization
    void Start()
    {
        //set states
        StateGUI = stateGUI.DEFAULT;
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
        if (!_building.GetComponent<Building>().RequirementsMet()) //failed
        {
            print("CANNOT CREATE BUILDING: REQUIREMENTS NOT MET");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Name of the building of whoms menu to open
    /// </summary>
    /// <param name="name">Name of the building</param>
    public bool OpenBuildingMenu(Transform _building)
    {
        building = _building;
        //find the requested building
        for (int i = 0; i < Buildings.Length; i++)
        {
            if (building.name.Contains(Buildings[i].name)) //found the building
            {
                buildingNum = i;
                StateGUI = stateGUI.OPEN;

                //build menu initialization
                menuWidth = 20 + 20 + menuButtonWidth;
                menuHeight = 40 + (BuildingStrings[buildingNum].ButtonStrings.Length * (menuButtonHeight + menuButtonBufferY));
                menuPos[0] = Screen.width - menuWidth - 10;
                menuPos[1] = Screen.height - menuHeight;
                menuButtonPos = new int[BuildingStrings[buildingNum].ButtonStrings.Length, 2];
                for (int j = 0; j < BuildingStrings[buildingNum].ButtonStrings.Length; j++)
                {
                    menuButtonPos[j, 0] = menuPos[0] + menuButtonBufferX;
                    menuButtonPos[j, 1] = menuPos[1] + (j * menuButtonBufferY) + (j * menuButtonHeight) + menuButtonBufferX * 2;
                }

                return true;
            }
        }

        print("INVALID BUILDING NAME");
        return false;
    }

    /// <summary>
    /// Draws the GUI items, this is called every fram
    /// </summary>
    void OnGUI()
    {
        switch (StateGUI)
        {
            case stateGUI.OPEN:
                BuildingMenuGUI();
                break;
            default:
                break;
        }
    }

    private void BuildingMenuGUI()
    {
        GUI.Box(new Rect(menuPos[0], menuPos[1], menuWidth, menuHeight), Buildings[buildingNum].name);

        if (building.GetComponent<Building>().IsBuilt()) //done building, can create units
        {
            //draw all the build buttons
            for (int i = 0; i < BuildingStrings[buildingNum].ButtonStrings.Length; i++)
            {
                if (GUI.Button(new Rect(menuButtonPos[i, 0], menuButtonPos[i, 1], menuButtonWidth, menuButtonHeight), BuildingStrings[buildingNum].ButtonStrings[i]))
                {
                    if (BuildingStrings[buildingNum].ButtonStrings[i] == "Close") //cancel button pressed
                    {
                        StateGUI = stateGUI.DEFAULT;
                        userControl.GetComponent<UserControl>().EnterDefaultState();
                        hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();
                        return;
                    }

                    if (BuildingStrings[buildingNum].ButtonStrings[i] == "Destroy") //destroy the building
                    {
                        StateGUI = stateGUI.DEFAULT;
                        userControl.GetComponent<UserControl>().EnterDefaultState();
                        hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();

                        //give back 50% resources of the building
                        PlayerData.manPower += building.GetComponent<Building>().ManPowerCost / 2;
                        PlayerData.minerals += building.GetComponent<Building>().MineralCost / 2;

                        Destroy(building.gameObject); //destroy the gameobject
                        return;
                    }

                    //create unit
                    Transform _unit = UnitParent.GetComponent<UnitParent>().CreateUnit(BuildingStrings[buildingNum].ButtonStrings[i]);
                    if (_unit != null)
                    {
                        unit = Instantiate(_unit) as Transform; //create the unit
                        unit.GetComponent<Unit>().Building = building;
                        building.GetComponent<Building>().CreatedUnit(unit);
                        PlayerData.unitCount++;
                        return;
                    }
                }

                if (Input.GetKeyUp(KeyCode.Escape)) //escape button
                {
                    StateGUI = stateGUI.DEFAULT;
                    userControl.GetComponent<UserControl>().EnterDefaultState();
                    hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();
                }
            }
        }
        else //still building, dont display unit buttons
        {
            for (int i = 0; i < BuildingStrings[buildingNum].ButtonStrings.Length; i++)
            {
                if (BuildingStrings[buildingNum].ButtonStrings[i] == "Close" || BuildingStrings[buildingNum].ButtonStrings[i] == "Destroy")
                {
                    if (GUI.Button(new Rect(menuButtonPos[i, 0], menuButtonPos[i, 1], menuButtonWidth, menuButtonHeight), BuildingStrings[buildingNum].ButtonStrings[i]))
                    {
                        if (BuildingStrings[buildingNum].ButtonStrings[i] == "Close") //cancel button pressed
                        {
                            StateGUI = stateGUI.DEFAULT;
                            userControl.GetComponent<UserControl>().EnterDefaultState();
                            hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();
                            return;
                        }

                        if (BuildingStrings[buildingNum].ButtonStrings[i] == "Destroy") //destroy the building
                        {
                            StateGUI = stateGUI.DEFAULT;
                            userControl.GetComponent<UserControl>().EnterDefaultState();
                            hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();

                            //give back 50% resources of the building
                            PlayerData.manPower += building.GetComponent<Building>().ManPowerCost / 2;
                            PlayerData.minerals += building.GetComponent<Building>().MineralCost / 2;

                            Destroy(building.gameObject); //destroy the gameobject
                            return;
                        }
                    }

                    if (Input.GetKeyUp(KeyCode.Escape)) //escape button
                    {
                        StateGUI = stateGUI.DEFAULT;
                        userControl.GetComponent<UserControl>().EnterDefaultState();
                        hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Cancels current menu and brings screen back to default state
    /// </summary>
    public void EnterDefaultState()
    {
        StateGUI = stateGUI.DEFAULT;
    }
}
