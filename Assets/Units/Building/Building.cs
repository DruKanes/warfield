using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    //building specifications
    public int MineralCost;
    public int ManPowerCost;
    public float MaxHealth;

    private float currentHealth;
    

    // Use this for initialization
    void Start()
    {

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
}
