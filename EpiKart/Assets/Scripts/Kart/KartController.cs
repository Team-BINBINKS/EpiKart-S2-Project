using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon's functionalities
using TMPro;
using System.Linq;
using Cinemachine;
using System;
using Photon.Realtime;
using UnityEngine.UI;

public class KartController : MonoBehaviour
{
    #region Script Arguments
    [Header("Models & Rigibodies")]
    public Transform playerModel;
    public Rigidbody sphere;
    public Transform frontWheelL, frontWheelR;
    public float maxWheelTurn;

    [Header("Animations & Particles")]
    public ParticleSystem[] dustTrail;
    public float maxEmission = 10f;
    public float emissionRate;

    [Header("Ground topography adaptation")]
    public Transform groundRayPoint;
    public LayerMask whatIsGround;
    public float groundRayLength = .3f;

    [Header("Movement")]
    public float acceleration = 100f;
    public float steering = 25f;
    public float gravity = 12.5f;
    public float dragOnGround = 5f;

    //[Header("Drift")]
    //public bool driftL;
    //public bool driftR;
    //public float driftTime, driftDir;
    //public bool isSliding;
    //public float outwardsDriftForce;

    [Header("Bonus")]
    public float bonusSpeed = 1f;
    public string usingBonus;
    public string bonusInventory;

    [Header("Checkpoints")]
    public List<bool> visitedCP = new List<bool>();
    public int racePosition;
    public int kartIndex;
    public int lapCount;

    [Header("Camera")]
    [SerializeField] private Camera myCamera;
    #endregion

    #region Script Vars
    float speed, currentSpeed;
    int backwardControls = 1;
    bool grounded;
    float turnInput;
    CheckpointManager manager;

    DateTime startRaceTime;
    List<TimeSpan> lapsTimes = new List<TimeSpan>();
    //protected GameObject bonusLabelGroup;
    #endregion

    // Store the player view ID on the network
    PhotonView view;
    TMP_Text txtTime;
    GameObject grpResults;

    void Start()
    {

        // Access the Photon network for the player
        view = GetComponent<PhotonView>();

        // Find the Virtual Camera in the room & assign it to the player
        if (view.IsMine)
        {
            GameObject VC = GameObject.Find("CM vcam1");
            CinemachineVirtualCamera CameraVC = VC.GetComponent<CinemachineVirtualCamera>();
            CameraVC.Follow = playerModel;
            CameraVC.LookAt = playerModel;
        }

        // Find & store the gameobject dealing with in-game UI
        //bonusLabelGroup = GameObject.Find("grpBonusLabel");

        // Initialize the checkpoint manager & current kart
        manager = GameObject.Find("CheckpointManager").GetComponent<CheckpointManager>();
        visitedCP.AddRange(manager.checkpoints.Select(cp => false));
        txtTime = GameObject.Find("txtTime").GetComponent<TMP_Text>();
        grpResults = GameObject.Find("grpResults");


        startRaceTime = DateTime.Now;
    }


    void EndGame()
    {
        bonusSpeed = 0;
        txtTime.alpha = 0;
        bonusInventory = "";
        usingBonus = "";
        grpResults.GetComponentInChildren<TMP_Text>().text = FormatResults();
    }

    [PunRPC]
    IEnumerator FinDeGame()
    {
        
        yield return new WaitForSeconds(20);
        Application.Quit();
    }

    private string FormatTime(TimeSpan time)
    {
        string first = ((int)(time.TotalSeconds)).ToString();
        string second = (((int)(time.Milliseconds / 10)).ToString().Length == 1) ? ("0" + ((int)(time.Milliseconds / 10)).ToString()) : ((int)(time.Milliseconds / 10)).ToString();
        return first + ":" + second;
    }



    void Update()
    {
        TimeSpan deltaTime = DateTime.Now.Subtract(startRaceTime);
        txtTime.text = FormatTime(deltaTime);



        // Check if the Photon view is the player's -- NECESSARY
        if (view.IsMine)
        {
            // Make the player follow the collider
            transform.position = sphere.transform.position - new Vector3(0, .75f, 0);

            // Accelerate according to the inputs


            float input = Input.GetAxis("Vertical");

            

            // Don't always update the input when stopping
            if (input != 0)
            {
                // Go slower when going backwards
                backwardControls = input > 0 ? 1 : -1;
                float multiplier = backwardControls < 0 ? .7f : 1f;
                speed = input * multiplier * acceleration;
            }

            // Steer according to the inputs
            turnInput = Input.GetAxis("Horizontal");
            if (turnInput != 0)
            {
                // Calculate the steering direction
                float amount = Mathf.Abs(turnInput);

                // Only allow to steer if the car is on the ground
                Steer(amount);
            }

            // Apply & update current speed
            currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 20f); speed = 0f;
            //print(currentSpeed);


            // Activate the bonus in the inventory
            if (Input.GetKeyDown("x") || Input.GetKeyDown(KeyCode.JoystickButton0) && bonusSpeed != 0)
            {
                if (string.IsNullOrEmpty(usingBonus)) ActivateBonus();
                else print("You can not activate a bonus while one is in use");
            }

            // Animate front wheels
            float wheelDeltaL = turnInput < 0 ? 1.15f : 1f, wheelDeltaR = turnInput > 0 ? 1.15f : 1f;
            float wheelTurn = turnInput * maxWheelTurn;
            frontWheelL.localRotation = Quaternion.Euler(frontWheelL.localRotation.eulerAngles.x, wheelTurn * wheelDeltaL, frontWheelL.localRotation.eulerAngles.z);
            frontWheelR.localRotation = Quaternion.Euler(frontWheelR.localRotation.eulerAngles.x, wheelTurn * wheelDeltaR, frontWheelR.localRotation.eulerAngles.z);
        }
    }

    private void FixedUpdate()
    {
        // Check if the Photon view is the player's
        if (view.IsMine)
        {
            #region NORMAL GROUND ADAPTATION
            // Init current grounded status & cast a beam
            grounded = false;
            RaycastHit hit;

            // Update the grounded status of the player & rotate the player according to terrain
            if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
            {
                grounded = true;
                playerModel.up = Vector3.Lerp(playerModel.up, hit.normal, Time.deltaTime * 8f);
                playerModel.Rotate(0, transform.eulerAngles.y, 0);
            }
            #endregion

            // Stop emitting while not moving
            emissionRate = 0;

            #region MOVEMENT
            // Only allow to move if the player is on the ground
            if (grounded)
            {
                // Set the sphere's drag to its normal value
                sphere.drag = dragOnGround;

                // Move forward when accelerating (bonusSpeed for nitroboost)
                sphere.AddForce(-playerModel.transform.forward * currentSpeed * bonusSpeed, ForceMode.Acceleration);

                // Apply gravity on the player
                sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

                // Start emitting dust particles while moving
                if (currentSpeed > 5 && bonusSpeed != 0) emissionRate = maxEmission;
                    
                #region DRIFT
                //// Activate the drifting ability
                //if (Input.GetKeyDown("space"))
                //{
                //    // We started drifting
                //    isSliding = true;
                //    print(driftDir);

                //    // Get which direction we're drifting towards
                //    if (driftDir > 0)
                //    {
                //        driftR = true;
                //        driftL = false;
                //    } else if (driftDir < 0)
                //    {
                //        driftR = false;
                //        driftL = true;
                //    }
                //}

                //// Store the amount of time spent drifting
                //if (Input.GetKey("space") && currentSpeed > 40 && turnInput != 0) driftTime += Time.deltaTime;
                ///* TODO ---- Deal with dirfting particles here */

                //// Stop drifting
                //if (!Input.GetKey("space") || currentSpeed < 40)
                //{
                //    // Reset booleans related to drifting
                //    driftR = false;
                //    driftL = false;
                //    isSliding = false;
                //    driftTime = 0;
                //}
                #endregion
            }
            else
            {
                // Reduce the sphere's drag to extremely low (increases fall speed)
                sphere.drag = .1f;

                // Apply extra gravity to make the car fall back down on the ground
                sphere.AddForce(Vector3.up * -gravity * 150f);
            }
            #endregion

            // Emit dust particles over time
            foreach (ParticleSystem part in dustTrail)
            {
                var emissionModule = part.emission;
                emissionModule.rateOverTime = emissionRate;
                part.startLifetime = .7f;
            }

        }
    }

    // Allow to turn horizontally
    public void Steer(float amount)
    {
        //driftDir = turnInput;

        //// Detect drifting
        //if (driftL && !driftR)
        //{
        //    // Drift left
        //    driftDir = turnInput < 0 ? -1.5f : -.5f;
        //    playerModel.localRotation = Quaternion.Lerp(playerModel.localRotation, Quaternion.Euler(0, -20f, 0), Time.deltaTime * 8f);
        //    if (isSliding && grounded) sphere.AddForce(transform.right * outwardsDriftForce * Time.deltaTime, ForceMode.Acceleration);
        //} else if (!driftL && driftR)
        //{
        //    // Drift right
        //    driftDir = turnInput > 0 ? 1.5f : .5f;
        //    playerModel.localRotation = Quaternion.Lerp(playerModel.localRotation, Quaternion.Euler(0, 20f, 0), Time.deltaTime * 8f);
        //    if (isSliding && grounded) sphere.AddForce(transform.right * -outwardsDriftForce * Time.deltaTime, ForceMode.Acceleration);
        //}
        //else
        //{
        //    playerModel.localRotation = Quaternion.Lerp(playerModel.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 8f);
        //} // + driftDir

        // Only allow the steering when moving
        if (Mathf.Abs(currentSpeed) > 5)
        {
            // Allow the player to rotate
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, steering * amount * turnInput * backwardControls * bonusSpeed * Time.deltaTime, 0f)); 
        }
    }

    // Deal with everything related to checkpoints & ranking system
    #region CHECKPOINTS & RANKS
    // Update the current visited checkpoint
    public bool UpdateCP(int index)
    {
        // Set the return value
        bool ret = false;
        if ((index == 0 || visitedCP[index - 1]) && !visitedCP[index]) ret = true;

        // Only update if no cheating has been found
        if (index == 0 || visitedCP[index - 1]) visitedCP[index] = true;

        // Returns true if update was successful (counters cheating & checkpoint skipping)
        return ret;
    }


    public string FormatResults()
    {
        string res = "Lap time : \n";
        foreach (TimeSpan el in lapsTimes)
        {
            res += ((int)(el.TotalSeconds)).ToString() + ":" + ((int)(el.Milliseconds / 10)).ToString() + "\n";
        }
        return res;
    }


    // Check if the kart just wen through the last checkpoint => new lap starting
    public bool CheckLap()
    {
        print("check lap");
        int i = 0, visitedLen = visitedCP.Count;
        while (i < visitedLen && visitedCP[i]) i++;

        // Check if we haven't cheated
        if (i == visitedLen)
        {
            // Restart all values of visited since we start a new lap
            print("LAP!!!!!!!!!!");
            for (int j = 0; j < visitedCP.Count; j++) visitedCP[j] = false;
            print("lapcount :" + lapsTimes);
            lapsTimes.Add(DateTime.Now.Subtract(startRaceTime));

            if (lapCount == 1)
            {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("FinDeGame", RpcTarget.All);
                EndGame();
            }

            lapCount++;
            return true;
        }
        return false;
    }

    // Respawn at the last visited checkpoint
    public (Vector3, Quaternion) GetRespawnPoint()
    {
        (Vector3, Quaternion) res = (manager.checkpoints[^1].transform.position, manager.checkpoints[^1].transform.rotation);
        for (int i = 0; i < visitedCP.Count; i++)
        {
            if (visitedCP[i])
            {
                res.Item1 = manager.checkpoints[i].transform.position;
                res.Item2 = manager.checkpoints[i].transform.rotation;
            }
        }

        return res;
    }
    #endregion

    // Deal with everything related to bonuses
    #region BONUSES
    void ActivateBonus()
    {
        if (grounded)
        {
            // Set the currently used bonus & reset inventory
            usingBonus = bonusInventory;
            bonusInventory = "";

            // Switch through all possible bonus cases
            switch (usingBonus)
            {
                case "NitroBoost":
                    StartCoroutine(NitroBoost());
                    break;
                case "OilPuddle":
                    StartCoroutine(OilPuddle());
                    break;
                case "GodMode":
                    StartCoroutine(GodMode());
                    break;
                default:
                    Debug.Log("KartController.ActivateBonus(): No bonus in inventory!");
                    break;
            }
            sphere.GetComponent<KartCollider>().UpdateBonusUI();
        }
    }

    IEnumerator NitroBoost()
    {
        // Increase the player's speed
        bonusSpeed = 1.5f ;

        // Wait for a few seconds and restore normal speed & inventory
        yield return new WaitForSeconds(2);
        bonusSpeed = 1;

        // Reset the bonus in use
        usingBonus = "";
    }

    IEnumerator OilPuddle()
    {
        if (grounded)
        {
            Vector3 pos = this.transform.position;
            GameObject normal = this.gameObject.transform.GetChild(2).gameObject;
            GameObject parent = normal.gameObject.transform.GetChild(0).gameObject;
            GameObject redKart = parent.gameObject.transform.GetChild(0).gameObject;

            // Wait a few seconds and spawn the oil puddle behind the player
            yield return new WaitForSeconds(0.5f);
            PhotonNetwork.Instantiate("OilPuddle", pos, redKart.transform.rotation);
        }
        else
        {
            // Set back the bonus inventory
            bonusInventory = "OilPuddle";
        }

        // Reset the bonus in use in all cases
        usingBonus = "";
    }

    IEnumerator GodMode()
    {
        // Increase the player's speed
        bonusSpeed = 1.15f;

        // Wait for a few seconds and restore normal speed & inventory
        yield return new WaitForSeconds(7.5f);
        bonusSpeed = 1;

        // Reset the bonus in use
        usingBonus = "";
    }
    #endregion
}