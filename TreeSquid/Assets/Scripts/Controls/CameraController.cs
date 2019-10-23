using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform[] cameraPositions;

    private const float lerpTime = 1;
    private float timer = 0;

    [SerializeField] private GameObject squid;
    private Transform squidCamTransform;
    private bool transitionedToSquid = false;

    private void Awake()
    {
        Transform squidCamT = squid.GetComponentInChildren<Camera>().transform;
        squidCamTransform = new GameObject("fakeSquid").transform;
        squidCamTransform.position = squidCamT.position;
        squidCamTransform.rotation = squidCamT.rotation;
    }

    private void Update()
    {
        if (timer < lerpTime)
        {
            timer += Time.deltaTime;
            if(timer > lerpTime)
            {
                timer = lerpTime;
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;

                if (transitionedToSquid)
                {
                    squid.SetActive(true);
                    gameObject.GetComponent<Camera>().enabled = (false);
                }

            }
            else
            {
                float p = timer / lerpTime;
                p = p * p * (3 - 2 * p);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, p);
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, p);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GoToCameraPosition(0);
        }
    }

    //////////////////////
    // BUTTON FUNCTIONS //
    //////////////////////

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadScene(string scenename)
    {
        SceneManager.LoadScene(scenename, LoadSceneMode.Single);
    }

    public void GoToCameraPosition(int index)
    {
        if (transitionedToSquid)
        {
            gameObject.GetComponent<Camera>().enabled = true;

            Transform squidCamT = squid.GetComponentInChildren<Camera>().transform;
            squidCamTransform.position = squidCamT.position;
            squidCamTransform.rotation = squidCamT.rotation;

            transform.SetParent(squidCamTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            squid.SetActive(false);
            transitionedToSquid = false;
        }
        
        transform.SetParent(cameraPositions[index]);
        timer = 0;
    }

    public void GoToSquid()
    {
        transform.SetParent(squidCamTransform);
        timer = 0;

        transitionedToSquid = true;
    }

}
