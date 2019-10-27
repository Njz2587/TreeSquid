using System.Collections;
using UnityEngine;
using Invector.CharacterController;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Character Settings")]
    public GameObject playerChest; //Bone The Player Will Be Launched From
    public LayerMask stickMask; //Mask Of What Layers The Player Can Stick Too

    public float MAX_FORCE = 330000; //The Value The Player Can Charge Too
    public float chargeIndex = 4000; //The Index The Value Charges By Each Frame While Held
    [HideInInspector]
    public float playerLaunchPower;

    [Header("UI Settings")]
    [Space(10)]
    public Texture2D progressBarEmpty; //Texture for the empty charge meter
    public Texture2D progressBarFull; //Texture for the charging meter
    public Texture2D whiteBoarder; //Boarder around meter

    [Header("Audio Settings")]
    [Space(10)]
    public AudioSource audioSource;
    public AudioClip squidPop;
    public List<AudioClip> squidNudgeSounds;
    public List<AudioClip> squidSplatSounds;

    [HideInInspector]
    public PlayerController instance; //Singleton

    #region Stored Data
    private bool isStuck;
    private bool hasLaunched;
    private bool isNudging;
    private int stuckOnPreviousLayer;
    private GameObject stuckOnObject;
    private GameObject previousStuckObject;
    private Vector3 ignoreVector = new Vector3(1, 0, 1);

    private float nudgePower;

    private Vector2 iconSize = new Vector2(30, 400);
    private Vector2 uiOffset = new Vector2(20, 20);
    private float boarderRadius = 5f;

    private float defaultVolume;
    private enum SquidSound { Nudge, Splat, Launch, Detatch}
    #endregion
    #endregion

    /// <summary>
    /// Sets up the class singleton
    /// </summary>
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        nudgePower = MAX_FORCE / 18;
        defaultVolume = audioSource.volume;
        PlayerVars.instance.player = gameObject;
        PlayerVars.instance.sceneState = PlayerVars.SceneState.PlayerActive;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        if (PlayerVars.instance.sceneState == PlayerVars.SceneState.PlayerActive)
        {
            PlayerControls();
        }
    }

    /// <summary>
    /// Draws Gizmo sphere for the stickpoint
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (stuckOnObject != null)
        {
            Gizmos.DrawSphere(stuckOnObject.transform.position, 0.2f);
        }
    }

    /// <summary>
    /// Identifies if the player has collided with a layer it recognizes
    /// </summary>
    /// <param name="col"></param>
    private void OnCollisionEnter(Collision col)
    {
        if (!isStuck)
        {
            if (stickMask == (stickMask | 1 << col.gameObject.layer))
            {
                isStuck = true;
                hasLaunched = false;

                stuckOnObject = new GameObject();
                stuckOnObject.name = "StickPoint";
                stuckOnObject.AddComponent<SphereCollider>().isTrigger = true;
                stuckOnObject.AddComponent<Rigidbody>().isKinematic = true;

                #region Layer Config
                stuckOnPreviousLayer = col.gameObject.layer;
                if (col.gameObject.GetComponent<Rigidbody>())
                {
                    col.gameObject.layer = 10;                 
                }
                previousStuckObject = col.gameObject;
                #endregion

                stuckOnObject.transform.position = col.contacts[0].point;

                #region Joint Config
                CharacterJoint joint = gameObject.AddComponent<CharacterJoint>();
                //gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                joint.connectedBody = stuckOnObject.GetComponent<Rigidbody>();
                joint.autoConfigureConnectedAnchor = false;
                //joint.enableProjection = true;
                joint.anchor = transform.InverseTransformPoint(col.contacts[0].point);
                joint.axis = new Vector3(1f, 1f, 1f);
                joint.connectedAnchor = Vector3.zero;


                SoftJointLimit softLimit = joint.lowTwistLimit;
                softLimit.limit = 0;
                softLimit.bounciness = 1f;
                softLimit.contactDistance = 0;
                joint.lowTwistLimit = softLimit;
                joint.highTwistLimit = softLimit;
                softLimit.limit = 360;
                joint.swing1Limit = softLimit;
                joint.swing2Limit = softLimit;
                #endregion

                //gameObject.transform.parent = stuckOnObject.transform;
                stuckOnObject.transform.parent = col.gameObject.transform;
            }

            if (hasLaunched)
            {
                if (col.gameObject.layer == 12) //Floor
                {
                    hasLaunched = false;
                }
            }
        }

        PlaySquidSound(SquidSound.Nudge, ScaleVolumeToForce((col.impulse /Time.fixedDeltaTime).magnitude, 100000));
    }

    /// <summary>
    /// Reset the squid if they stayput for too long
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay(Collision collision)
    {
        if (hasLaunched)
        {
            if (gameObject.GetComponent<Rigidbody>().velocity.magnitude < 0.1f)
            {
                hasLaunched = false;
            }
        }
    }

    /// <summary>
    /// Players Controls
    /// </summary>
    private void PlayerControls()
    {
        #region Nudge
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if(isStuck)
            {
                PlaySquidSound(SquidSound.Detatch, defaultVolume);
                ReleaseStick();
            }

            if (playerChest.GetComponent<Rigidbody>().velocity.magnitude < 10)
            {
                if (playerChest.GetComponent<Rigidbody>().velocity.magnitude > 1)
                {
                    PlaySquidSound(SquidSound.Nudge, defaultVolume/2);           
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    Debug.Log(RemoveVectorComponents(GetUnitDirectionVector(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward), ignoreVector));
                    playerChest.GetComponent<Rigidbody>().AddForce(RemoveVectorComponents(GetUnitDirectionVector(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward), ignoreVector) * (nudgePower));
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    playerChest.GetComponent<Rigidbody>().AddForce(RemoveVectorComponents(GetUnitDirectionVector(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.right * -1), ignoreVector) * (nudgePower));
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    playerChest.GetComponent<Rigidbody>().AddForce(RemoveVectorComponents(GetUnitDirectionVector(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.right), ignoreVector) * (nudgePower));
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    playerChest.GetComponent<Rigidbody>().AddForce(RemoveVectorComponents(GetUnitDirectionVector(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward * -1), ignoreVector) * (nudgePower));
                }
            }
        }
        #endregion

        #region Launch
        if (hasLaunched == false)
        {
            if (Input.GetMouseButton(0)) //left mouse down
            {
                if (playerLaunchPower < (MAX_FORCE))
                {
                    playerLaunchPower += chargeIndex;
                }
            }
            else if (Input.GetMouseButtonUp(0) && playerLaunchPower > 0)
            {
                //Play Launch Sound Here
                PlaySquidSound(SquidSound.Launch, ScaleVolumeToForce(playerLaunchPower, MAX_FORCE));

                if (isStuck)
                {
                    ReleaseStick();
                }
                hasLaunched = true;
                playerChest.GetComponent<Rigidbody>().AddForce(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward * (playerLaunchPower));
                playerLaunchPower = 0;
            }
        }
        #endregion

        #region Zoom
        if (playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam)
        {
            if (Input.GetMouseButton(1))
            {
                if (playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance > 0 && playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance - 5 > 0)
                {
                    playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance -= 5;
                }
            }
            else if (playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance != playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().initialDefaultDistance)
            {
                playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance += 1;
            }
        }
        #endregion

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
        #endregion
    }

    /// <summary>
    /// Resets all of the player's charge and stick values
    /// </summary>
    public void ResetPlayerCharge()
    {
        playerLaunchPower = 0;
        hasLaunched = true;
        ReleaseStick();
    }

    #region Helper Methods

    /// <summary>
    /// Depending on the passed enum, plays a squid soundeffect
    /// </summary>
    /// <param name="soundTypeToPlay"></param>
    /// <param name="volume"></param>
    private void PlaySquidSound(SquidSound soundTypeToPlay, float volume)
    {
        audioSource.volume = volume;
        switch(soundTypeToPlay)
        {
            case SquidSound.Detatch:
                if(squidPop && !audioSource.isPlaying) { AudioManager.instance.PlaySound(audioSource, squidPop); }
                break;
            case SquidSound.Nudge:
                if (squidNudgeSounds != null && squidNudgeSounds.Count > 0 && !audioSource.isPlaying) { AudioManager.instance.PlaySound(audioSource, squidNudgeSounds[Random.Range(0, squidNudgeSounds.Count)]); }                    
                break;
            case SquidSound.Splat:
                if (squidSplatSounds != null && squidSplatSounds.Count > 0) { AudioManager.instance.PlaySound(audioSource, squidSplatSounds[Random.Range(0, squidSplatSounds.Count)]); }               
                break;
            case SquidSound.Launch:
                if (squidNudgeSounds != null && squidNudgeSounds.Count > 0) { AudioManager.instance.PlaySound(audioSource, squidNudgeSounds[Random.Range(0, squidNudgeSounds.Count)]); }
                break;
        }
    }

    /// <summary>
    /// Resets the player's ability to stick to walls
    /// </summary>
    /// <param name="stickResetDelay"></param>
    /// <returns></returns>
    private IEnumerator ResetStick(float stickResetDelay)
    {
        //Debug.Log("Attempting To Reset Stick...");
        if (stuckOnObject)
        {
            if (stuckOnObject.transform.childCount == 0)
            {
                yield return new WaitForSeconds(stickResetDelay);
                if (gameObject.transform.parent == null)
                {
                    isStuck = false;
                    previousStuckObject.layer = stuckOnPreviousLayer;
                    Destroy(stuckOnObject);
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(stickResetDelay);
            isStuck = false;
        }
        //Debug.Log("Reset!");
    }

    /// <summary>
    /// Releases player from wall and cleans up needed data
    /// </summary>
    private void ReleaseStick()
    {
        gameObject.transform.parent = null;
        CharacterJoint joint = gameObject.GetComponent<CharacterJoint>();
        if (joint != null)
        {
            Destroy(joint);
        }
        StartCoroutine(ResetStick(0.12f));
    }

    /// <summary>
    /// Scales a volume based on a force
    /// </summary>
    /// <param name="force"></param>
    /// <param name="maxForce"></param>
    /// <returns></returns>
    private float ScaleVolumeToForce(float force, float maxForce)
    {
        //Debug.Log(gameObject.name + " Impacted With With A Force Of " + force);
        float impactVolume = 0.0f;

        if (force > 0.05)
        {
            impactVolume = (force / maxForce);

            if (impactVolume > 1.0)
            {
                impactVolume = 1.0f;
            }

            if (impactVolume < 0.0)
            {
                impactVolume = 0.0f;
            }
        }

        return impactVolume;
    }

    /// <summary>
    /// Returns a Unit Direction Vector exactly 1.0
    /// </summary>
    /// <param name="vectorToExtract"></param>
    /// <returns></returns>
    private Vector3 GetUnitDirectionVector(Vector3 vectorToExtract)
    {
        return new Vector3(GetValueDirection(vectorToExtract.x), GetValueDirection(vectorToExtract.y), GetValueDirection(vectorToExtract.z));
    }

    /// <summary>
    /// Multiplies all components of a vector by another to remove components if done by 0
    /// </summary>
    /// <param name="vectorToEdit"></param>
    /// <param name="componentsToRemove"></param>
    /// <returns></returns>
    private Vector3 RemoveVectorComponents(Vector3 vectorToEdit, Vector3 componentsToRemove)
    {
        return new Vector3(vectorToEdit.x * componentsToRemove.x, vectorToEdit.y * componentsToRemove.y, vectorToEdit.z * componentsToRemove.z);
    }

    /// <summary>
    /// Determines whether a number is positive, negative, or zero
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private float GetValueDirection(float value)
    {
        if (value < 0)
        {
            return -1;
        }
        else if (value == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// Returns the Rectangle of a shadow based on given current rectangle
    /// </summary>
    /// <param name="baseRect"></param>
    /// <param name="shadowWidth"></param>
    /// <param name="flipRect"></param>
    /// <returns></returns>
    private Rect GetShadowRect(Rect baseRect, float shadowWidth, Rect flipRect)
    {
        return new Rect((baseRect.x - (shadowWidth * flipRect.x)), (baseRect.y - shadowWidth) * flipRect.y, (baseRect.width + ((shadowWidth * 2) * flipRect.width)), (baseRect.height + (shadowWidth * 2)) * flipRect.height);
    }
    #endregion
}
