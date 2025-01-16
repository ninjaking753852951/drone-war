using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct Cuboid
{
    public Vector3 position;
    public Vector3 size;
    public Quaternion rotation;

    private Vector3[] axes;
    private Vector3[] corners;

    public Cuboid(Vector3 pos, Vector3 sz, Vector3 eulerAngles)
    {
        position = pos;
        size = sz;
        rotation = Quaternion.Euler(eulerAngles);
        axes = null;    // Lazy initialization
        corners = null; // Lazy initialization
    }

    public bool Intersects(Cuboid other)
    {
        // Get axes we need to test (15 in total for OBB-OBB test)
        List<Vector3> axes = GetSATAxes(other);

        // Test projection of both cuboids onto each axis
        foreach (Vector3 axis in axes)
        {
            float minA, maxA, minB, maxB;
            ProjectOntoAxis(this, axis, out minA, out maxA);
            ProjectOntoAxis(other, axis, out minB, out maxB);

            // If we find a separating axis, the cuboids don't intersect
            if (!OverlapOnAxis(minA, maxA, minB, maxB))
            {
                return false;
            }
        }

        // No separating axis found, the cuboids must intersect
        return true;
    }

    public void DrawWithGizmos(Color color)
    {
        Vector3[] corners = GetCorners();
        Gizmos.color = color;

        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[0], corners[2]);
        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[3]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[3], corners[2]);
        Gizmos.DrawLine(corners[3], corners[7]);
        Gizmos.DrawLine(corners[7], corners[5]);
        Gizmos.DrawLine(corners[7], corners[6]);
        Gizmos.DrawLine(corners[4], corners[6]);
        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        
        /*// Draw edges of the cuboid
        for (int i = 0; i < 4; i++)
        {
            // Connect bottom face corners
            Gizmos.DrawLine(corners[i], corners[((i + 3) % 4) +4]);

            // Connect top face corners
            //Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]);

            // Connect vertical edges
            Gizmos.DrawLine(corners[i], corners[i + 4]);
        }*/
    }
    
    private List<Vector3> GetSATAxes(Cuboid other)
    {
        List<Vector3> axes = new List<Vector3>();

        // Get face normals of both cuboids
        Vector3[] aAxes = GetLocalAxes();
        Vector3[] bAxes = other.GetLocalAxes();

        // Add face normals
        axes.AddRange(aAxes);
        axes.AddRange(bAxes);

        // Add cross products of all combinations of edges
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 cross = Vector3.Cross(aAxes[i], bAxes[j]);
                if (cross.sqrMagnitude > 1e-8f) // Improved precision check
                {
                    axes.Add(cross.normalized);
                }
            }
        }

        return axes;
    }

    private Vector3[] GetLocalAxes()
    {
        if (axes == null)
        {
            axes = new Vector3[3];
            axes[0] = (rotation * Vector3.right).normalized;
            axes[1] = (rotation * Vector3.up).normalized;
            axes[2] = (rotation * Vector3.forward).normalized;
        }
        return axes;
    }

    private static void ProjectOntoAxis(Cuboid cuboid, Vector3 axis, out float min, out float max)
    {
        Vector3[] corners = cuboid.GetCorners();

        min = float.MaxValue;
        max = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            float projection = Vector3.Dot(corner, axis);
            min = Mathf.Min(min, projection);
            max = Mathf.Max(max, projection);
        }
    }

    private static bool OverlapOnAxis(float minA, float maxA, float minB, float maxB)
    {
        const float EPSILON = 1e-6f; // Small value to handle floating-point precision
        return (maxA + EPSILON) >= minB && (maxB + EPSILON) >= minA;
    }

    public Vector3[] GetCorners()
    {
        if (corners == null)
        {
            corners = new Vector3[8];
            Vector3 halfSize = size * 0.5f;

            // Define local corners relative to the center
            Vector3[] localCorners = new Vector3[8]
            {
                new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                new Vector3(-halfSize.x, halfSize.y, halfSize.z),
                new Vector3(halfSize.x, halfSize.y, halfSize.z),
            };

            // Transform local corners to world space
            for (int i = 0; i < 8; i++)
            {
                corners[i] = position + rotation * localCorners[i];
            }
        }
        return corners;
    }
}
