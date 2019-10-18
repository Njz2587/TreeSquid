using System.Collections;
using UnityEngine;
using Invector.CharacterController;

public class PlayerController : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Character Settings")]
    public GameObject playerChest; //Bone The Player Will Be Launched From
    public LayerMask stickMask; //Mask Of What Layers The Player Can Stick Too

    public float MAX_FORCE = 330000; //The Value The Player Can Charge Too
    public float chargeIndex = 4000; //The Index The Value Charges By Each Frame While Held

    [Header("UI Settings")]
    [Space(10)]
    public Texture2D progressBarEmpty; //Texture for the empty charge meter
    public Texture2D progressBarFull; //Texture for the charging meter
    public Texture2D whiteBoarder; //Boarder around meter

    #region Stored Data
    private bool isStuck;
    private bool hasLaunched;
    private int stuckOnPreviousLayer;
    private GameObject stuckOnObject;
    private GameObject previousStuckObject;

    private float nudgePower;
    private float playerLaunchPower;

    private Vector2 iconSize = new Vector2(30, 400);
    private Vector2 uiOffset = new Vector2(20, 20);
    private float boarderRadius = 5f;
    #endregion
    #endregion

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        nudgePower = MAX_FORCE / 18;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        PlayerControls();
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
    /// Draws Charge Meter
    /// </summary>
    private void OnGUI()
    {
        //Launch power
        if (playerLaunchPower > 0)
        {
            Rect playerBarRect = new Rect(uiOffset.x, Screen.height - uiOffset.y, Screen.width / iconSize.x, (playerLaunchPower / (MAX_FORCE/(iconSize.y)) * -1));
            GUI.DrawTexture(GetShadowRect(GetShadowRect(playerBarRect, boarderRadius, new Rect(1, 1, 1, 1)), 2f, new Rect(1, 1, 1, 1)), whiteBoarder, ScaleMode.StretchToFill, true, 10.0F, Color.white, 0, 0);
            GUI.DrawTexture(GetShadowRect(playerBarRect, boarderRadius, new Rect(1, 1, 1, 1)), progressBarEmpty, ScaleMode.StretchToFill, true, 10.0F, Color.black, 0, 0);
            GUI.DrawTexture(playerBarRect, progressBarFull, ScaleMode.StretchToFill, true, 10.0F, Color.red, 0, 0);
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
    }

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
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            string keyCode = Input.inputString;

            ReleaseStick();

            if (playerChest.GetComponent<Rigidbody>().velocity.magnitude < 10)
            {
                if (keyCode == "w")
                {
                    playerChest.GetComponent<Rigidbody>().AddForce(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward * (nudgePower));
                }
                else if (keyCode == "a")
                {
                    playerChest.GetComponent<Rigidbody>().AddForce((playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.right * -1) * (nudgePower));
                }
                else if (keyCode == "d")
                {
                    playerChest.GetComponent<Rigidbody>().AddForce(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.right * (nudgePower));
                }
                else if (keyCode == "s")
                {
                    playerChest.GetComponent<Rigidbody>().AddForce((playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward * -1) * (nudgePower));
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
    }

    #region Helper Methods
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
