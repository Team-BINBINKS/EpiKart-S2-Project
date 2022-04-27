using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.UI;

public class KartCollider : MonoBehaviour
{
    #region Script Arguments
    public GameObject kart;
    public Texture[] bonusImages = new Texture[3];
    #endregion

    #region Script vars
    protected SpawnBonus spawnBonus;
    protected TMP_Text bonusLabel;
    protected GameObject bonusLabelGroup;
    protected CheckpointManager checkpointManager;
    protected KartController kartController;

    protected TMP_Text lapCounter;
    protected TMP_Text rankUI;
    protected TMP_Text txtWarning;
    protected GameObject grpResults;
    protected RawImage imgBonus;

    private int playerPersoRank;
    #endregion

    private void Start()
    {

        for (int i = 0; i < 22; i++) checkpointCounters[i] = 0;
        lapCounter = GameObject.Find("txtLap").GetComponent<TMP_Text>();
        rankUI = GameObject.Find("txtRank").GetComponent<TMP_Text>();
        txtWarning = GameObject.Find("txtWarning").GetComponent<TMP_Text>();
        grpResults = GameObject.Find("grpResults");
        imgBonus = GameObject.Find("imgBonus").GetComponent<RawImage>();

        // Get the component of the spawning bonus game object
        spawnBonus = GameObject.Find("SpawnBonus").GetComponent<SpawnBonus>();

        // In-game UI
        //bonusLabelGroup = GameObject.Find("grpBonusLabel");
        //bonusLabel = GameObject.Find("txtBonusLabel").GetComponent<TMP_Text>();
        //bonusLabelGroup.SetActive(false);

        // Initialize the checkpoint manager & current kart
        checkpointManager = GameObject.Find("CheckpointManager").GetComponent<CheckpointManager>();
        kartController = kart.GetComponent<KartController>();
        SetPlayerPos(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        playerPersoRank = PhotonNetwork.LocalPlayer.ActorNumber;
        rankUI.text = playerPersoRank.ToString();

    }
        

    // Updates when an objects collides with the Bonus 
    public IEnumerator OnTriggerEnter(Collider col) // col is the GameObect wich colides with the bonus
    {
        switch (col.gameObject.layer)
        {
            case 6: // Collide with another kart
                if (col.gameObject.transform.parent.transform.GetChild(0).GetComponent<KartController>().bonusInventory == "MonsterTruck") CrashCollision();
                break;

            case 7: // Collide with a bonus
                RandomBonus();
                PhotonNetwork.Destroy(col.gameObject);
                StartCoroutine(spawnBonus.respawnBonus(col.transform.position));
                break;

            case 8: // Collide with an oil puddle
                if (kartController.usingBonus != "GodMode")
                {
                    StartCoroutine(CrashCollision());
                    PhotonNetwork.Destroy(col.gameObject.transform.parent.gameObject);
                }
                break;

            case 9: // Collide with the kill zone
                StartCoroutine(RespawnCheckpoint());
                break;

            case 10: // Collide with a checkpoint

                //bonusLabelGroup.SetActive(true);
                //bonusLabel.text = col.ToString();

                // Loop through all checkpoints of the track
                int i = 0;
                while (checkpointManager.checkpoints[i] != col.gameObject && i < checkpointManager.checkpoints.Count )
                {
                    i++;
                }

                if (i == checkpointManager.checkpoints.Count)
                {
                    print("checkpoints not find");
                }
                else
                {
                    
                    print("============== COLLISION OF " + kart.GetPhotonView().Owner.NickName + "==============");

                    if (kart.GetPhotonView().Owner != PhotonNetwork.LocalPlayer)
                    {
                        print("difference between player doing nothing  just updating the rank ?");
                        yield return new WaitForSeconds(1);
                        rankUI.text = ReadPlayerPos(PhotonNetwork.LocalPlayer).ToString() ;
                        break;
                    }

                    print("checkpoint find");
                    print("index checkpoint : " + i);

                    // Update the list of checkpoints visited by the kart
                    if (kartController.UpdateCP(i))
                    {
                        // Increment the encountered checkpoint value
                        //checkpointCounters[i]++;
                        SendAugmentation(i);
                        //rankManager.checkpointCounters[i]++;


                        print("currRank: " + kartController.racePosition);
                        print("newRank: " + (checkpointCounters[i]+1));

                        // Swap both ranks
                        int swapWith = (checkpointCounters[i]+1) % PhotonNetwork.CurrentRoom.PlayerCount == 0 ? PhotonNetwork.CurrentRoom.PlayerCount : (checkpointCounters[i]+1) % PhotonNetwork.CurrentRoom.PlayerCount;
                        print("swapping with the guy at index : " + swapWith);
                        playerPersoRank = swapWith;
                        rankUI.text = playerPersoRank.ToString();

                        SwapRanks(PhotonNetwork.LocalPlayer, swapWith);
                        //SwapRanks(kartController.racePosition, rankManager.checkpointCounters[i]);
                    }
                    else
                    {
                        if (!kartController.visitedCP[i])
                        {
                            if (int.Parse(lapCounter.text) != 1)
                            {
                                txtWarning.text = "You have not respected checkpoints order." + " At cp : " + i;
                                yield return new WaitForSeconds(2);
                                txtWarning.text = "";

                            }
                        }
                    }

                    // Detect a new lap
                    if (i == checkpointManager.checkpoints.Count - 1) {
                        if (kartController.CheckLap())
                        {
                            print("adding 1 to the UI counter of lap");
                            lapCounter.text = (int.Parse(lapCounter.text)+1).ToString();

                        }
                    }


                    print(PhotonNetwork.LocalPlayer.NickName + " is at pos " +  + playerPersoRank + " but on photon is " + (int)PhotonNetwork.LocalPlayer.CustomProperties["Rank"]);
                    //PrintListCheckpoints();
                };
                break;

            default:
                Debug.Log("Something wrong happened...");
                break;
        }
    }

    #region TitouanPartSeeLater
    private ExitGames.Client.Photon.Hashtable playerCustomProp = new ExitGames.Client.Photon.Hashtable();

    private void SetPlayerPos(Player player, int pos)
    {
        playerCustomProp["Rank"] = pos;
        player.SetCustomProperties(playerCustomProp);
    }
    private int ReadPlayerPos(Player player)
    {
        if (!player.CustomProperties.ContainsKey("Rank"))
        {
            throw new System.Exception("key not found wtf");
        }
        return (int)player.CustomProperties["Rank"];
    }

    private void SwapPlayerRank(Player p1, Player p2)
    {
        print("swapping between " + p1.NickName + " and " + p2.NickName);
        print("of values " + ReadPlayerPos(p1) + " " + ReadPlayerPos(p2));

        int tmp = ReadPlayerPos(p1);
        SetPlayerPos(p1, ReadPlayerPos(p2));
        SetPlayerPos(p2, tmp);

        print("in sortie of swap");
        print("of values " + ReadPlayerPos(p1) + " " + ReadPlayerPos(p2));
    }

    private void SwapRanks(Player p1, int rank)
    {
        Player p2 = null;
        int i = 0;
        while ((p2 is null) && i < PhotonNetwork.PlayerList.Length)
        {
            print("Player " + PhotonNetwork.PlayerList[i].NickName + "is at place " + ReadPlayerPos(PhotonNetwork.PlayerList[i]));
            if (ReadPlayerPos(PhotonNetwork.PlayerList[i]) == rank) p2 = PhotonNetwork.PlayerList[i];
            i++;
        }
        SwapPlayerRank(p1, p2);

    }

    public void SendAugmentation(int i)
    {
        //print("we send to everyone " + i + "is augmented");
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well

        PhotonNetwork.RaiseEvent(1, i, raiseEventOptions, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }

    

        private void OnEnable()
    {

        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private int[] checkpointCounters = new int[22]; // TODO Update with the number of CP

    private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {

        if (obj.Code == 1)
        {
            //print("we received the order of augmenting at index " + obj.CustomData);
            checkpointCounters[(int)obj.CustomData]++;


            //print("event received");
            //print("before");
            //PrintListCheckpoints();

            //int temp = checkpointCounters[0];

            //int[] receivedData = (int[])obj.CustomData;
            //print("len of what is received " + receivedData.Length);
            //checkpointCounters = receivedData;

            //print("original is " + temp);
            //print("after is " + checkpointCounters[0]);
            //print("diff between them " + (temp - checkpointCounters[0]));



            //print("after");
            //PrintListCheckpoints();

        }
    }

    //private void Update()
    //{
    //    string listCheckPoints = "";
    //    foreach (int i in checkpointCounters) listCheckPoints = listCheckPoints + i + " ";
    //    print("Actual content of lCP is : " + listCheckPoints);

    //}

    private void PrintListCheckpoints()
    {
        //string listCheckPoints = "";
        //foreach (int i in checkpointCounters) listCheckPoints = listCheckPoints +  i + " ";
        //print(listCheckPoints);
    }
    #endregion

    public int propRandom(int rank, int size)
    {
        float x = ((int)Random.Range(0, 100) / 100);

        // Find the kart in the hierarchy
        if (rank <= PhotonNetwork.CurrentRoom.PlayerCount / 4) return (int)Mathf.Pow(size, x);
        if (PhotonNetwork.CurrentRoom.PlayerCount * 3 / 4 <= rank) return -size * (int)Mathf.Pow(x, 2) + size;
        else return (int)x * size;
    }

    // Randomly pick a bonus and broadcast it
    public void RandomBonus()
    {
        // Find the kart in the hierarchy
        int size = SpawnBonus.BonusType.GetNames(typeof(SpawnBonus.BonusType)).Length;
        int bonusRank = propRandom((int)PhotonNetwork.LocalPlayer.CustomProperties["Rank"], size) ;
        string bonusName = ((SpawnBonus.BonusType)Random.Range(0, size)).ToString();

        // Add the bonus to the kart's inventory if empty, and change the label on screen
        if (string.IsNullOrEmpty(kart.GetComponent<KartController>().bonusInventory))
        {
            kart.GetComponent<KartController>().bonusInventory = bonusName;
            //bonusLabelGroup.SetActive(true);
            //bonusLabel.text = kart.GetComponent<KartController>().racePosition.ToString();

            UpdateBonusUI(bonusName);
        }
    }

    public void UpdateBonusUI(string bonusName = "")
    {
        imgBonus.color = new Color(0, 0, 0, 1);
        switch (bonusName)
        {
            case "OilPuddle": imgBonus.texture = bonusImages[0];  break;
            case "GodMode": imgBonus.texture = bonusImages[1]; break;
            case "NitroBoost": imgBonus.texture = bonusImages[2]; break;
            default: imgBonus.texture = null; imgBonus.color = new Color(0, 0, 0, 0); break;
        }

    }

    // Detects the collision
    IEnumerator CrashCollision()
    {
        // Wait a few seconds before stopping the kart
        yield return new WaitForSeconds(.25f);
        kart.GetComponent<KartController>().bonusSpeed = 0;

        // Wait some more time & restore parameters
        yield return new WaitForSeconds(2);
        kart.GetComponent<KartController>().bonusSpeed = 1;
    }
    
    // Respawn at given coord and dir
    IEnumerator RespawnCheckpoint()
    {
        // Stop the kart
        kartController.bonusSpeed = 0;

        // Wait & restore parameters
        yield return new WaitForSeconds(.5f);
        kartController.bonusSpeed = 1;

        // Set the new position & rotation of the kart
        (transform.position, kart.transform.rotation) = kartController.GetRespawnPoint();
    }
}