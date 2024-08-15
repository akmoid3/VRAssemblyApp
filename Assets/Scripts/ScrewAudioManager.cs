using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrewAudioManager : MonoBehaviour
{
    public static ScrewAudioManager Instance { get; private set; }

    [SerializeField] private AudioSource screwAudioSource;

    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the AudioSource if it's not assigned in the Inspector
        if (screwAudioSource == null)
        {
            screwAudioSource = GetComponent<AudioSource>();
        }

        if (screwAudioSource != null)
        {
            screwAudioSource.loop = true;
        }
    }

    public void PlayScrewSound()
    {
        if (screwAudioSource != null && !screwAudioSource.isPlaying)
        {
            screwAudioSource.Play();
        }
    }

    public void StopScrewSound()
    {
        if (screwAudioSource != null && screwAudioSource.isPlaying)
        {
            screwAudioSource.Stop();
        }
    }

    public void SetPitch(float pitch)
    {
        if (screwAudioSource != null)
        {
            screwAudioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3.0f);
        }
    }
}
