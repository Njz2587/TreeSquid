using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Detection Zone Settings")]
    public DetectionMode detectionMode = DetectionMode.Incremental; //Mode of how this detection zone will behave
    [ConditionalHide("isIncremental", true)]
    public int detectionIncrement; //How much detection is gained every second the player is in contact with this region
    [ConditionalHide("isIncremental", true)]
    public float detectionIncrementRate = 1.0f; //How often the detection rate should increment in seconds
    [ConditionalHide("isIncremental", true, true)]
    public int instantDetectionIncrement = 10; //How much detection is gained instantly when the player is in contact with this region

    [Header("Audio Settings")]
    [Space(10)]
    public AudioSource audioSource;
    public AudioClip instantDeathSound;

    [Header("Gizmo Control")]
    [Space(10)]
    public GizmoDrawObject drawObject;

    #region Stored Values
    public enum DetectionMode { Incremental, Instant }
    [HideInInspector]
    public bool isIncremental;
    [HideInInspector]
    public bool updating;
    [HideInInspector]
    public bool currentlyInContact; //True if the player is currently within contact with this region
    #endregion
    #endregion

    /// <summary>
    /// Either increment the detection value or reset to checkpoint if squid is within
    /// </summary>
    void Update()
    {
        if(currentlyInContact)
        {
            if (PlayerVars.instance.sceneState == PlayerVars.SceneState.PlayerActive)
            {
                if (!updating)
                {
                    updating = true;
                    if (detectionMode == DetectionMode.Incremental)
                    {
                        StartCoroutine(IncrementDecection(detectionIncrementRate));
                    }
                    else if (detectionMode == DetectionMode.Instant)
                    {
                        if(audioSource && instantDeathSound)
                        {
                            audioSource.PlayOneShot(instantDeathSound);
                        }
                        PlayerVars.instance.ResetToCheckPoint(instantDetectionIncrement);
                    }
                }
            }
            else
            {
                currentlyInContact = false;
            }
        }
    }

    /// <summary>
    /// Call the OnValidate method of the detection zone gizmo and Update inspector tools based on mode
    /// </summary>
    private void OnValidate()
    {
        if (drawObject != null)
        {
            drawObject.OnValidate();
        }

        if(detectionMode == DetectionMode.Instant)
        {
            if(isIncremental)
            {
                isIncremental = false;
            }
        }
        else if (detectionMode == DetectionMode.Incremental)
        {
            if (!isIncremental)
            {
                isIncremental = true;
            }
        }
    }

    /// <summary>
    /// Call the OnDrawGizmos method of the detection zone gizmo
    /// </summary>
    private void OnDrawGizmos()
    {
        if (drawObject != null )
        {
            drawObject.defaultColor = Color.red;
            drawObject.OnDrawGizmos();
        }
    }

    /// <summary>
    /// Sets currentlyInContact to true if the player makes contact
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == 11) //Squid
        {
            currentlyInContact = true;
        }
    }

    /// <summary>
    /// Sets currentlyInContact to false if the player leaves the trigger
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == 11) //Squid
        {
            currentlyInContact = false;
        }
    }

    #region Helper Methods
    /// <summary>
    /// Increment the total detection value of the player after a delay
    /// </summary>
    /// <param name="incrementDelay"></param>
    /// <returns></returns>
    private IEnumerator IncrementDecection(float incrementDelay)
    {
        PlayerVars.instance.currentDetectionAmount += detectionIncrement;
        yield return new WaitForSeconds(incrementDelay);
        updating = false;
    }
    #endregion
}
