using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVars : MonoBehaviour
{
    public float gameMusicVolumeScale = 1, gameSFXVolumeScale = 1;

    [HideInInspector]
    public static GameVars instance; //Singleton

    /// <summary>
    /// Define Singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Check for setup variables
    /// </summary>
    private void Update()
    {
        if(SlidersUI.instance)
        {
            if(gameMusicVolumeScale != SlidersUI.instance.musicSlider.value)
            {
                gameMusicVolumeScale = SlidersUI.instance.musicSlider.value;
            }

            if (gameSFXVolumeScale != SlidersUI.instance.sfxSlider.value)
            {
                gameSFXVolumeScale = SlidersUI.instance.sfxSlider.value;
            }
        }
    }
}
