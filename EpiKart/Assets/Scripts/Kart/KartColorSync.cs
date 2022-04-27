using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities
using Photon.Realtime; // Import Photon's server properties
using TMPro; // Allow to manipulate UI elements
using UnityEngine.SceneManagement;
using System.Linq;


public class KartColorSync : MonoBehaviour, IPunObservable
{
    public MeshRenderer kartMesh;
    static public Dictionary<string, Color> colorList = new Dictionary<string, Color>(){
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
    static Color GetColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    static string GetColorNameFromColor(Color color)
    {
        foreach (KeyValuePair<string, Color> kvp in colorList)
        {
            if (kvp.Value == color)
            {
                return kvp.Key;
            }
        }
        return "Something wrong happened...";
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetColorNameFromColor(kartMesh.materials[1].color));
        }
        else if (stream.IsReading)
        {
            string colorIndex = (string)stream.ReceiveNext();

            if (colorIndex == null) Debug.LogWarning("Something wrong happened...");

            kartMesh.materials[1].color = colorList[colorIndex];
        }
    }

}
