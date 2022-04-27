using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities
using TMPro; // Allow to manipulate UI elements
using UnityEngine.UI; // Use the images
using Photon.Realtime; // Import Photon's server properties
using System.Linq;
using UnityEngine.SceneManagement;

public class RetrieveRoomName : MonoBehaviour//, IPunObservable
{

    #region Script Arguments
    public GameObject launchButton;
    public TMP_Text codeTextField;
    public TMP_Text[] playerTexts = new TMP_Text[8]; // Array of all 8 slots for the players to join
    public RawImage[] dots = new RawImage[8]; // Array of all 8 slots for the players to join
    public GameObject[] selectorDots = new GameObject[12];
    public Color[] playersColors = new Color[8]; // Array of all 8 slots for the players to join
    public GameObject grpColorSelector;
    public GameObject animCtrl;
    #endregion

    #region Color Dictionary
    static Color GetColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public PhotonView photonView;

    public Dictionary<string, Color> colorList = new Dictionary<string, Color>(){
        {"Red",     GetColor(247f, 5f, 33f)  },
        {"Orange",  GetColor(251f,146f,60f)  },
        {"Yellow",  GetColor(253f,230f,138f) },
        {"Green",   GetColor(74f,222f,128f)  },
        {"Cyan",    GetColor(34f,212f,238f)  },
        {"Blue",    GetColor(59f,130f,246f)  },
        {"Purple",  GetColor(110f, 0f, 219f) },
        {"Pink",    GetColor(249f,168f,212f) },
        {"White",   GetColor(255f,255f,255f) },
        {"Grey",    GetColor(71f, 71f, 71f)  },
        {"Black",   GetColor(0f,0f,0f)       },
        {"Brown",   GetColor(112f, 41f, 12f) }
    };
    #endregion

    // Change text stats when entering the room
    void Start()
    {
        // Sync all clients to the master
        PhotonNetwork.AutomaticallySyncScene = true;

        // Set the code room text to the server name
        codeTextField.text = PhotonNetwork.CurrentRoom.Name;

        // Update every player slots when joining
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            // Update all names accross network
            playerTexts[i].text = PhotonNetwork.PlayerList[i].NickName;

        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (PhotonNetwork.IsMasterClient)
        {
            launchButton.SetActive(true);
        }
        GettingColorCustomProp();
        AttributingNewColor();

    }
    public void InitializeSelectorDots()
    {
        List<string> alreadySelectedColors = new List<string>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            alreadySelectedColors.Add((string)player.CustomProperties["Color"]);
        }

        for (int i = 0; i < selectorDots.Length; i++)
        {
            (string key, Color color) = colorList.ElementAt(i);
            selectorDots[i].GetComponent<Image>().color = color;
            selectorDots[i].GetComponent<Button>().interactable = !alreadySelectedColors.Contains(key);
        }
    }

    public void ClickOnDot(int playernb)
    {
        if (playernb == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            InitializeSelectorDots();
            grpColorSelector.SetActive(true);
            Animation anim = grpColorSelector.GetComponent<Animation>();
            anim.Play();
            
        }
        else
        {
            Debug.Log("You just click on someone else dot");
        }
    }

    public void AttributingNewColor()
    {
        List<string> allColors = colorList.Keys.ToList();

        List<string> alreadySelectedColors = new List<string>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            alreadySelectedColors.Add((string)player.CustomProperties["Color"]);
        }

        List<string> diff = allColors.Except(alreadySelectedColors).ToList();
        int randomColor = Random.Range(0, diff.Count);
        string colorName = diff[randomColor];
        Color color = colorList[colorName];
        dots[PhotonNetwork.LocalPlayer.ActorNumber - 1].color = color;
        _myCustomProperties["Color"] = colorName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_myCustomProperties);
        PhotonNetwork.RaiseEvent(0, "", RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }

    public void SelectColor(string colorName)
    {
        Color color = colorList[colorName];
        dots[PhotonNetwork.LocalPlayer.ActorNumber - 1].color = color;
        _myCustomProperties["Color"] = colorName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_myCustomProperties);
        PhotonNetwork.RaiseEvent(0, "", RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }

    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    public void UpdatingColorCustomProp()
    {
        Debug.Log("Updating Custom Prop");
        int randomColor = Random.Range(0, colorList.Count);
        (string key, Color color) = colorList.ElementAt(randomColor);
        dots[PhotonNetwork.LocalPlayer.ActorNumber-1].color = color;
        _myCustomProperties["Color"] = key;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_myCustomProperties);
        PhotonNetwork.RaiseEvent(0, "", RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }

    public void GettingColorCustomProp()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("Color"))
            {

                dots[i].color = colorList[(string)PhotonNetwork.PlayerList[i].CustomProperties["Color"]];
            }
            else{
                if (PhotonNetwork.LocalPlayer.ActorNumber-1 != i) {
                    Debug.Log("No color;");
                }
            }
        };  
    }


    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
        if (obj.Code == 0)
        {
            GettingColorCustomProp();
        }
    }

    void Update()
    {
        // Update every player slots when joining
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            // Update all names accross network
            playerTexts[i].text = PhotonNetwork.PlayerList[i].NickName;
        }
    }

    public void SpecialLaunchGame(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            animCtrl.SetActive(false);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            int playerID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Color playerColor = playersColors[playerID];
            GameObject spawnPlayers = GameObject.Find("SpawnPlayers");
            SpawnPlayers spawnPlayersCpt = spawnPlayers.GetComponent<SpawnPlayers>();
            spawnPlayersCpt.kartColor = colorList[(string)PhotonNetwork.LocalPlayer.CustomProperties["Color"]];
        }
    }
}