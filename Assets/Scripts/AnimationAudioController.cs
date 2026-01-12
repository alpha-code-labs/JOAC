using UnityEngine;

public class AnimationAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float pitch = .88f;

    void Start()
    {
        if (audioSource != null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false; // Disable Play On Awake for dynamically added AudioSource
        }

        // Set the pitch
        audioSource.pitch = pitch;
    }

    public void PlayAudioClip()
    {
        Debug.Log("Playing Audio Clip");
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void SetPitch(float newPitch)
    {
        pitch = newPitch;
        if (audioSource != null)
            audioSource.pitch = pitch;
    }
}