using UnityEngine;
using System.Collections;

public class UserControl : MonoBehaviour
{
    
    private enum state { DEFAULT, BUILD, BUILD_MENU, BUILDING_MENU, NONE }
    private state State;

    public enum selectedState { NONE, BUILDING, UNIT }
    public selectedState SelectedState;

    //building / unit variables
    public Transform BuildingParent;
    public Transform UnitParent;
    private Transform building;
    private Transform unit;
    private ArrayList unitGroup = new ArrayList();
    //private Transform[] unitGroup = new Transform[100];
    private int selectedUnitCount;

    //selection box
    public Texture2D selectionTexture;
    private GUIStyle selectionStyle = new GUIStyle();
    private bool startBox;
    private float x1; //initial x position
    private float x2; //initial y position
    private float y1; //ending x position
    private float y2; //ending y position

    //cursor icons
    private Rect buildCursor; //position of the cursor when in build mode
    public Texture2D buildCursorTexture; //regular build cursor
    public Texture2D buildCursor2Texture; //highlighted build cursor

    //edge of screen
    private enum edgeState { LEFT, LEFT_UP, LEFT_DOWN, UP, DOWN, RIGHT, RIGHT_UP, RIGHT_DOWN }
    private edgeState EdgeState;
    public int edgeBuffer;
    public int screenMoveRate;

    //zoom camera
    public float zoomRate;
    public int zoomLimit;
    private float zoomOriginal;

    //Menu variables
    public int menuButtonWidth;
    public int menuButtonHeight;
    public int menuButtonBufferY;
    public int menuButtonBufferX;
    public string[] buildMenuStrings;

    private int menuWidth;
    private int buildMenuHeight;
    private int[,] buildMenuButtonPos; //[icon number, {x,y}]
    private int[] buildMenuPos = new int[2]; //[0] = x coordinate, [1]= y coordinate

    //heads up display 
    public Transform hud;

    // Use this for initialization
    void Start()
    {
        //set state to default
        State = state.DEFAULT;
        SelectedState = selectedState.NONE;

        //set intial zoom values
        zoomOriginal = Camera.mainCamera.fieldOfView;

        //build Menu initialization
        menuWidth = 20 + 20 + menuButtonWidth;
        buildMenuHeight = 40 + (buildMenuStrings.Length * (menuButtonHeight + menuButtonBufferY));
        buildMenuPos[0] = Screen.width - menuWidth - 10;
        buildMenuPos[1] = Screen.height - buildMenuHeight;
        buildMenuButtonPos = new int[buildMenuStrings.Length, 2];
        for (int i = 0; i < buildMenuStrings.Length; i++)
        {
            buildMenuButtonPos[i, 0] = buildMenuPos[0] + menuButtonBufferX;
            buildMenuButtonPos[i, 1] = buildMenuPos[1] + (i * menuButtonBufferY) + (i * menuButtonHeight) + menuButtonBufferX * 2;
        }

        //selection box
        selectedUnitCount = 0;
        selectionStyle.normal.background = selectionTexture;
        startBox = false;

    }

    // Update is called once per frame
    void Update()
    {
        //check for user input on the following functions
        MoveCamera(); //execute any movement that needs to be done on the camera
        ZoomCamera(); //zooms the camera in/out
        MoveUnits(); //move units to target location

        //check if the build button was pressed
        if (Input.GetButtonDown("Build"))
        {
            State = state.BUILD_MENU;
            BuildingParent.GetComponent<BuildingParent>().EnterDefaultState();
            UnitParent.GetComponent<UnitParent>().EnterDefaultState();
            hud.GetComponent<HeadsUpDisplay>().EnterDefaultState();
        }

        switch (State)
        {
            case state.BUILD:
                BuildState();
                break;
            default:
                OverBuilding(); //checks if user clicked a building
                OverUnit(); //checks if user clicked a unit
				PauseMenu(); //checks if user paused game
                break;
        }
    }

	//pauses game 
	private void PauseMenu()
	{
		 if (Input.GetKeyDown(KeyCode.Escape)) //pause the game
        {
			//code that actually pauses it and not just opens a menu, careful about switching scenes cause it wont preserve everything.
			
		}
	}
	
    /// <summary>
    /// Zooms the camera in/out
    /// </summary>

    private void ZoomCamera()
    {
        float scrollVal = Input.GetAxis("Mouse ScrollWheel");
        if (Camera.main.fieldOfView - scrollVal * zoomRate >= zoomLimit && Camera.main.fieldOfView - scrollVal * zoomRate <= zoomOriginal)
            Camera.main.fieldOfView -= scrollVal * zoomRate;
    }

    /// <summary>
    /// Executes all movement of the camera
    /// </summary>
    private void MoveCamera()
    {
        //move the camera if arrow keys are pressed
        float horizontalVal = Input.GetAxis("Horizontal");
        float verticalVal = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(screenMoveRate * horizontalVal, 0, screenMoveRate * verticalVal) * Time.deltaTime);

        //move camera as long as mouse is at edge of screen
        if (CursorAtEdge())
        {
            switch (EdgeState)
            {
                case edgeState.LEFT:
                    transform.Translate(new Vector3(-screenMoveRate, 0, 0) * Time.deltaTime);
                    break;
                case edgeState.LEFT_UP:
                    transform.Translate(new Vector3(-screenMoveRate, 0, screenMoveRate) * Time.deltaTime);
                    break;
                case edgeState.LEFT_DOWN:
                    transform.Translate(new Vector3(-screenMoveRate, 0, -screenMoveRate) * Time.deltaTime);
                    break;
                case edgeState.UP:
                    transform.Translate(new Vector3(0, 0, screenMoveRate) * Time.deltaTime);
                    break;
                case edgeState.DOWN:
                    transform.Translate(new Vector3(0, 0, -screenMoveRate) * Time.deltaTime);
                    break;
                case edgeState.RIGHT:
                    transform.Translate(new Vector3(screenMoveRate, 0, 0) * Time.deltaTime);
                    break;
                case edgeState.RIGHT_UP:
                    transform.Translate(new Vector3(screenMoveRate, 0, screenMoveRate) * Time.deltaTime);
                    break;
                case edgeState.RIGHT_DOWN:
                    transform.Translate(new Vector3(screenMoveRate, 0, -screenMoveRate) * Time.deltaTime);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Move units to target location specified by right click
    /// </summary>
    private void MoveUnits()
    {
        if (Input.GetButtonDown("Right Click") && SelectedState == selectedState.UNIT) //move units to target location
        {
            //get the location that was clicked
            Vector3 target;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);
                target = new Vector3(hit.point.x, hit.point.y + 5, hit.point.z);
                //send the move signal to the units
                if (selectedUnitCount > 1)
                {
                    for (int i = 0; i < unitGroup.Count; i++)
                        ((Transform)unitGroup[i]).GetComponent<Unit>().EnterMovingState(target);
                }
                else
                {
                    unit.GetComponent<Unit>().EnterMovingState(target);
                }
            }
        }

    }

    /// <summary>
    /// Checks if the user created a selection box
    /// </summary>
    private void SelectionBox()
    {
        if (!startBox && Input.GetButtonDown("Left Click")) //user has not started the selection box, and left clicked
        {
            x1 = Input.mousePosition.x;
            y1 = Input.mousePosition.y;
            startBox = true;
        }
        else if (startBox) //selection box has been started
        {
            if (Input.GetButton("Left Click")) //mouse is held down, draw the box
            {
                x2 = Input.mousePosition.x;
                y2 = Input.mousePosition.y;
                GUI.Box(new Rect(x1, Screen.height - y1, x2 - x1, y1 - y2), "", selectionStyle);
            }
            else //user let go of mouse, select all objects within the box
            {
                x2 = Input.mousePosition.x;
                y2 = Input.mousePosition.y;
                startBox = false;

                //rearrange the coordinates so everything is standardized
                if (x2 < x1)
                {
                    float temp = x2;
                    x2 = x1;
                    x1 = temp;
                }

                if (y2 < y1)
                {
                    float temp = y2;
                    y2 = y1;
                    y1 = temp;
                }
                SelectUnitsFromBox();
            }
        }
    }

    /// <summary>
    /// Select units after a selection box has been performed
    /// </summary>
    private void SelectUnitsFromBox()
    {
        //convert selection box to world points by using rays
        Vector3 s1 = new Vector3(x1, y1, 0);
        Vector3 s2 = new Vector3(x2, y2, 0);
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(s1);
        LayerMask unitsMask = 1 << 8;//the ray will ignore all colliders except the ones with the "Terrain" layer
        unitsMask = ~unitsMask;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000, unitsMask))
            s1 = hit.point;
        ray = Camera.main.ScreenPointToRay(s2);
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000, unitsMask))
            s2 = hit.point;

        selectedUnitCount = 0;
        unitGroup.Clear();
        GameObject[] allUnits = GameObject.FindGameObjectsWithTag("Unit"); //gather all Unit objects
        for (int i = 0; i < allUnits.Length; i++)
        {
            Vector3 pos = allUnits[i].transform.position;
            if (pos.x <= s2.x && pos.x >= s1.x && pos.z <= s2.z && pos.z >= s1.z) //within the selection box
            {
                selectedUnitCount++;
                unitGroup.Add(allUnits[i].transform);
            }
        }

        if (selectedUnitCount == 1) //only 1 unit selected
        {
            unit = unitGroup[0] as Transform;
            if (UnitParent.GetComponent<UnitParent>().OpenUnitMenu(unit))
            {
                //properly set states and reset building and current states
                State = state.NONE;
                SelectedState = selectedState.UNIT;
                selectedUnitCount = 1;
                BuildingParent.GetComponent<BuildingParent>().EnterDefaultState();
                hud.GetComponent<HeadsUpDisplay>().EnterSingleUnitState(unit);
            }
        }
        else if (selectedUnitCount > 1) //group selection
        {
            unit = unitGroup[0] as Transform;
            if (UnitParent.GetComponent<UnitParent>().OpenUnitMenu(unit))
            {
                State = state.NONE;
                SelectedState = selectedState.UNIT;
                BuildingParent.GetComponent<BuildingParent>().EnterDefaultState();
                hud.GetComponent<HeadsUpDisplay>().EnterGroupUnitState(selectedUnitCount);
            }
        }

    }

    /// <summary>
    /// Checks if currently over a building when user gives input
    /// </summary>
    private void OverBuilding()
    {
        //check to see if you are hovering over a building

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "Building" && Input.GetButtonDown("Left Click")) //selected a building
            {
                if (BuildingParent.GetComponent<BuildingParent>().OpenBuildingMenu(hit.collider.transform))
                {
                    //properly set states and resest unit and current state
                    State = state.NONE;
                    SelectedState = selectedState.BUILDING;
                    building = hit.collider.transform;
                    UnitParent.GetComponent<UnitParent>().EnterDefaultState();
                    hud.GetComponent<HeadsUpDisplay>().EnterBuildingState(building);
                }
            }
        }
    }

    /// <summary>
    /// Checks if currently over a unit when user gives input
    /// </summary>
    private void OverUnit()
    {
        //check to see if you are hovering over a unit

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "Unit" && Input.GetButtonDown("Left Click")) //selected a unit
            {
                if (UnitParent.GetComponent<UnitParent>().OpenUnitMenu(hit.collider.transform))
                {
                    //properly set states and reset building and current states
                    State = state.NONE;
                    SelectedState = selectedState.UNIT;
                    unit = hit.collider.transform;
                    selectedUnitCount = 1;
                    BuildingParent.GetComponent<BuildingParent>().EnterDefaultState();
                    hud.GetComponent<HeadsUpDisplay>().EnterSingleUnitState(unit);
                }
            }
        }
    }

    /// <summary>
    /// Checks if the cursor is at the edge of the screen, if so then the EdgeState is updated
    /// </summary>
    private bool CursorAtEdge()
    {
        //NOTE (0,0) is the bottom left edge of the screen

        //check if mouse is at the left edge of the screen
        if (Input.mousePosition.x - edgeBuffer <= 0)
        {
            //check if at top or bottom of the screen as well
            if (Input.mousePosition.y + edgeBuffer >= Screen.height)
                EdgeState = edgeState.LEFT_UP;
            else if (Input.mousePosition.y - edgeBuffer <= 0)
                EdgeState = edgeState.LEFT_DOWN;
            else
                EdgeState = edgeState.LEFT;
            return true;
        }

        //check if mouse is at the right edge of the screen
        if (Input.mousePosition.x + edgeBuffer >= Screen.width)
        {
            //check if at top or bottom of the screen as well
            if (Input.mousePosition.y + edgeBuffer >= Screen.height)
                EdgeState = edgeState.RIGHT_UP;
            else if (Input.mousePosition.y - edgeBuffer <= 0)
                EdgeState = edgeState.RIGHT_DOWN;
            else
                EdgeState = edgeState.RIGHT;
            return true;
        }

        //check if mouse is at the top edge of the screen
        if (Input.mousePosition.y + edgeBuffer >= Screen.height)
        {
            EdgeState = edgeState.UP;
            return true;
        }

        //check if mouse is at the bottom edge of the screen
        if (Input.mousePosition.y - edgeBuffer <= 0)
        {
            EdgeState = edgeState.DOWN;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Cancels current menu and brings screen back to default state
    /// </summary>
    public void EnterDefaultState()
    {
        State = state.DEFAULT;
        SelectedState = selectedState.NONE;
    }

    private void BuildState()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //cancel the creation of the building
        {
            //re add the resources
            PlayerData.manPower += building.GetComponent<Building>().ManPowerCost;
            PlayerData.minerals += building.GetComponent<Building>().MineralCost;

            //decrement the tower count on BuildingParent

            Destroy(building.gameObject); //destroy the tower component
            Screen.showCursor = true; //display the cursor again

            State = state.DEFAULT;
            return;
        }

        //update the (x,z) position of the bulding
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask unitsMask = 1 << 8;//the ray will ignore all colliders except the ones with the "Terrain" layer
        unitsMask = ~unitsMask;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000, unitsMask))
        {
            building.position = hit.point;

            bool valid = true;
            //now update the y position of the tower
            if (Physics.Raycast(new Vector3(building.position.x,building.position.y + 50, building.position.z), Vector3.down, out hit, 1000, unitsMask))
            {
                building.position = new Vector3(hit.point.x, hit.point.y + building.collider.bounds.extents.y, hit.point.z);

                //mine shafts can only be built ontop of mineralsAdo
                if (building.name.Contains("Mine Shaft"))
                {
                    if (Physics.Raycast(new Vector3(building.position.x, building.position.y + 10, building.position.z), Vector3.down * 100, out hit))
                    {
                        if (hit.collider.tag == "Minerals")
                            valid = true;
                        else
                            valid = false;
                    }
                }
            }

            //check to see if your near any other buildings
            if (!building.name.Contains("Mine Shaft"))
            {
                float radius = building.collider.bounds.extents.x;
                if (building.collider.bounds.extents.z > radius)
                    radius = building.collider.bounds.extents.z;
                Collider[] colliders = Physics.OverlapSphere(building.position, radius);
                foreach (Collider surrounding in colliders)
                {
                    if ((surrounding != building.collider) && (surrounding.tag == "Building" || surrounding.tag == "Unit" || surrounding.tag == "Minerals")) //too close to another building or unit
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid && building.GetComponent<Building>().ValidLocation) //unit in the way and its not already in a false state, no need to call twice
                building.GetComponent<Building>().SetInValidLocation();
            else if (valid && !building.GetComponent<Building>().ValidLocation)
                building.GetComponent<Building>().SetValidLocation();
        }


        //building has been placed
        if (Input.GetButtonDown("Left Click") && building.GetComponent<Building>().ValidLocation)
        {
            building.collider.isTrigger = false;
            building.GetComponent<Building>().DisableTransparentEffect();
            Screen.showCursor = true;
            State = state.DEFAULT;
        }
    }

    /// <summary>
    /// Draws the GUI items, this is called every fram
    /// </summary>
    void OnGUI()
    {
        switch (State)
        {
            case state.DEFAULT:
                DefaultMenuGUI();
                break;
            case state.BUILD_MENU:
                BuildMenuGUI();
                break;
            default:
                break;
        }

        SelectionBox(); //check if the user is performing a selection box
    }

    private void BuildMenuGUI()
    {
        GUI.Box(new Rect(buildMenuPos[0], buildMenuPos[1], menuWidth, buildMenuHeight), "Build Menu");

        //draw all the build buttons
        for (int i = 0; i < buildMenuStrings.Length; i++)
        {
            if (GUI.Button(new Rect(buildMenuButtonPos[i, 0], buildMenuButtonPos[i, 1], menuButtonWidth, menuButtonHeight), buildMenuStrings[i]))
            {
                if (buildMenuStrings[i] == "Close")
                    State = state.DEFAULT;
                else
                {
                    Transform _building = BuildingParent.GetComponent<BuildingParent>().CreateBuilding(buildMenuStrings[i]);
                    if (_building != null)
                    {
                        building = Instantiate(_building) as Transform; //create the building
                        building.collider.isTrigger = true;
                        building.GetComponent<Building>().EnableTransparentEffect(); //enable ghost effect
                        Screen.showCursor = false;
                        State = state.BUILD;
                        return;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) //escape button
                State = state.DEFAULT;
        }
    }

    private void DefaultMenuGUI()
    {
        GUI.Box(new Rect(Screen.width - menuWidth - 10, Screen.height - menuButtonHeight - 20, menuWidth, buildMenuHeight), "");
        if (GUI.Button(new Rect(Screen.width - menuButtonWidth - menuButtonBufferX - 10, Screen.height - menuButtonHeight - 10, menuButtonWidth, menuButtonHeight), "Build"))
        {
            State = state.BUILD_MENU;
        }
    }


    /*

    //Build Cursor
    if (State == state.BUILD)
    {
        //move the cursor
        float horizontalVal = Input.GetAxis("Horizontal Rotation" + player);
        float verticalVal = Input.GetAxis("Vertical Rotation" + player);
        if (horizontalVal > .15F || horizontalVal < -.15F)
        {
            float val = horizontalVal * Time.deltaTime * 150;
            if (buildCursor.x + val < xMax_Cursor && buildCursor.x + val > xMin_Cursor)
                buildCursor.x += val;
        }
        if (verticalVal > .15F || verticalVal < -.15F)
        {
            float val = verticalVal * Time.deltaTime * 150;
            if (buildCursor.y + val < yMax_Cursor && buildCursor.y + val > yMin_Cursor)
                buildCursor.y += val;
        }

        //check to see if you are hovering over a tower, if so change the color to red cursor
        if (Input.GetButtonDown("B" + player))
            print(Input.mousePosition);
        //print("default " + buildCursor.x + " " + (Screen.height - buildCursor.y));
        //print((buildCursor.x + 70) + " " + (Screen.height - buildCursor.y - 15));
        Ray ray = cameraBuild.GetChild(0).camera.ScreenPointToRay(new Vector3(buildCursor.x + 70, Screen.height - buildCursor.y - 15, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "Tower")
                GUI.DrawTexture(buildCursor, buildCursor2Texture);
            else
                GUI.DrawTexture(buildCursor, buildCursorTexture, ScaleMode.StretchToFill);
        }
        else
            GUI.DrawTexture(buildCursor, buildCursorTexture);
    }
    }
     */
}
