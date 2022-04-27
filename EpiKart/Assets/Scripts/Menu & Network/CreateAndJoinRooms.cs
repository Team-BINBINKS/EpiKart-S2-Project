using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities
using Photon.Realtime; // Import Photon's server properties
using TMPro; // Allow to manipulate UI elements
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    #region Script Arguments
    public TMP_InputField joinInputField;
    public TMP_InputField usernameField;
    public TMP_Text warningInputField;

    public GameObject animCtrl; // To go the scene manager
    private NewSceneManager newSceneManager;
    #endregion

    #region Script Vars
    const string GLYPHS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    #endregion

    private void Start()
    {
        newSceneManager = animCtrl.GetComponent<NewSceneManager>();
    }

    public string GenerateRoomCode()
    {
        // Initiliaze the default code
        string code = "";

        // Build & return a five-letter alphanumeric word
        for (int i = 0; i < 5; i++) code += GLYPHS[Random.Range(0, GLYPHS.Length)];
        return code;
    }

    // Called when clicking the create room button
    public void CreateRoom()
    {
        if (CheckSetUsername(usernameField.text))
        {
            // Setup the server properties
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 8;

            // Store the current room code
            string roomCode = GenerateRoomCode();

            // Allow to create only if room name isn't empty
            if (roomCode != "") PhotonNetwork.CreateRoom(roomCode, roomOptions);
        }
    }

    // Called when join room button is clicked after entering room code
    public void JoinRoom()
    {
        // Compute the length
        int inputFieldLength = joinInputField.text.Length;

        if (CheckSetUsername(usernameField.text))
        {
            // Join a pre-existing room of name given by the 'Join Room' InputField
            if (string.IsNullOrEmpty(joinInputField.text)) warningInputField.text = "Empty room code entered!";
            if (inputFieldLength > 0 && inputFieldLength < 5) warningInputField.text = "Room code must be 5 characters long!";

            PhotonNetwork.JoinRoom(joinInputField.text);
        }

    }

    public bool CheckSetUsername(string username)
    {
        Regex reg = new Regex("^[a-zA-Z0-9]([._-](?![._-])|[a-zA-Z0-9]){3,18}[a-zA-Z0-9]$");
        if (reg.IsMatch(username))
        {
            PhotonNetwork.LocalPlayer.NickName = username;
            return true;
        }
        else
        {
            warningInputField.text = "Username not correct";
            return false;
        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        joinInputField = GameObject.Find("InputFieldMapCode").GetComponent<TMP_InputField>();
        warningInputField = GameObject.Find("txtWarningJoin").GetComponent<TMP_Text>();

        if (returnCode == 32764) warningInputField.text = "Room has been closed by host!";
        else warningInputField.text = "Room does not exist!";
    }

    // Called when local player's status is detected to have joined the room
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.IsOpen)
        {
            // Move to the given room when joining
            //PhotonNetwork.LoadLevel("scn_GameLobby");// old way
            newSceneManager.ShowLobby();
           
        }
        else
        {
            PhotonNetwork.Disconnect();
            //SceneManager.LoadScene("scn_MainLobby"); // old way
            newSceneManager.HideLobby();
        }

    }
}
