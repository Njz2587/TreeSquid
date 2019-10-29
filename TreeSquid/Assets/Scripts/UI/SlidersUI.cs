using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidersUI : MonoBehaviour
{
    public Slider musicSlider, sfxSlider;

    [HideInInspector]
    public static SlidersUI instance; //Singleton

    /// <summary>
    /// Define Singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
