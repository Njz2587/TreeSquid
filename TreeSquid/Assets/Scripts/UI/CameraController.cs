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
    private bool squidTransition = false, hasAlreadyDoneTutorial = false;

    [SerializeField] private ControlsUI controlsUI;

    private Camera m_camera;

    private void Awake()
    {
        Transform squidCamT = squid.GetComponentInChildren<Camera>().transform;
        squidCamTransform = new GameObject("fakeSquid").transform;
        squidCamTransform.position = squidCamT.position;
        squidCamTransform.rotation = squidCamT.rotation;

        m_camera = GetComponent<Camera>();
    }
    private void Start()
    {
        PlayerVars.instance.isUsingMenu = true;
        PlayerVars.instance.player = squid.GetComponentInChildren<PlayerController>().gameObject;
        PlayerVars.instance.DisablePlayer();
        squid.SetActive(false);
        ShowMenus(true);
    }
    private void Update()
    {
        if (m_camera.enabled)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (timer < lerpTime)
        {
            timer += Time.deltaTime;
            if(timer > lerpTime)
            {
                timer = lerpTime;
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;

                if (squidTransition)
                {                  
                    
                    controlsUI.gameObject.SetActive(true);

                    m_camera.enabled = (false);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    ShowMenus(false);
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerVars.instance.sceneState = PlayerVars.SceneState.PlayerActive;
            Time.timeScale = 1;
            PlayerVars.instance.isUsingMenu = false;
            LoadScene("Level_1");
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

    private void ShowMenus(bool show)
    {
        foreach(Transform t in cameraPositions)
        {
            t.parent.gameObject.SetActive(show);
        }
    }

    public void GoToCameraPosition(int index)
    {
        if (squidTransition)
        {
            ShowMenus(true);
            m_camera.enabled = true;

            Transform squidCamT = squid.GetComponentInChildren<Camera>().transform;
            squidCamTransform.position = squidCamT.position;
            squidCamTransform.rotation = squidCamT.rotation;

            transform.SetParent(squidCamTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            squid.SetActive(false);
            PlayerVars.instance.DisablePlayer();
            controlsUI.gameObject.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            squidTransition = false;
        }
        
        transform.SetParent(cameraPositions[index]);
        timer = 0;
        PlayerVars.instance.sceneState = PlayerVars.SceneState.PlayerActive;
        PlayerVars.instance.isUsingMenu = true;
        Time.timeScale = 1;
        GetComponent<AudioListener>().enabled = true;
    }

    public void GoToSquid()
    {
        if (hasAlreadyDoneTutorial == false)
        {
            GetComponent<AudioListener>().enabled = false;
            transform.SetParent(squidCamTransform);
            timer = 0;

            squid.SetActive(true);
            PlayerVars.instance.isUsingMenu = false;
            PlayerVars.instance.EnablePlayer();
            squidTransition = true;

            hasAlreadyDoneTutorial = true;
        }
        else
        {
            LoadScene("Level_1");
        }
    }
}
