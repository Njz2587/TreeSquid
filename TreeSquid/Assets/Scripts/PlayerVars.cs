using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVars : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Player Varables")]
    public SceneState sceneState = SceneState.PlayerNull;
    public float MAX_DETECTION = 100;
    public const float CheckPointResetDelay = 3.0f;
    public float GameOverResetDelay = 3.0f;

    [Header("Scene Music")]
    [Space(10)]
    public AudioClip sceneMusic;
    public AudioSource audioSource;

    #region Stored Variables
    public enum SceneState { PlayerNull, PlayerDisabled, PlayerActive, PlayerPaused }
    [HideInInspector]
    public float currentDetectionAmount; //Current amount of detection the player has gained
    [HideInInspector]
    public CheckPoint mostRecentCheckPoint; //Most recent checkpoint that the player has reached
    [HideInInspector]
    public List<int> reachedCheckPoints; //All previous checkpoints that the player has reached
    [HideInInspector]
    public GameObject player; //The player defined by the start method of the player
    [HideInInspector]
    public static PlayerVars instance; //Singleton

    private bool isReseting = false;
    #endregion
    #endregion

    /// <summary>
    /// Define Singleton
    /// </summary>
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Assign a checkpoint ID to all level checkpoints
    /// </summary>
    private void Start()
    {
        reachedCheckPoints = new List<int>();
        //Give Each Checkpoint an ID
        CheckPoint[] checkPoints = GameObject.FindObjectsOfType<CheckPoint>();
        for(int i = 0; i < checkPoints.Length; i++)
        {
            checkPoints[i].CheckPointID = i;
        }

        if(sceneMusic & audioSource)
        {
            audioSource.clip = sceneMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Identify if the player has reached the max detection
    /// </summary>
    private void Update()
    {
        if (player != null)
        {
            if (sceneState == SceneState.PlayerActive)
            {
                //End the level if player reaches max detection
                if (currentDetectionAmount >= MAX_DETECTION)
                {
                    currentDetectionAmount = MAX_DETECTION;
                    GameOver();
                }
            }
        }

        #region Commands
        if (Input.GetKey(KeyCode.Slash))
        {
            string comboKeyCode = Input.inputString;
            if (comboKeyCode == "r")
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (comboKeyCode == "c")
            {
                PlayerVars.instance.ResetToCheckPoint(0);
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(sceneState != SceneState.PlayerPaused)
            {
                sceneState = SceneState.PlayerPaused;
                Time.timeScale = 0;
            }
            else
            {
                sceneState = SceneState.PlayerActive;
                Time.timeScale = 1;
            }
        }
        #endregion
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == 11) //Squid Layer
        {
            ResetToCheckPoint(0, 0);
        }
    }

    /// <summary>
    /// Reset the player to their last checkpoint and increase their detection amount
    /// </summary>
    /// <param name="detectionAmount"></param>
    public void ResetToCheckPoint(int detectionAmount, float resetDelay = CheckPointResetDelay)
    {
        if (isReseting == false)
        {
            isReseting = true;
            //Debug.Log("RESTART AT LAST CHECKPOINT");
            DisablePlayer();
            currentDetectionAmount += detectionAmount;
            StartCoroutine(ResetPlayer(resetDelay));
        }
    }

    #region Helper Methods
    /// <summary>
    /// Disables the player's controls and makes them inactive and invisible
    /// </summary>
    public void DisablePlayer()
    {
        sceneState = SceneState.PlayerDisabled;
        player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<PlayerController>().ResetPlayerCharge();
        ResetAllDetectionZones();
    }

    /// <summary>
    /// Enables the player's controls and makes them active and visible
    /// </summary>
    public void EnablePlayer()
    {
        sceneState = SceneState.PlayerActive;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }

    /// <summary>
    /// Disables the player and starts restarting the scene with a delay
    /// </summary>
    private void GameOver()
    {
        //Restart Scene
        Debug.Log("GAME OVER");
        DisablePlayer();
        StartCoroutine(RestartScene(GameOverResetDelay));
    }

    /// <summary>
    /// Resets all detection zones so that the player is not falsely recognized
    /// </summary>
    private void ResetAllDetectionZones()
    {
        foreach(DetectionZone zone in GameObject.FindObjectsOfType<DetectionZone>())
        {
            if(zone.currentlyInContact)
            {
                zone.currentlyInContact = false;
                zone.updating = false;
            }
        }
    }

    /// <summary>
    /// Resets the player's position to their last checkpoint and reenables them
    /// </summary>
    /// <param name="resetDelay"></param>
    /// <returns></returns>
    private IEnumerator ResetPlayer(float resetDelay)
    {
        yield return new WaitForSeconds(resetDelay);
        if (mostRecentCheckPoint != null)
        {
            mostRecentCheckPoint.ResetObjects();
            player.transform.position = mostRecentCheckPoint.drawObjects[1].drawTransform.position;
            player.transform.forward = mostRecentCheckPoint.drawObjects[1].drawTransform.forward;
        }
        ResetAllDetectionZones();
        EnablePlayer();

        isReseting = false;
    }

    /// <summary>
    /// Restarts the current scene
    /// </summary>
    /// <param name="restartDelay"></param>
    /// <returns></returns>
    private IEnumerator RestartScene(float restartDelay)
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
