using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun; // Import Photon's functionalities
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    #region Script Arguments
    public GameObject playerPrefab;
    public MeshRenderer kartMesh;
    public Color kartColor;
    public Color newcolor;
    #endregion

    #region Script Vars
    List<Vector3> kartSlots = new List<Vector3>() {
        {new Vector3(268.5f, 0.25f, 194.1f)},
        {new Vector3(273.7f, 0.25f, 190.6f)},
        {new Vector3(278.4f, 0.25f, 194.1f)},
        {new Vector3(283.5f, 0.25f, 190.6f)},
        {new Vector3(288.2f, 0.25f, 194.1f)},
        {new Vector3(293.2f, 0.25f, 190.6f)},
        {new Vector3(198.2f, 0.25f, 194.1f)},
        {new Vector3(303.2f, 0.25f, 190.6f)},
    };
    public Dictionary<string, Color> colorList = new Dictionary<string, Color>(){
        {"Blue",    GetColor(59f,130f,246f)  },
        {"Cyan",    GetColor(34f,212f,238f)  },
        {"Pink",    GetColor(249f,168f,212f) },
        {"White",   GetColor(255f,255f,255f) },
        {"Black",   GetColor(0f,0f,0f)       },
        {"Orange",  GetColor(251f,146f,60f)  },
        {"Yellow",  GetColor(253f,230f,138f) },
        {"Green",   GetColor(74f,222f,128f)  }
    };
    #endregion

    static Color GetColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    private void Start()
    {
        Vector3 spawnPos = kartSlots[PhotonNetwork.LocalPlayer.ActorNumber-1];
        Vector3 spawnOrientation = new Vector3(0, 90, 0);
        GameObject instantiatedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.Euler(spawnOrientation));
        MeshRenderer playerMesh = instantiatedPlayer.GetComponentInChildren<MeshRenderer>();
        playerMesh.materials[1].color = kartColor;

        // Set the instantiated player's race rank --- TO REMOVE?
        instantiatedPlayer.GetComponentInChildren<KartController>().racePosition = PhotonNetwork.LocalPlayer.ActorNumber;




        //TODO REWORK
        // Instantiate a player at a random position on the terrain

        // MeshRenderer playerMesh = instantiedPlayer.GetComponentInChildren<MeshRenderer>();
        // int randomColor = Random.Range(0, 8);
        // Color dotColor = colorList.Values.ToList()[randomColor];
        // playerMesh.materials[1].color = dotColor;
        // SetKartColor(Color.blue);
    }
}