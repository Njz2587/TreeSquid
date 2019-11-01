using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EffectManager : MonoBehaviour
{
    // Singleton
    public static EffectManager Instance;
    // inspector Assigned Variables
    [SerializeField] PostProcessVolume postProcessVolume;
    // Public Variables
    public PostProcessProfile defaultPostProcessProfile;
    public PostProcessProfile vignettePostProcessProfile;
    public bool vignetteActive = false;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if(vignetteActive)
        {
            postProcessVolume.profile = vignettePostProcessProfile;
        }
        else
        {
            postProcessVolume.profile = defaultPostProcessProfile;
        }
    }

    public void SetVignette(bool set)
    {
        vignetteActive = set;
    }
}
