using UnityEngine;
using System.Collections;

public class HeadsUpDisplay : MonoBehaviour
{
    public GUITexture topBackground;
    public GUIText minerals;
    public GUIText manPower;
    public GUIText unitCount;
    public GUIText time;

    // Use this for initialization
    void Start()
    {
        //top display
        topBackground.pixelInset = new Rect(0, Screen.height, Screen.width, Screen.width / 100);
        minerals.pixelOffset = new Vector2(10, Screen.height - 5);
        manPower.pixelOffset = new Vector2(Screen.width / 4, Screen.height - 5);
        unitCount.pixelOffset = new Vector2(Screen.width / 2, Screen.height - 5);
        time.pixelOffset = new Vector2(Screen.width / 1.334F, Screen.height - 5);
    }

    // Update is called once per frame
    void Update()
    {
        minerals.text = "Minerals: " + PlayerData.minerals.ToString();
        manPower.text = "Man Power: " + PlayerData.manPower.ToString();
        unitCount.text = "Unit Count: " + PlayerData.unitCount.ToString();
        time.text = GetTime();

    }

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
