using UnityEngine;

namespace LowPolyAnimalPack
{
  public class PlaySound : MonoBehaviour
  {
    [SerializeField]
    private AudioClip animalSound;
    [SerializeField]
    private AudioClip walking;
    [SerializeField]
    private AudioClip eating;
    [SerializeField]
    private AudioClip running;
    [SerializeField]
    private AudioClip attacking;
    [SerializeField]
    private AudioClip death;
    [SerializeField]
    private AudioClip sleeping;

    void AnimalSound()
    {
      if (animalSound)
      {
        AudioManagerAnimals.PlaySound(animalSound, transform.position);
      }
    }

    void Walking()
    {
      if (walking)
      {
        AudioManagerAnimals.PlaySound(walking, transform.position);
      }
    }

    void Eating()
    {
      if (eating)
      {
        AudioManagerAnimals.PlaySound(eating, transform.position);
      }
    }

    void Running()
    {
      if (running)
      {
        AudioManagerAnimals.PlaySound(running, transform.position);
      }
    }

    public void Attacking()
    {
      if (attacking)
      {
        AudioManagerAnimals.PlaySound(attacking, transform.position);
      }
    }

    void Death()
    {
      if (death)
      {
        AudioManagerAnimals.PlaySound(death, transform.position);
      }
    }

    void Sleeping()
    {
      if (sleeping)
      {
        AudioManagerAnimals.PlaySound(sleeping, transform.position);
      }
    }
  }
}