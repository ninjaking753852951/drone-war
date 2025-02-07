using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/VFX")]
public class VFXData : ScriptableObject
{
    public GameObject prefab;   // Prefab for the VFX
    public float lifetime;      // Lifetime of the effect
    public SFXData sfx;
}
