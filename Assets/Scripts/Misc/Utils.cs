using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
public static class Utils
{
    public static Rigidbody FindParentRigidbody(Transform startTransform, Rigidbody avoid = null)
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

    public static SphereCollider FindSphereColliderAtPosition(Vector3 position, float radius = 0.1f, LayerMask layerMask = default)
    {
        // Default to everything if no layer mask is provided
        if (layerMask == default)
        {
            layerMask = ~0; // Equivalent to "Everything"
        }

        // Get all colliders in the sphere
        Collider[] colliders = Physics.OverlapSphere(position, radius, layerMask);

        foreach (Collider collider in colliders)
        {
            SphereCollider sphere = collider.GetComponent<SphereCollider>();
            if (sphere != null && Vector3.Distance(sphere.bounds.center, position) < radius)
            {
                return sphere;
            }
        }

        return null; // No SphereCollider found
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
    
    /*public static void DestroyAllDrones()
    {
        List<DroneBlock> machineBlocks = GameObject.FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToList();
        foreach (DroneBlock machineBlock in machineBlocks)
        {
            GameObject.Destroy(machineBlock.gameObject);
        }
    }*/
    
    public static List<DroneController> DronesFromTeam(List<DroneController> drones, int teamID)
    {
        List<DroneController> dronesFromTeam = new List<DroneController>();

        foreach (DroneController drone in drones)
        {
            if(drone.curTeam == teamID)
                dronesFromTeam.Add(drone);
        }
        return dronesFromTeam;
    }
        
    public static void DestroyAllDrones()
    {
        HashSet<DroneBlock> machineBlocks = GameObject.FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToHashSet();
        foreach (DroneBlock machineBlock in machineBlocks)
        {
            if(machineBlock != null)
                if(machineBlock.gameObject != null)
                    GameObject.DestroyImmediate(machineBlock.gameObject);
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
    
    public static Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
    
    public static Texture2D MakeColorTransparent(Texture2D texture, Color colorToMakeTransparent, float tolerance = 0f)
    {
        // Create a copy of the texture to modify
        Texture2D newTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        newTexture.filterMode = texture.filterMode;

        // Get the pixels from the original texture
        Color[] pixels = texture.GetPixels();

        colorToMakeTransparent = pixels[0];

        // Iterate through all pixels
        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsColorMatch(pixels[i], colorToMakeTransparent, tolerance))
            {
                // Match found: Make the pixel transparent
                pixels[i] = new Color(pixels[i].r, pixels[i].g, pixels[i].b, 0f);
            }
        }

        // Set the modified pixels back to the new texture
        newTexture.SetPixels(pixels);
        newTexture.Apply();

        return newTexture;
    }
    
    private static bool IsColorMatch(Color color1, Color color2, float tolerance)
    {
        return Mathf.Abs(color1.r - color2.r) <= tolerance &&
               Mathf.Abs(color1.g - color2.g) <= tolerance &&
               Mathf.Abs(color1.b - color2.b) <= tolerance &&
               Mathf.Abs(color1.a - color2.a) <= tolerance;
    }
    
    public static void ClearRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, new Color(0, 0, 0, 0)); // Clear with transparent
        RenderTexture.active = null;
    }
    
    public static void RemoveNetworkComponents(GameObject gameObject)
    {
        // Remove NetworkObject component
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            Object.DestroyImmediate(networkObject);
        }

        // Remove all NetworkBehaviour components
        NetworkBehaviour[] networkBehaviours = gameObject.GetComponents<NetworkBehaviour>();
        for (int i = networkBehaviours.Length - 1; i >= 0; i--)
        {
            NetworkBehaviour behaviour = networkBehaviours[i];
            Object.DestroyImmediate(behaviour);
        }

        // Recursively clean up all child objects
        foreach (Transform child in gameObject.transform)
        {
            RemoveNetworkComponents(child.gameObject);
        }
    }

    public static Vector3 SnapToGrid(Vector3 position, Vector3 cellSize, Vector3? gridCenter = null, Quaternion? gridRotation = null)
    {
        // Use default values if optional parameters are not provided
        Vector3 center = gridCenter ?? Vector3.zero;
        Quaternion rotation = gridRotation ?? Quaternion.identity;

        // Transform the position to the grid's local space
        Vector3 localPosition = Quaternion.Inverse(rotation) * (position - center);

        // Snap to grid in local space
        localPosition.x = Mathf.Round(localPosition.x / cellSize.x) * cellSize.x;
        localPosition.y = Mathf.Round(localPosition.y / cellSize.y) * cellSize.y;
        localPosition.z = Mathf.Round(localPosition.z / cellSize.z) * cellSize.z;

        // Transform back to world space
        return rotation * localPosition + center;
    }
    
    public static void DestroyNetworkObjectWithChildren(NetworkObject parentNetworkObject)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the server can destroy networked objects!");
            return;
        }

        if (parentNetworkObject == null)
        {
            Debug.LogWarning("The provided NetworkObject is null.");
            return;
        }

        // Recursively despawn all children
        DespawnChildren(parentNetworkObject.transform);

        // Finally despawn the parent
        if (parentNetworkObject.IsSpawned)
        {
            parentNetworkObject.Despawn(true);
        }
    }

    private static void DespawnChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            NetworkObject childNetworkObject = child.GetComponent<NetworkObject>();
            if (childNetworkObject != null && childNetworkObject.IsSpawned)
            {
                // Recursively handle grandchildren
                DespawnChildren(child);

                // Despawn the child
                childNetworkObject.Despawn(true);
            }
            else
            {
                // Destroy non-NetworkObject children
                Object.Destroy(child.gameObject);
            }
        }
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

public static class TransformExtensions
{
    public static Transform FindChildWithTag(this Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child;

            // Recursively search in the child's hierarchy
            Transform found = child.FindChildWithTag(tag);
            if (found != null)
                return found;
        }

        return null; // No child with the tag was found
    }
}
