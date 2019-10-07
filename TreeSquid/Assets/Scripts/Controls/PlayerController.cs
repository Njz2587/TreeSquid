using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.CharacterController;

public class PlayerController : MonoBehaviour
{
    public GameObject playerChest;
    public LayerMask stickMask;
    public Texture2D progressBarEmpty, progressBarFull, whiteBoarder;

    public bool isStuck;
    public GameObject stuckOnObject = null;
    public GameObject previousStuckObject;
    public int stuckOnPreviousLayer;
    public float nudgePower;
    private Vector3 defaultScale;
    private float playerLaunchPower;
    private float iconSize = 60, uiOffset = 6;
    private float MAX_FORCE = 22000 * 30;
    private float chargeIndex = 8000;
    private float boarderRadius = 5f;

    // Start is called before the first frame update
    void Start()
    {
        nudgePower = MAX_FORCE / 18;
        defaultScale = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerControls();
    }


    private void OnCollisionEnter(Collision col)
    {
        if (!isStuck)
        {
            if (stickMask == (stickMask | 1 << col.gameObject.layer))
            {
                isStuck = true;
                GetComponent<Rigidbody>().isKinematic = true;

                stuckOnObject = new GameObject();
                stuckOnObject.name = "StickPoint";

                stuckOnPreviousLayer = col.gameObject.layer;

                if (col.gameObject.GetComponent<Rigidbody>())
                {
                    col.gameObject.layer = 10;                 
                }

                previousStuckObject = col.gameObject;
                stuckOnObject.transform.position = col.contacts[0].point;
                gameObject.transform.root.parent = stuckOnObject.transform;
                stuckOnObject.transform.parent = col.gameObject.transform;
            }
        }
    }


    public IEnumerator ResetStick(float stickResetDelay)
    {
        //Debug.Log("Attempting To Reset Stick...");
        if (stuckOnObject)
        {
            if (stuckOnObject.transform.childCount == 0)
            {
                yield return new WaitForSeconds(stickResetDelay);
                isStuck = false;
                previousStuckObject.layer = stuckOnPreviousLayer;
                Destroy(stuckOnObject);
            }
        }
        else
        {
            yield return new WaitForSeconds(stickResetDelay);
            isStuck = false;
        }
        //Debug.Log("Reset!");
    }

    void PlayerControls()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            string keyCode = Input.inputString;

            //playerChest.GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().isKinematic = false;
            gameObject.transform.parent = null;
            StartCoroutine(ResetStick(0.01f));

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
       

        #region Launch
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
                GetComponent<Rigidbody>().isKinematic = false;
                gameObject.transform.parent = null;
                //Debug.Log("Reset Stick...");
                StartCoroutine(ResetStick(0.1f));
            }
            playerChest.GetComponent<Rigidbody>().AddForce(playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.transform.forward * (playerLaunchPower));
            playerLaunchPower = 0;
        }
        #endregion

        #region Zoom
        if (playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam)
        {
            if (Input.GetMouseButton(1))
            {
                if (playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance > 0 && playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance - 20 > 0)
                {
                    playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance -= 20;
                }
            }
            else if (playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance != playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().initialDefaultDistance)
            {
                playerChest.GetComponent<vThirdPersonInput>().PlayerOneCam.GetComponent<vThirdPersonCamera>().defaultDistance += 2;
            }
        }
        #endregion
    }

    void OnGUI()
    {
        //Launch power
        if (playerLaunchPower > 0)
        {
            Rect playerBarRect = new Rect(uiOffset + ((iconSize - iconSize / 3f) / 2), (Screen.height - iconSize - (uiOffset * 2)), iconSize / 3f, (playerLaunchPower / 4000) * -1);
            GUI.DrawTexture(GetShadowRect(GetShadowRect(playerBarRect, boarderRadius, new Rect(1, 1, 1, 1)), 2f, new Rect(1, 1, 1, 1)), whiteBoarder, ScaleMode.StretchToFill, true, 10.0F, Color.white, 0, 0);
            GUI.DrawTexture(GetShadowRect(playerBarRect, boarderRadius, new Rect(1, 1, 1, 1)), progressBarEmpty, ScaleMode.StretchToFill, true, 10.0F, Color.black, 0, 0);
            GUI.DrawTexture(playerBarRect, progressBarFull, ScaleMode.StretchToFill, true, 10.0F, Color.red, 0, 0);
        }           
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (stuckOnObject != null)
        {
            Gizmos.DrawSphere(stuckOnObject.transform.position, 0.2f);
        }
    }

    Rect GetShadowRect(Rect baseRect, float shadowWidth, Rect flipRect)
    {
        return new Rect((baseRect.x - (shadowWidth * flipRect.x)), (baseRect.y - shadowWidth) * flipRect.y, (baseRect.width + ((shadowWidth * 2) * flipRect.width)), (baseRect.height + (shadowWidth * 2)) * flipRect.height);
    }
}
