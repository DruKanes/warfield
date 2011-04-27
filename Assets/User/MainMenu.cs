using UnityEngine;
using System.Collections;
class MainMenu : MonoBehaviour
    {

        void OnGUI()
        {
            // Make a background box
           // GUI.Box(new Rect((Screen.width/2-30), (Screen.height/2-20), 100, 90), "");

            // Make a button if its pressed load the MainLevel
            if (GUI.Button(new Rect((Screen.width/2-50), (Screen.height/2+20), 100, 20), "Start Game"))
            {
                Application.LoadLevel("MainLevel");
            }
        }
    }

