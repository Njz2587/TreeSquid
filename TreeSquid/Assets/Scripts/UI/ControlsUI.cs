using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private Graphic tutorialPanel;
    [SerializeField] private Text tutorialText;

    [SerializeField] private Graphic pausePanel;
    [SerializeField] private Graphic gameOverPanel;
    private bool paused;

    [SerializeField] private Slider power;
    [SerializeField] private PlayerController player;
    [SerializeField] private Image crosshair;

    [SerializeField] private Text percentage;



    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            percentage.enabled = false;
            StartCoroutine(Tutorial());
        }
        else
        {
            tutorialPanel.gameObject.SetActive(false);
        }

        //StartCoroutine(Fade(pausePanel, 0, 0, 0, 0));
        pausePanel.gameObject.SetActive(false);
        StartCoroutine(Fade(gameOverPanel, 0, 0, 0, 0));

        PlayerVars.instance.GameOverAction += ShowGameOver;
    }

    private void Update()
    {
        power.value = player.playerLaunchPower / player.MAX_FORCE;
        crosshair.enabled = power.value > 0;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopAllCoroutines();
            StartCoroutine(Tutorial());
        }

        pausePanel.gameObject.SetActive(PlayerVars.instance.sceneState.Equals(PlayerVars.SceneState.PlayerPaused));

        percentage.text = PlayerVars.instance.currentDetectionAmount + "%";
    }

    public void ShowGameOver()
    {
        gameOverPanel.gameObject.SetActive(true);

        StartCoroutine(Fade(gameOverPanel, 0, 1, 1, 0));
    }

    public IEnumerator Tutorial()
    {
        float time = 1;
        float holdTime = 3;

        ShowMessage("Mouse: look around", time, holdTime);
        yield return new WaitForSeconds(time * 2 + holdTime);

        ShowMessage("Left Mouse: Launch squid", time, holdTime);
        yield return new WaitForSeconds(time * 2 + holdTime);

        ShowMessage("Right Mouse: Zoom", time, holdTime);
        yield return new WaitForSeconds(time * 2 + holdTime);

        ShowMessage("WASD: Nudge/Pop off walls", time, holdTime);
        yield return new WaitForSeconds(time * 2 + holdTime);

        ShowMessage("Esc: begin game", time, holdTime);
        yield return new WaitForSeconds(time * 2 + holdTime);

        ShowMessage("Q: replay tutorial", time, holdTime);
        yield return new WaitForSeconds(time * 2 + holdTime);

    }


    private IEnumerator FadeInOut(Graphic parentGraphic, float alphaStart, float alphaFinish, float time = 2, float holdTime = 5)
    {
        float elapsedTime = 0;

        Graphic[] graphics = parentGraphic.GetComponentsInChildren<Graphic>();

        foreach (Graphic graphic in graphics)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alphaStart);
        }

        while (elapsedTime < time)
        {
            float alphaNow = Mathf.Lerp(parentGraphic.color.a, alphaFinish, (elapsedTime / time));
            foreach (Graphic graphic in graphics)
            {
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alphaNow);
            }

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(Fade(parentGraphic, alphaFinish, alphaStart, time, holdTime));
    }

    private IEnumerator Fade(Graphic parentGraphic, float alphaStart, float alphaFinish, float time = 2, float holdTime = 5)
    {
        float elapsedTime = 0;

        Graphic[] graphics = parentGraphic.GetComponentsInChildren<Graphic>();

        foreach (Graphic graphic in graphics)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alphaStart);
        }

        yield return new WaitForSeconds(holdTime);

        while (elapsedTime < time)
        {
            float alphaNow = Mathf.Lerp(parentGraphic.color.a, alphaFinish, (elapsedTime / time));
            foreach (Graphic graphic in graphics)
            {
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alphaNow);
            }
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }


    public void Resume()
    {
        Debug.Log("Resume");
        PlayerVars.instance.sceneState = PlayerVars.SceneState.PlayerActive;
        Time.timeScale = 1;
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowMessage(string message, float time = 2, float holdTime = 5)
    {
        tutorialText.text = message;
        ShowMessage(time, holdTime);
    }

    public void LoadScene(string scenename)
    {
        Debug.Log("Return to menu");
        SceneManager.LoadScene(scenename, LoadSceneMode.Single);
    }

    public void ShowMessage(float time = 2 , float holdTime = 5)
    {
        StartCoroutine(FadeInOut(tutorialPanel, 0, 1, time, holdTime));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

}
