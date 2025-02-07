using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SFX")]
public class SFXData : ScriptableObject
{
    [SerializeField]
    List<AudioClip> clip;
    public float volume = 0.5f;
    public float basePitch = 1;
    public float pitchVariance = 0;
    public float spatialBlend = 0;
    
    public AudioClip Clip() => clip[Random.Range(0, clip.Count)];

}
