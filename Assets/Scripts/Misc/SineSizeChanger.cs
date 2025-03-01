using UnityEngine;

public class SineSizeChanger : MonoBehaviour
{

    public Vector3 sizeMultiplier;

    public float frequency;
    
    Vector3 initialSize;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialSize = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(initialSize, new Vector3(initialSize.x * sizeMultiplier.x, initialSize.y * sizeMultiplier.y, initialSize.z * sizeMultiplier.z), (Mathf.Sin(Time.time * frequency * Mathf.PI) + 1) /2);
    }
}
