using UnityEngine;
using System.Collections;

public class UnitParent : MonoBehaviour
{
    private enum stateGUI { DEFAULT, OPEN }
    private stateGUI StateGUI;

    //build Menu variables
    private int unitNum;
    public int menuButtonWidth;
    public int menuButtonHeight;
    public int menuButtonBufferY;
    public int menuButtonBufferX;
    public string[] UnitStrings;
    private int menuWidth;
    private int menuHeight;
    private int[,] menuButtonPos; //[icon number, {x,y}]
    private int[] menuPos = new int[2]; //[0] = x coordinate, [1]= y coordinate

    public Transform[] Units; //list of all the buildings that can be built 
    public Transform userControl; //the UserControl object
    public Transform hud; //the heads up display object

    private Transform unit;



    // Use this for initialization
    void Start()
    {
        //set states
        StateGUI = stateGUI.DEFAULT;

        //Menu initialization
        menuWidth = 20 + 20 + menuButtonWidth;
        menuHeight = 40 + (UnitStrings.Length * (menuButtonHeight + menuButtonBufferY));
        menuPos[0] = Screen.width - menuWidth - 10;
        menuPos[1] = Screen.height - menuHeight;
        menuButtonPos = new int[UnitStrings.Length, 2];
        for (int i = 0; i < UnitStrings.Length; i++)
        {
            menuButtonPos[i, 0] = menuPos[0] + menuButtonBufferX;
            menuButtonPos[i, 1] = menuPos[1] + (i * menuButtonBufferY) + (i * menuButtonHeight) + menuButtonBufferX * 2;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Create a unit
    /// </summary>
    /// <param name="name">The name of the unit you want to create</param>
    /// <returns>Returns the unit prefab to be created, returns null if unit could not be created</returns>
    public Transform CreateUnit(string name)
    {
        //find the requested unit
        for (int i = 0; i < Units.Length; i++)
        {
            if (name == Units[i].name && UnitPreCondition(Units[i])) //found the building and the building can be built
            {
                //TO DO: set that this building has been built for requirements met purposes

                //lower resources
                PlayerData.manPower -= Units[i].GetComponent<Unit>().ManPowerCost;
                PlayerData.minerals -= Units[i].GetComponent<Unit>().MineralCost;
                return Units[i];
            }
        }

        print("INVALID UNIT NAME");
        return null;
    }

    /// <summary>
    /// Checks if the pre conditions for creating the unit are met
    /// </summary>
    /// <param name="_num">The array number that the unit resides at</param>
    /// <returns>True if the unit can be made, false if a condition is not met</returns>
    private bool UnitPreCondition(Transform _unit)
    {
        //check resources
        if (PlayerData.manPower - _unit.GetComponent<Unit>().ManPowerCost < 0) //failed
        {
            print("CANNOT CREATE UNIT: NOT ENOUGH MAN POWER");
            return false;
        }
        if (PlayerData.minerals - _unit.GetComponent<Unit>().MineralCost < 0) //failed
        {
            print("CANNOT CREATE UNIT: NOT ENOUGH MINERALS");
            return false;
        }

        //check requirements
        if (!_unit.GetComponent<Unit>().RequirementsMet()) //failed
        {
            print("CANNOT CREATE UNIT: REQUIREMENTS NOT MET");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Name of the unit of whoms menu to open
    /// </summary>
    /// <param name="name">Name of the unit</param>
    public bool OpenUnitMenu(Transform _unit)
    {
        unit = _unit;

        //find the requested building
        for (int i = 0; i < Units.Length; i++)
        {
            if (unit.name.Contains(Units[i].name)) //found the unit
            {
                StateGUI = stateGUI.OPEN;
                unitNum = i;
                return true;
            }
        }
        print(unit.name);
        print("INVALID UNIT NAME");
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
        GUI.Box(new Rect(menuPos[0], menuPos[1], menuWidth, menuHeight), Units[unitNum].name);

        //draw all the unit buttons
        for (int i = 0; i < UnitStrings.Length; i++)
        {
            if (GUI.Button(new Rect(menuButtonPos[i, 0], menuButtonPos[i, 1], menuButtonWidth, menuButtonHeight), UnitStrings[i]))
            {
                if (i == UnitStrings.Length - 1) //cancel button pressed
                {
                    StateGUI = stateGUI.DEFAULT;
                    userControl.GetComponent<UserControl>().EnterDefaultState();
                    hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();
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

    /// <summary>
    /// Cancels current menu and brings screen back to default state
    /// </summary>
    public void EnterDefaultState()
    {
        StateGUI = stateGUI.DEFAULT;
    }
}
