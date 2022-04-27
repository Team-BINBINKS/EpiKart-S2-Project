using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NewSceneManager : MonoBehaviour
{

    public GameObject canvasMain;
    public GameObject canvasLobby;
    public GameObject canvasSettings;

    public void ShowLobby()
    {
        canvasLobby.SetActive(true);
    }
    public void HideLobby()
    {
        canvasLobby.SetActive(true);
    }

    public void GoBack()
    {
        if (canvasSettings.activeSelf)
        {
            canvasSettings.SetActive(false);
        }
        else if (canvasLobby.activeSelf)
        {
            canvasLobby.SetActive(false);
        }
    }

    public void GoToSettings()
    {
        canvasSettings.SetActive(true);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }


    // public void JoinRoom()


    // GO from main screen to lobby 
    // -> if creator and not creator
    // go to settings from anywhere
    // save state of animations
}
