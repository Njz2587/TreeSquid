using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private RectTransform tutTransform;
    private Text tutText;
    private Image tutPanel;

    [SerializeField] private Slider power;
    [SerializeField] private PlayerController player;

    private void OnEnable()
    {
        tutText = tutTransform.GetComponentInChildren<Text>();
        tutPanel = tutTransform.GetComponent<Image>();

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            StartCoroutine(Tutorial());
        }
        else
        {
            tutPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        power.value = player.playerLaunchPower / player.MAX_FORCE;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopAllCoroutines();
            StartCoroutine(Tutorial());
        }
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


    private IEnumerator FadeInOut(Graphic graphic, float alphaStart, float alphaFinish, float time = 2, float holdTime = 5)
    {
        float elapsedTime = 0;
        Color colorStart = graphic.color;
        colorStart.a = alphaStart;

        Color colorFinish = graphic.color;
        colorFinish.a = alphaFinish;

        graphic.color = colorStart;

        while (elapsedTime < time)
        {
            graphic.color = Color.Lerp(graphic.color, colorFinish, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(Fade(graphic, alphaFinish, alphaStart, time, holdTime));
    }

    private IEnumerator Fade(Graphic graphic, float alphaStart, float alphaFinish, float time = 2, float holdTime = 5)
    {
        float elapsedTime = 0;
        Color colorStart = graphic.color;
        colorStart.a = alphaStart;

        Color colorFinish = graphic.color;
        colorFinish.a = alphaFinish;

        graphic.color = colorStart;

        yield return new WaitForSeconds(holdTime);

        while (elapsedTime < time)
        {
            graphic.color = Color.Lerp(graphic.color, colorFinish, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void ShowMessage(string message, float time = 2, float holdTime = 5)
    {
        tutText.text = message;
        ShowMessage(time, holdTime);
    }

    public void ShowMessage(float time = 2 , float holdTime = 5)
    {
        StartCoroutine(FadeInOut(tutText, 0, 0.5f, time, holdTime));
        StartCoroutine(FadeInOut(tutPanel, 0, 0.5f, time, holdTime));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

}
