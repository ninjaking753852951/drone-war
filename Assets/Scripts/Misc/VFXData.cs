using UnityEngine;

[CreateAssetMenu(menuName = "VFX/VFX Data")]
public class VFXData : ScriptableObject
{
    public GameObject prefab;   // Prefab for the VFX
    public float lifetime;      // Lifetime of the effect
}
