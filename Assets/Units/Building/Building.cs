using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    public enum state { DEFAULT, CREATE_UNIT, BUILDING }
    public state State;

    private Transform unit;
    public Transform Unit
    {
        get { return unit; }
    }

    //initial building progress
    public int BuildTime;
    private float startBuildTime;
    public float StartBuildTime
    {
        get { return startBuildTime; }
    }

    //building specifications
    public int MineralCost;
    public int ManPowerCost;
    public float MaxHealth;
    private float currentHealth;
    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    private Transform spawnPoint;
    public Transform SpawnPoint
    {
        get { return spawnPoint; }
    }

    //ghost effect
    private ArrayList renderList = new ArrayList(); //hold the render materials that will be used to create the transparent look when building
    private Renderer[] renderArray; //array that will be created by renderList, holds the render objects
    private Color[] originalColor;

    //create building variables
    private bool validLocation;
    public bool ValidLocation
    {
        get { return validLocation; }
    }

    //create unit variables
    private Queue unitsQueue = new Queue();
    public Queue UnitsQueue
    {
        get { return unitsQueue; }
        set { unitsQueue = value; }
    }

    // Use this for initialization
    void Awake()
    {
        //set states
        State = state.DEFAULT;

        currentHealth = MaxHealth;

        //set the spawn location for the units
        spawnPoint = transform.FindChild("SpawnPoint");

        //find / add render objects, used for creating transparent effect
        if (renderer != null)
            renderList.Add(renderer);
        GetRenderObjects(transform); //set the renderArray, used to enable / disable the transparent look
        renderArray = (Renderer[])renderList.ToArray(typeof(Renderer));

        //save the originalColor this is used with the ghosting effect
        originalColor = new Color[renderArray.Length];
        for(int i=0; i<renderArray.Length; i++)
            originalColor[i] = renderArray[i].material.color;

        SetInValidLocation();
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case state.DEFAULT:
                DefaultState();
                break;
            case state.BUILDING:
                BuildingState();
                break;
            case state.CREATE_UNIT:
                CreateUnitState();
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
        if (Time.time - startBuildTime > BuildTime)
            State = state.DEFAULT;
    }

    private void CreateUnitState()
    {
        if (unit.GetComponent<Unit>().IsBuilt())
        {
            if (unitsQueue.Count > 0)
            {
                unit = (Transform)unitsQueue.Dequeue();
                unit.gameObject.SetActiveRecursively(true);
                unit.GetComponent<Unit>().EnterBuildingState();
            }
            else //nore more units to build
            {
                State = state.DEFAULT;
            }
        }
    }

    /// <summary>
    /// Checks to see if the requirements are met to create this building
    /// </summary>
    /// <returns>Returns true if requirements are met, false if a requirement is not met</returns>
    public bool RequirementsMet()
    {
        return true;
    }

    /// <summary>
    /// Recursively sets the renderArray with all Render Objects in the current object, this includes all children
    /// </summary>
    /// <param name="_transform">Current Transform object</param>
    private void GetRenderObjects(Transform _transform)
    {
        if (_transform == null) //base case
            return;

        foreach (Transform child in _transform) //check all children for render objects
        {
            if (child.renderer != null) //a renderer exists so save this object
                renderList.Add(child.renderer);
            GetRenderObjects(child); //recursively check if childrens children have render objects
        }
    }

    /// <summary>
    /// Turns on the transparent look on the the render materials
    /// </summary>
    public void EnableTransparentEffect()
    {
        for (int i = 0; i < renderArray.Length; i++)
        {
            //turn on transparent effect
            foreach (Material material in ((Renderer)renderArray[i]).materials)
            {
                material.shader = Shader.Find("Transparent/Diffuse");
                material.color = new Color(material.color.r, material.color.g, material.color.b, .5F);
            }
        }
    }

    /// <summary>
    /// Turns off the transparent look on the render materials
    /// </summary>
    public void DisableTransparentEffect()
    {
        for (int i = 0; i < renderArray.Length; i++)
        {
            //turn off transparent effect
            foreach (Material material in ((Renderer)renderArray[i]).materials)
            {
                material.shader = Shader.Find("Diffuse");
                material.color = new Color(material.color.r, material.color.g, material.color.b, 1F);
            }
        }
        RestoreColor();

        State = state.BUILDING;
        startBuildTime = Time.time;
    }
    
    /// <summary>
    /// Changes the color of all materials in this building
    /// </summary>
    /// <param name="_color">Color you wish to set the materials to</param>
    private void ChangeColor(Color _color)
    {
        for (int i = 0; i < renderArray.Length; i++)
        {
            foreach (Material material in ((Renderer)renderArray[i]).materials)
                material.color = new Color(_color.r, _color.g, _color.b, material.color.a);
        }
    }

    /// <summary>
    /// Restores the color of all materials to its original colors
    /// </summary>
    private void RestoreColor()
    {
        for (int i = 0; i < renderArray.Length; i++)
        {
            foreach (Material material in ((Renderer)renderArray[i]).materials)
                material.color = originalColor[i];
        }
    }

    /// <summary>
    /// Places the building in a valid build state
    /// </summary>
    public void SetValidLocation()
    {
        validLocation = true;
        ChangeColor(Color.green);
    }

    /// <summary>
    /// Places the building in a invalid build state
    /// </summary>
    public void SetInValidLocation()
    {
        validLocation = false;
        ChangeColor(Color.red);
    }

    /// <summary>
    /// Returns the status of the building
    /// </summary>
    /// <returns>True if the building is built, false if the building is still being created</returns>
    public bool IsBuilt()
    {
        if (State == state.BUILDING)
            return false;

        return true;
    }

    /// <summary>
    /// Signals the building that a unit has been created by this building
    /// </summary>
    /// <param name="_unit">The unit that was created</param>
    public void CreatedUnit(Transform _unit)
    {
        if (State != state.CREATE_UNIT) //unit is not already being created
        {
            State = state.CREATE_UNIT;
            unit = _unit;
            unit.gameObject.SetActiveRecursively(true);
            unit.GetComponent<Unit>().EnterBuildingState();
        }
        else //unit is already being created
        {
            unitsQueue.Enqueue(_unit);
        }
    }
}
