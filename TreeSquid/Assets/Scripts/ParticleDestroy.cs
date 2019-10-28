using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroy : MonoBehaviour
{
    public float timeOffset;
	// Use this for initialization
	void Start ()
    {
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration + timeOffset);
    }
}
