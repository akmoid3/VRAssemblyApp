using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip screwSound;
    [SerializeField] private AudioClip popSound;

    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the AudioSource if it's not assigned in the Inspector
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.loop = false; 
        }
    }

    public void PlayScrewSound()
    {
        if (audioSource != null && screwSound != null)
        {
            audioSource.clip = screwSound; 
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
    }

    public void PlayPopSound()
    {
        if (audioSource != null && popSound != null)
        {
            audioSource.clip = popSound;
            audioSource.loop = false;
            audioSource.volume = 1.0f;
            audioSource.Play();
        }
    }


    public void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void SetPitch(float pitch)
    {
        if (audioSource != null)
        {
            audioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3.0f);
        }
    }
}
