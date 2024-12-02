using System.Collections;
using UnityEngine;

public class TargetFlasher : MonoBehaviour
{
    public Renderer rend;
    private Color originalColor;

    void Start()
    {
        // Store the original color of the material
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the hit object contains a Projectile script
        if (other.transform.root.GetComponent<Projectile>() != null)
        {
            // If so, set the material color to red for 0.2 seconds
            StartCoroutine(FlashRed());
        }
    }

    private IEnumerator FlashRed()
    {
        if (rend != null)
        {
            // Change material color to red
            rend.material.color = Color.red;

            // Wait for 0.2 seconds
            yield return new WaitForSeconds(0.2f);

            // Reset material color to the original
            rend.material.color = originalColor;
        }
    }
}
