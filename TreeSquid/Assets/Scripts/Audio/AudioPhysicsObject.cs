using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPhysicsObject : MonoBehaviour
{
    public float MAX_FORCE = 250;
    public AudioSource audioSource;
    public AudioClip collisionSound;

    /// <summary>
    /// Handles Object Collision
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (audioSource && collisionSound)
        {
            PlayerCollideAudio(collision);
        }
    }

    /// <summary>
    /// Plays specified clip on specified audioSource
    /// </summary>
    /// <param name="col"></param>
    private void PlayerCollideAudio(Collision col)
    {
        audioSource.volume = ScaleVolumeToForce((col.impulse / Time.fixedDeltaTime).magnitude, 250);
        if (audioSource.isPlaying == false)
        {
            audioSource.PlayOneShot(collisionSound);
        }
    }

    /// <summary>
    /// Scales a volume based on a force
    /// </summary>
    /// <param name="force"></param>
    /// <param name="maxForce"></param>
    /// <returns></returns>
    /// <summary>
    /// Scales a volume based on a force
    /// </summary>
    /// <param name="force"></param>
    /// <param name="maxForce"></param>
    /// <returns></returns>
    private float ScaleVolumeToForce(float force, float maxForce)
    {
        //Debug.Log(gameObject.name + " Impacted With With A Force Of " + force);
        float impactVolume = 0.0f;

        if (force > 0.05)
        {
            impactVolume = (force / maxForce);

            if (impactVolume > 1.0)
            {
                impactVolume = 1.0f;
            }

            if (impactVolume < 0.0)
            {
                impactVolume = 0.0f;
            }
        }

        return impactVolume;
    }
}
