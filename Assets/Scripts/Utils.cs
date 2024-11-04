using System.Collections.Generic;
using UnityEngine;
public static class Utils
{
    public static Rigidbody FindParentRigidbody(Transform startTransform, Rigidbody avoid)
    {
        Transform currentTransform = startTransform;

        while (currentTransform != null)
        {
            Rigidbody rb = currentTransform.GetComponent<Rigidbody>();
            if (rb != null && rb != avoid)
            {
                return rb;
            }
            currentTransform = currentTransform.parent;
        }

        return null; // No Rigidbody found in the hierarchy
    }

    public static Transform ClosestTo(List<Transform> transforms, Vector3 target)
    {
        if (transforms == null || transforms.Count == 0)
            return null; // Return null if the list is empty or null

        Transform closestTransform = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform transform in transforms)
        {
            // Calculate the squared distance between the current transform and the target
            float distanceSqr = (transform.position - target).sqrMagnitude;

            // If this transform is closer, update the closestTransform
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestTransform = transform;
            }
        }

        return closestTransform;
    }
    
    public static Transform CursorScan()
    {
        // Cast a ray from the cursor to the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform; // Return the transform of the hit object
        }
        return null;
    }
    
    public static Vector3 CursorScanPos()
    {
        // Cast a ray from the cursor to the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point; // Return the transform of the hit object
        }
        return Vector3.zero;
    }
}
