using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Object sceneToLoad;

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.name == "Player")
        {
            SceneManager.LoadScene(sceneToLoad.name);
        }
    }
}
