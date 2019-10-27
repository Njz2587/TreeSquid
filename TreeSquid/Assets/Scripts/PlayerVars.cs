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
    public float CheckPointResetDelay = 3.0f;
    public float GameOverResetDelay = 3.0f;

    #region Stored Variables
    public enum SceneState { PlayerNull, PlayerDisabled, PlayerActive }
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
    }

    /// <summary>
    /// Identify if the player has reached the max detection
    /// </summary>
    private void Update()
    {       
        if (player != null && sceneState == SceneState.PlayerActive)
        {
            //End the level if player reaches max detection
            if (currentDetectionAmount >= MAX_DETECTION)
            {
                currentDetectionAmount = MAX_DETECTION;
                GameOver();
            }
        }
    }

    /// <summary>
    /// Reset the player to their last checkpoint and increase their detection amount
    /// </summary>
    /// <param name="detectionAmount"></param>
    public void ResetToCheckPoint(int detectionAmount)
    {
        Debug.Log("RESTART AT LAST CHECKPOINT");
        DisablePlayer();
        currentDetectionAmount += detectionAmount;
        StartCoroutine(ResetPlayer(CheckPointResetDelay));
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
