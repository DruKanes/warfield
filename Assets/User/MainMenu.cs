using UnityEngine;
using System.Collections;
class MainMenu : MonoBehaviour
    {

        void OnGUI()
        {
            // Make a background box
            GUI.Box(new Rect(10, 10, 100, 90), "");

            // Make a button if its pressed load the MainLevel
            if (GUI.Button(new Rect(20, 40, 80, 20), "Start Game"))
            {
                Application.LoadLevel("MainLevel");
            }
        }
    }

