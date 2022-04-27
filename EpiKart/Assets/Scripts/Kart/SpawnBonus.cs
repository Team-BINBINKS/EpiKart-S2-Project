using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities

public class SpawnBonus : MonoBehaviour
{
    // Declare an enum of all in-game bonuses
    public enum BonusType
    {
        OilPuddle,
        NitroBoost,
        GodMode
    }

    #region Script Arguments
    [Header("Models")]
    public GameObject BonusCube;
    #endregion

    [Header("Bonus Coordinates")]
    private Vector3 BosPos1 = new Vector3(206f, 1f, 192.5f);
    private Vector3 BosPos2 = new Vector3(206f, 1f, 190.5f);
    private Vector3 BosPos3 = new Vector3(206f, 1f, 194.5f);
    private Vector3 BosPos4 = new Vector3(271f, 1f, 260f);
    private Vector3 BosPos5 = new Vector3(271f, 1f, 263f);
    private Vector3 BosPos6 = new Vector3(274.5f, 1f, 261.5f);
    private Vector3 BosPos7 = new Vector3(274.5f, 1f, 264.5f);
    private Vector3[] BonusArray;


    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            BonusArray = new Vector3[] { BosPos1, BosPos2, BosPos3, BosPos4, BosPos5, BosPos6, BosPos7 };

            foreach (var e in BonusArray)
            {
                PhotonNetwork.Instantiate(BonusCube.name, e, Quaternion.Euler(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180))); // create a bonus cube
            }
        }

    }

    // Respawns bonuses
    public IEnumerator respawnBonus(Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Respawn a bonus cube at given position & random rotation
            yield return new WaitForSeconds(2); // wait for a bit before respawning
            PhotonNetwork.Instantiate(BonusCube.name, position, Quaternion.Euler(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));
        }
    }
}
