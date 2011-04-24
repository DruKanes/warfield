using UnityEngine;
using System.Collections;

public class HeadsUpDisplay : MonoBehaviour
{
    private enum state { DEFAULT, BUILDING, SINGLE_UNIT, GROUP_UNIT }
    private state State;

    //top display
    public GUITexture topBackground;
    public GUIText minerals;
    public GUIText manPower;
    public GUIText unitCount;
    public GUIText time;

    //bottom display
    public GUITexture botBackground;
    public GUIText title;
    public GUIText health;
    public GUIText detail;
    public Texture2D progressEmpty;
    public Texture2D progressFull;
    private Vector2 progressPos;
    private Vector2 progressSize = new Vector2(100, 20);
    
    private Transform building;
    private Transform unit;
    public Transform Unit
    {
        set { unit = value; }
    }
    private int groupUnitCount;

    // Use this for initialization
    void Start()
    {
        //set the state
        State = state.DEFAULT;

        //top display
        topBackground.pixelInset = new Rect(0, Screen.height, Screen.width, Screen.height / 100);
        minerals.pixelOffset = new Vector2(10, Screen.height - 5);
        manPower.pixelOffset = new Vector2(Screen.width / 4, Screen.height - 5);
        unitCount.pixelOffset = new Vector2(Screen.width / 2, Screen.height - 5);
        time.pixelOffset = new Vector2(Screen.width / 1.334F, Screen.height - 5);

        //bot display
        botBackground.pixelInset = new Rect(0, 0, Screen.width, Screen.height / 100);
        title.pixelOffset = new Vector2(Screen.width / 4, title.fontSize + 10);
        health.pixelOffset = new Vector2(Screen.width / 2, health.fontSize + 10);
        detail.pixelOffset = new Vector2(Screen.width / 1.5F, detail.fontSize + 10);
        progressPos = new Vector2(Screen.width / 1.25F, Screen.height - detail.fontSize - 10);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTopDisplay();

        switch (State)
        {
            case state.DEFAULT:
                DefaultState();
                break;
            case state.BUILDING:
                BuildingState();
                break;
            case state.SINGLE_UNIT:
                SingleUnitState();
                break;
            case state.GROUP_UNIT:
                GroupUnitState();
                break;
            default:
                break;
        }

    }

    private void DefaultState()
    {
        
    }

    private void BuildingState()
    {
        title.text = building.name.Remove(building.name.IndexOf('('));
        health.text = "Health: " + building.GetComponent<Building>().CurrentHealth.ToString() + "/" + building.GetComponent<Building>().MaxHealth;
        if(building.GetComponent<Building>().State == Building.state.DEFAULT)
            detail.text = "";
    }

    private void SingleUnitState()
    {
        title.text = unit.name.Remove(unit.name.IndexOf('('));
        health.text = "Health: " + unit.GetComponent<Unit>().CurrentHealth.ToString() + "/" + unit.GetComponent<Unit>().MaxHealth;
        detail.text = "";
    }

    private void GroupUnitState()
    {
        title.text = groupUnitCount.ToString() + " Units Selected";
        health.text = "";
        detail.text = "";
    }

    void OnGUI()
    {
        switch (State)
        {
            case state.BUILDING:
                BuildingGUIState();
                break;
            default:
                break;
        }
    }

    private void BuildingGUIState()
    {
        if (building.GetComponent<Building>().State == Building.state.BUILDING) //building is being created
        {
            detail.text = "Building Progress:";
            progressPos = new Vector2(detail.pixelOffset.x + (detail.text.Length * (detail.fontSize/2)), Screen.height - detail.fontSize - 10);

            //draw the progress bar bar
            GUI.DrawTexture(new Rect(progressPos.x,progressPos.y,progressSize.x,progressSize.y), progressEmpty);
            GUI.DrawTexture(new Rect(progressPos.x, progressPos.y, progressSize.x * ((Time.time - building.GetComponent<Building>().StartBuildTime) / building.GetComponent<Building>().BuildTime), progressSize.y), progressFull);
        }
        else if (building.GetComponent<Building>().State == Building.state.CREATE_UNIT) //unit is being created
        {
            detail.text = building.GetComponent<Building>().Unit.name.Remove(building.GetComponent<Building>().Unit.name.IndexOf('(')) + " Progress:";
            progressPos = new Vector2(detail.pixelOffset.x + (detail.text.Length * (detail.fontSize / 2)), Screen.height - detail.fontSize - 10);

            //draw the progress bar bar
            GUI.DrawTexture(new Rect(progressPos.x, progressPos.y, progressSize.x, progressSize.y), progressEmpty);
            GUI.DrawTexture(new Rect(progressPos.x, progressPos.y, progressSize.x * ((Time.time - building.GetComponent<Building>().Unit.GetComponent<Unit>().StartBuildTime) / building.GetComponent<Building>().Unit.GetComponent<Unit>().BuildTime), progressSize.y), progressFull);
        }
    }

    /// <summary>
    /// Enter the default state for the Heads up Display
    /// </summary>
    public void EnterDefaultState()
    {
        title.text = "";
        health.text = "";
        detail.text = "";
        State = state.DEFAULT;
    }

    /// <summary>
    /// Enter the Building state for the heads up display
    /// </summary>
    /// <param name="_building">Building that was selected</param>
    public void EnterBuildingState(Transform _building)
    {
        building = _building;
        State = state.BUILDING;
    }

    /// <summary>
    /// Enter the Unit state for the heads up display
    /// </summary>
    /// <param name="_unit">Unit that was selected</param>
    public void EnterSingleUnitState(Transform _unit)
    {
        unit = _unit;
        State = state.SINGLE_UNIT;
    }

    /// <summary>
    /// Enter the Group Unit state for the heads up display
    /// </summary>
    /// <param name="count">Number of units selected</param>
    public void EnterGroupUnitState(int count)
    {
        groupUnitCount = count;
        State = state.GROUP_UNIT;
    }

    /// <summary>
    /// Updates the GUI Text on the top display
    /// </summary>
    private void UpdateTopDisplay()
    {
        minerals.text = "Minerals: " + PlayerData.minerals.ToString();
        manPower.text = "Man Power: " + PlayerData.manPower.ToString();
        unitCount.text = "Unit Count: " + PlayerData.unitCount.ToString();
        time.text = GetTime();
    }


    /// <summary>
    /// Gets the current formated time
    /// </summary>
    /// <returns>Returns a formated string of the current time</returns>
    private string GetTime()
    {
        int minutes = (int)Time.time / 60;
        int seconds = (int)Time.time % 60;

        string _minutes;
        if (minutes < 10)
            _minutes = "0" + minutes.ToString();
        else
            _minutes = minutes.ToString();

        string _seconds;
        if (seconds < 10)
            _seconds = "0" + seconds.ToString();
        else
            _seconds = seconds.ToString();

        return _minutes + ":" + _seconds;            
    }
}
