using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Import Photon's functionalities
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ServerManager : MonoBehaviourPunCallbacks
{
    #region Script Arguments
    public AnimationLaunch anim;
    public RawImage imgPing;
    public Texture[] pingTextures;
    #endregion

    private void Update()
    {

        int ping = PhotonNetwork.GetPing();
        
        if (ping < 20)
        {
            imgPing.texture = pingTextures[0];
        }
        else if (ping > 50)
        {
            imgPing.texture = pingTextures[2];
        }
        else
        {
            imgPing.texture = pingTextures[1];
        }
    }



    #region Connect To Server
    // As soon as created, try to connect to server
    public void Connect()
    {
        // Connect the client to the server
        PhotonNetwork.ConnectUsingSettings();

        // Display a loading animation
        /*TODO*/
    }

    public override void OnConnectedToMaster()
    {
        // Join the lobby as soon as the client is connected
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // Unshow the loading animation
        /*TODO*/

        // De-activate the server manager object (to allow for re-use)
        this.gameObject.SetActive(false);
    }
    #endregion

    #region Disconnect To Server
    public void Disconnect()
    {
        if (anim.GetComponent<AnimationLaunch>().transition.GetCurrentAnimatorStateInfo(0).IsName("MenuClick 1"))
        {
            // Disconnect the client to the server
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // De-activate the server manager object (to allow for re-use)
        this.gameObject.SetActive(false);
    }
    #endregion
}
