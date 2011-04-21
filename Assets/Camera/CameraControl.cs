using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    //cursor icons
    private Rect buildCursor; //position of the cursor when in build mode
    public Texture2D buildCursorTexture; //regular build cursor
    public Texture2D buildCursor2Texture; //highlighted build cursor

    //edge of screen
    private enum edgeState { LEFT, LEFT_UP, LEFT_DOWN, UP, DOWN, RIGHT, RIGHT_UP, RIGHT_DOWN  }
    private edgeState EdgeState;
    public int edgeBuffer;
    public int screenMoveRate;

    //zoom camera
    public float zoomRate;
    public int zoomLimit;
    public float zoomOriginal;


    // Use this for initialization
    void Start()
    {
        zoomOriginal = Camera.mainCamera.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        //check for user input on the following functions
        MoveCamera(); //execute any movement that needs to be done on the camera
        ZoomCamera(); //zooms the camera in/out
    }

    /// <summary>
    /// Zooms the camera in/out
    /// </summary>
    private void ZoomCamera()
    {
        float scrollVal = Input.GetAxis("Mouse ScrollWheel");
        if (Camera.mainCamera.fieldOfView-scrollVal*zoomRate >= zoomLimit && Camera.mainCamera.fieldOfView-scrollVal*zoomRate <= zoomOriginal)
            Camera.mainCamera.fieldOfView -= scrollVal * zoomRate;
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
    /// Build state, moves the camera based on Horizontal/Vertical axis of controller
    /// </summary>
    /// <param name="cameraShoot">the camera for Shoot Mode</param>
    /*
private void BuildState()
{

float horizontalVal = Input.GetAxis("Horizontal" + player);
float verticalVal = Input.GetAxis("Vertical" + player);
if (horizontalVal > .25F || horizontalVal < -.25F)
    cameraBuild.Translate(new Vector3(screenMoveRate * horizontalVal, 0, 0) * Time.deltaTime);
if (verticalVal > .25F || verticalVal < -.25F)
    cameraBuild.Translate(new Vector3(0, 0, screenMoveRate * verticalVal) * Time.deltaTime);

// Boundary
if (cameraBuild.transform.position.x > maxX)
    cameraBuild.transform.position = new Vector3(maxX, cameraBuild.transform.position.y, cameraBuild.transform.position.z);
else if (cameraBuild.transform.position.x < minX)
    cameraBuild.transform.position = new Vector3(minX, cameraBuild.transform.position.y, cameraBuild.transform.position.z);

if (cameraBuild.transform.position.z > maxZ)
    cameraBuild.transform.position = new Vector3(cameraBuild.transform.position.x, cameraBuild.transform.position.y, maxZ);
else if (cameraBuild.transform.position.z < minZ)
    cameraBuild.transform.position = new Vector3(cameraBuild.transform.position.x, cameraBuild.transform.position.y, minZ);

//select tower
if (Input.GetButtonDown("A" + player))
{
    Ray ray = cameraBuild.GetChild(0).camera.ScreenPointToRay(new Vector3(buildCursor.x + 70, Screen.height - buildCursor.y - 15, 0));
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit))
    {
        if (hit.collider.tag == "Tower") //tower has been selected, change state to SELECT TOWER MENU and open the edit menu
        {
            State = state.EDIT_TOWER_MENU;
            //tower.gameObject.tag = "In Use";
            tower = hit.transform;
            //OpenMenu(editMenu); //open the edit menu
        }
        else //no tower selected, change state to BUILD TOWER MENU and open the build menu
        {
            State = state.BUILD_TOWER_MENU;
            //OpenMenu(buildMenu); //open the edit menu
        }
    }
    else //no tower selected, change state to BUILD TOWER MENU and open the build menu
    {
        State = state.BUILD_TOWER_MENU;
        //OpenMenu(buildMenu); //open the edit menu
    }
}
         
}

   

/// <summary>
/// Turns off ShootMode Camera and Turns on BuildMode Camera
/// </summary>
private void EnterBuildState()
{

            shootCursor = new Rect((Screen.width - shootCursorTexture.width) / 4, (Screen.height -
                shootCursorTexture.height) / 4F, shootCursorTexture.width, shootCursorTexture.height);

            buildCursor = new Rect((Screen.width - buildCursorTexture.width) / 4, (Screen.height -
                buildCursorTexture.height) / 4F, buildCursorTexture.width, buildCursorTexture.height);


}

/// <summary>
/// Build tower state, move right joystick to move tower and then press A to build it
/// </summary>
private void BuildTowerState()
{
//move camera
float horizontalVal = Input.GetAxis("Horizontal" + player);
float verticalVal = Input.GetAxis("Vertical" + player);
if (horizontalVal > .25F || horizontalVal < -.25F)
{
    cameraBuild.Translate(new Vector3(screenMoveRate * horizontalVal, 0, 0) * Time.deltaTime);
    tower.Translate(new Vector3(screenMoveRate * horizontalVal, 0, 0) * Time.deltaTime); //move the tower with the camera
}
if (verticalVal > .25F || verticalVal < -.25F)
{
    cameraBuild.Translate(new Vector3(0, 0, screenMoveRate * verticalVal) * Time.deltaTime);
    tower.Translate(new Vector3(0, 0, screenMoveRate * verticalVal) * Time.deltaTime); //move the tower with the camera
}

//move tower location
horizontalVal = Input.GetAxis("Horizontal Rotation" + player);
verticalVal = Input.GetAxis("Vertical Rotation" + player);
Vector3 screenPos = cameraBuild.GetChild(0).camera.WorldToScreenPoint(tower.position);

if (horizontalVal > .15F) //move tower to the right
{
    float val = horizontalVal * Time.deltaTime * 20;
    if (screenPos.x + val < xMax_Tower)
        tower.Translate(val, 0, 0);
}
else if (horizontalVal < -.15F) //move tower to the left
{
    float val = horizontalVal * Time.deltaTime * 20;
    if (screenPos.x + val > xMin_Tower)
        tower.Translate(val, 0, 0);
}
if (verticalVal > .15F) //move tower down
{
    float val = verticalVal * Time.deltaTime * -20;
    if (screenPos.y + val > yMin_Tower)
        tower.Translate(0, 0, val);
}
else if (verticalVal < -.15F) //move tower up
{
    float val = verticalVal * Time.deltaTime * -20;
    if (screenPos.y + val < yMax_Tower)
        tower.Translate(0, 0, val);
}

//Presses A to place building or Presses B to cancel the operation
if (Input.GetButtonDown("A" + player) && tower.GetComponent<Tower>().IsBuildValid()) //place tower and go back to build mode
{
    tower.GetComponent<Tower>().OriginalPlayer = player; //used to say who built the tower
    tower.GetComponent<Tower>().ExitBuild();
    State = state.BUILD;

    //transform.parent.audio.clip = click;
    transform.parent.audio.PlayOneShot(click);
}
else if (Input.GetButtonDown("B" + player)) //cancel the tower placement and go back to build mode
{
    PlayerData.gold += tower.GetComponent<Tower>().GoldCost;
    Destroy(tower.gameObject);
    State = state.BUILD;
}
}

void OnGUI()
{


       

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
 * */
}
