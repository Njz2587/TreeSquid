using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private RectTransform tutTransform;
    [SerializeField] private float showTime;
    private Text tutText;
    private Image tutPanel;

    [SerializeField] private Slider power;

    public float PowerValue { set { if (power != null) { power.value = value; } } }
    public string TutMessage { private get; set; }

    private void Awake()
    {
        tutText = tutTransform.GetComponentInChildren<Text>();
        tutPanel = tutTransform.GetComponent<Image>();

        StartCoroutine(Fade(tutText, 0.5f, 0, 2));
        StartCoroutine(Fade(tutPanel, 0.5f, 0, 2));

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ShowMessage("STOP THAT SQUID");
        }
    }

    private IEnumerator FadeInOut(Graphic graphic, float alphaStart, float alphaFinish, float time)
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

        StartCoroutine(Fade(graphic, alphaFinish, alphaStart, time));
    }

    private IEnumerator Fade(Graphic graphic, float alphaStart, float alphaFinish, float time)
    {
        float elapsedTime = 0;
        Color colorStart = graphic.color;
        colorStart.a = alphaStart;

        Color colorFinish = graphic.color;
        colorFinish.a = alphaFinish;

        graphic.color = colorStart;

        yield return new WaitForSeconds(showTime);

        while (elapsedTime < time)
        {
            graphic.color = Color.Lerp(graphic.color, colorFinish, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void ShowMessage(string message)
    {
        TutMessage = message;
        ShowMessage();
    }

    public void ShowMessage()
    {
        StopAllCoroutines();

        StartCoroutine(FadeInOut(tutText, 0, 0.5f, 2));
        StartCoroutine(FadeInOut(tutPanel, 0, 0.5f, 2));
    }

}
