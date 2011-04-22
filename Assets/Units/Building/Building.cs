using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    private enum state { DEFAULT, CREATE_UNIT }
    private state State;

    //building specifications
    public int MineralCost;
    public int ManPowerCost;
    public float MaxHealth;
    private float currentHealth;

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

    // Use this for initialization
    void Awake()
    {
        //set states
        State = state.DEFAULT;

        //initial creation of building
        validLocation = false;

        //find / add render objects, used for creating transparent effect
        if (renderer != null)
            renderList.Add(renderer);
        GetRenderObjects(transform); //set the renderArray, used to enable / disable the transparent look
        renderArray = (Renderer[])renderList.ToArray(typeof(Renderer));

        //save the originalColor this is used with the ghosting effect
        originalColor = new Color[renderArray.Length];
        for(int i=0; i<renderArray.Length; i++)
            originalColor[i] = renderArray[i].material.color;
    }

    // Update is called once per frame
    void Update()
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

    /// <summary>
    /// Recursively sets the renderArray with all Render Objects in the current object, this includes all children
    /// </summary>
    /// <param name="_transform">Current Transform object</param>
    private void GetRenderObjects(Transform _transform)
    {
        if (_transform == null)
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
}
