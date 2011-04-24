using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    public enum state { NONE, BUILDING, MOVING, DEFAULT }
    public state State;

    //initial building progress
    public int BuildTime;
    private float startBuildTime;
    public float StartBuildTime
    {
        get { return startBuildTime; }
    }

    //unit specifications
    private Vector3 target;
    public Vector3 Target
    {
        set { target = value; }
    }
    public float dist;
    public float turnRot;
    public float fence;
    public float speed;
    public int MineralCost;
    public int ManPowerCost;
    public float MaxHealth;
    private float currentHealth;
    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    private Transform building;
    public Transform Building
    {
        set { building = value; }
    }

    //ghost effect
    private ArrayList renderList = new ArrayList(); //hold the render materials that will be used to create the transparent look when building
    private Renderer[] renderArray; //array that will be created by renderList, holds the render objects
    private Color[] originalColor;

    // Use this for initialization
    void Awake()
    {
        //set States
        State = state.NONE;

        currentHealth = MaxHealth;

        //find / add render objects, used for creating transparent effect
        if (renderer != null)
            renderList.Add(renderer);
        GetRenderObjects(transform); //set the renderArray, used to enable / disable the transparent look
        renderArray = (Renderer[])renderList.ToArray(typeof(Renderer));
        EnableTransparentEffect();

        gameObject.SetActiveRecursively(false);
    }

    void Start()
    {
        //place the unit next to the building it came from
        transform.position = building.GetComponent<Building>().SpawnPoint.position;
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHeight();

        switch (State)
        {
            case state.BUILDING:
                BuildingState();
                break;
            case state.MOVING:
                MovingState();
                break;
            case state.DEFAULT:
                DefaultState();
                break;
            default:
                break;
        }
    }

    private void BuildingState()
    {
        if (Time.time - startBuildTime > BuildTime)
        {
            DisableTransparentEffect();
            State = state.DEFAULT;
        }
    }

    public void EnterMovingState(Vector3 _target)
    {
        State = state.MOVING;
        target = _target;
    }

    private void MovingState()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        Vector3 lookDirection;
        int key;
        if (Physics.Raycast(transform.position, fwd, dist))
        {
            if (Physics.Raycast(transform.position, fwd + new Vector3(0, 45, 0), dist))
            {
                if (Physics.Raycast(transform.position, fwd - new Vector3(0, 45, 0), dist))
                {
                    if (Random.Range(1, 2) > 1.5F)
                        key = 2;
                    else key = 1;

                    switch (key)
                    {
                        case 1: transform.Rotate(Vector3.up, turnRot); break;
                        case 2: transform.Rotate(Vector3.up, -turnRot); break;
                        default: break;
                    }
                }
                else
                {
                    transform.Rotate(Vector3.up, turnRot);
                }
            }
            else
            {
                if (Physics.Raycast(transform.position, fwd - new Vector3(0, 45, 0), dist))
                {
                    transform.Rotate(Vector3.up, -turnRot);
                }
                else
                {
                    if (Random.Range(1, 2) > 1.5F)
                        key = 2;
                    else key = 1;

                    switch (key)
                    {
                        case 1: transform.Rotate(Vector3.up, turnRot); break;
                        case 2: transform.Rotate(Vector3.up, -turnRot); break;
                        default: break;
                    }
                }
            }
        }
        else
        {
            if (Vector3.Distance(target, transform.position) > fence) //keep moving forward
            {
                transform.position = transform.position + fwd * speed * Time.deltaTime;
            }
            else //reached destination
            {
                State = state.DEFAULT;
            }

            lookDirection = target - transform.position;

            if (!Physics.Raycast(transform.position, lookDirection, dist))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection.normalized), Time.deltaTime * 2);
            }
        }
    }

    private void DefaultState()
    {

    }

    /// <summary>
    /// Checks to see if the requirements are met to create this building
    /// </summary>
    /// <returns>Returns true if requirements are met, false if a requirement is not met</returns>
    public bool RequirementsMet()
    {
        return true;
    }

    private void Spawn()
    {
        float radius = collider.bounds.extents.x;
        if(collider.bounds.extents.z > radius)
            radius = collider.bounds.extents.z;

        //check to see if your near any other units
        
        bool valid = true;
        float start = System.DateTime.Now.Second;
        do
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider surrounding in colliders)
            {
                if (surrounding.tag == "Building" || surrounding.tag == "Unit") //touching another unit or building
                {
                    print(Time.time + " hit another unit");
                    valid = false;
                    break;
                }
            }
        } while (!valid && System.DateTime.Now.Second - start < 0);        
    }


    /// <summary>
    /// Call this every frame, will make sure the unit is on the ground
    /// </summary>
    private void UpdateHeight()
    {
        //update the y position of the unit
        RaycastHit hit;
        LayerMask unitsMask = 1 << 8;//the ray will ignore all colliders except the ones with the "Terrain" layer
        unitsMask = ~unitsMask;
        if (Physics.Raycast(transform.position, Vector3.down * 100, out hit, 1000, unitsMask))
            transform.position = new Vector3(transform.position.x, hit.point.y + collider.bounds.extents.y, transform.position.z);
    }

    /// <summary>
    /// Returns the status of the unit
    /// </summary>
    /// <returns>True if the unit is built, false if the unit is still being created</returns>
    public bool IsBuilt()
    {
        if (State == state.BUILDING || State == state.NONE)
            return false;

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
                material.color = new Color(material.color.r, material.color.g, material.color.b, 0);
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
    }

    /// <summary>
    /// Called when the unit is up for creation
    /// </summary>
    public void EnterBuildingState()
    {
        startBuildTime = Time.time;
        State = state.BUILDING;
    }
}
