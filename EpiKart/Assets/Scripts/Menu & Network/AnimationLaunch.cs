using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities
using UnityEngine.SceneManagement;


public class AnimationLaunch : MonoBehaviourPunCallbacks
{
    #region Scripts Arguments
    public Animator transition;
    #endregion

    void Update()
    {
        // Launch starting animation on input
        if (Input.anyKey) LaunchAnimation("clickTrigger");
    }

    //private void Awake()
    //{
    //    Application.targetFrameRate = 60;

    //}

    // Launches an animation specified in argument
    public void LaunchAnimation(string trigger)
    {
        // Launches animation in Animator section
        transition.SetTrigger(trigger);
    }

    // Goes to specified room (on network)
    public void GotoScene(string scene)
    {
        PhotonNetwork.LoadLevel(scene);
    }

    public void LaunchAnimationDiconnect(string trigger)
    {
        // Launches animation in Animator section
        transition.SetTrigger(trigger);
        PhotonNetwork.Disconnect();
    }

    // Goes to specified room (on local)
    public void LoadScene(string parameterSceneName)
    {
        SceneManager.LoadScene(parameterSceneName);
    }

    // Goes to the specified waiting room (on local)
    public void LeaveWaitingRoom(string parameterSceneName)
    {
        SceneManager.LoadScene(parameterSceneName);
        PhotonNetwork.Disconnect();
    }
}