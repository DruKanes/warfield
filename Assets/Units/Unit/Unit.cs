using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    //unit specifications
    private float currentHealth;
    public float CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }

    private float mineralCost;
    public float MineralCost
    {
        get { return mineralCost; }
        set { mineralCost = value; }
    }

    private float manPowerCost;
    public float ManPowerCost
    {
        get { return manPowerCost; }
        set { manPowerCost = value; }
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool RequirementsMet()
    {
        return true;
    }

    public Vector4 SpawnLocation()
    {
        return transform.position;
    }
}
