using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private Dictionary<string, AudioClip> audioClips;

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

        // Load all audio clips from the Resources/Sounds folder
        LoadAllAudioClips();
    }

    private void LoadAllAudioClips()
    {
        audioClips = new Dictionary<string, AudioClip>();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");

        foreach (AudioClip clip in clips)
        {
            audioClips[clip.name] = clip;
        }
    }

    public void PlaySound(AudioSource audioSource, string clipName, bool loop = false, float volume = 1.0f)
    {
        if (audioClips.ContainsKey(clipName))
        {
            audioSource.clip = audioClips[clipName];
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"AudioManager: Sound '{clipName}' not found!");
        }
    }

    public void StopSound(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void SetPitch(AudioSource audioSource, float pitch)
    {
        if (audioSource != null)
        {
            audioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3.0f);
        }
    }

    public void SetVolume(AudioSource audioSource, float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
        }
    }
}
