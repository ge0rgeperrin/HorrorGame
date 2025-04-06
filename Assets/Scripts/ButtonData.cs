using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour
{
    public function Function;
    public GameObject lobbyMenu;
    
    public void Logic()
    {
        switch (Function)
        {
            case function.Play:
                lobbyMenu.SetActive(true);
                break;
            case function.Quit:
                Application.Quit();
                break;
        }
    }
    
    public enum function
    {
        Play,
        Quit,
    }
}