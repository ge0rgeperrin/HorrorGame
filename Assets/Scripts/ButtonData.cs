namespace Subterranea
{
    using System.Collections;
    using System.Collections.Generic;
    using Subterranea;
    using UnityEngine;

    public class ButtonData : MonoBehaviour
    {
        public function Function;
    
        public void Logic()
        {
            switch (Function)
            {
                case function.Play:
                    MainMenuManager.Instance.LobbyMenu.SetActive(true);
                    MainMenuManager.Instance.PlayMenu.SetActive(false);
                    break;
                case function.Quit:
                    Application.Quit();
                    break;
                case function.FindLobbies:
                    MainMenuManager.Instance.FindLobbies(5);
                    break;
                case function.BackToPlayMenu:
                    MainMenuManager.Instance.LobbyMenu.SetActive(false);
                    MainMenuManager.Instance.PlayMenu.SetActive(true);
                    break;
            }
        }
    
        public enum function
        {
            Play,
            Quit,
            FindLobbies,
            BackToPlayMenu
        }
    }
}