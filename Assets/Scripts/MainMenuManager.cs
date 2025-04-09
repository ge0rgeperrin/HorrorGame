using System;
using System.Collections;
using Subterranea;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    private void Start()
    {
        Instance = this;
        StartCoroutine(MainMenuSequence());
    }

    private IEnumerator MainMenuSequence()
    {
        PlayMenu.SetActive(false);
        yield return new WaitUntil(() => LoginManager.LoggedIn);
        PlayMenu.SetActive(true);
    }

    public GameObject PlayMenu;
}