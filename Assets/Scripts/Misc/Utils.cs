using System.Collections.Generic;
using System.Linq;
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
    
    public static Transform FurthestFrom(List<Transform> transforms, Vector3 target)
    {
        if (transforms == null || transforms.Count == 0)
            return null; // Return null if the list is empty or null

        Transform furthestTransform = null;
        float furthestDistance = 0;

        foreach (Transform transform in transforms)
        {
            // Calculate the squared distance between the current transform and the target
            float distanceSqr = (transform.position - target).sqrMagnitude;

            // If this transform is closer, update the closestTransform
            if (distanceSqr > furthestDistance)
            {
                furthestDistance = distanceSqr;
                furthestTransform = transform;
            }
        }

        return furthestTransform;
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
    
    public static Vector3 CalculateAveragePosition(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            Debug.LogWarning("Position list is empty or null.");
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        foreach (Vector3 position in positions)
        {
            sum += position;
        }

        return sum / positions.Count;
    }

    public static void SetChildrenToColour(GameObject parentObj, Color colour)
    {
        List<Renderer> rends = parentObj.GetComponentsInChildren<Renderer>().ToList();
        foreach (Renderer rend in rends)
        {
            rend.material.color = colour;
        }
    }
    
    public static void DestroyAllDrones()
    {
        List<DroneBlock> machineBlocks = GameObject.FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToList();
        foreach (DroneBlock machineBlock in machineBlocks)
        {
            GameObject.Destroy(machineBlock.gameObject);
        }
    }
    
    public static List<Transform> GetTransformsFromComponents<T>(List<T> components) where T : MonoBehaviour
    {
        List<Transform> transforms = new List<Transform>();

        foreach (T component in components)
        {
            if (component != null)
            {
                transforms.Add(component.transform);
            }
        }

        return transforms;
    }
    
    public static void DestroyAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    
    public static float CalculateBoundingSphereRadius(Rigidbody rb)
    {
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody is null.");
            return 0f;
        }

        // Get all colliders attached to the Rigidbody and its children
        Collider[] colliders = rb.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found on the Rigidbody or its children.");
            return 0f;
        }

        Vector3 center = rb.worldCenterOfMass; // Use the Rigidbody's center of mass
        float maxDistance = 0f;

        foreach (Collider collider in colliders)
        {
            // Get the bounds of the collider
            Bounds bounds = collider.bounds;

            // Calculate the farthest distance from the Rigidbody's center of mass
            Vector3[] corners = new Vector3[8];
            bounds.GetCorners(corners);
            foreach (Vector3 corner in corners)
            {
                float distance = Vector3.Distance(center, corner);
                maxDistance = Mathf.Max(maxDistance, distance);
            }
        }

        return maxDistance;
    }
    
    public static void GetCorners(this Bounds bounds, Vector3[] corners)
    {
        if (corners.Length < 8)
            throw new System.ArgumentException("Corners array must have a length of at least 8.");

        corners[0] = bounds.min; // Bottom-left-near
        corners[1] = bounds.max; // Top-right-far

        // Bottom-right-near
        corners[2] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        // Bottom-left-far
        corners[3] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);

        // Top-right-near
        corners[4] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        // Top-left-far
        corners[5] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);

        // Top-left-near
        corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        // Bottom-right-far
        corners[7] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
    }
    
}

public static class GameObjectExtensions
{
    public static T GetFirstComponentInHierarchy<T>(this GameObject gameObject) where T : Component
    {
        Transform currentTransform = gameObject.transform;

        while (currentTransform != null)
        {
            T component = currentTransform.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            currentTransform = currentTransform.parent;
        }

        return null; // Return null if no component is found in the hierarchy
    }
    
    
}
