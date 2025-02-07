using UnityEngine;

public class MotorSoundController : MonoBehaviour
{

    public AudioSource audioSource;

    public AudioClip idleClip;

    public float pitch;

    public float pitchMax;

    public float volume;

    public float volumeMax;

    public float transitionSpeed;
    
    public float value;

    public bool throttle;

    public bool mute;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource.enabled = !mute;
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.enabled = !mute;
        float target = 0;
        if (throttle)
        {
            target = 1;
        }

        value = Mathf.Lerp(value, target, transitionSpeed * Time.deltaTime);

        audioSource.pitch = Mathf.Lerp(pitch, pitchMax, value);
        audioSource.volume = Mathf.Lerp(volume, volumeMax, value);
    }
}
