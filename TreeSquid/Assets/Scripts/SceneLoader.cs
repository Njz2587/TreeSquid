using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer == 11) //Squid Layer
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
