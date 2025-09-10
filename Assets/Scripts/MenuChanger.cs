using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MenuChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadGame1()
    {
        SceneManager.LoadScene("CityScene");
    }

    public void LoadGame2()
    {
        Debug.Log("Game 2 is still under development");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!");
    }
    public void BackMenu()
    {
        SceneManager.LoadScene("MainScene");
    }
}
