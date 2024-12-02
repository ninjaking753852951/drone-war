using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleExperiments : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var circles = new List<(Vector3, float)>
        {
            (new Vector3(0, 0, 0), 1f),
            (new Vector3(2, 0, 0), 1f),
            (new Vector3(4, 0, 0), 1f)
        };

        List<Vector3> packedPositions = PackCirclesTightly(circles);

        foreach (var pos in packedPositions)
        {
            Debug.Log(pos);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public static List<Vector3> PackCirclesTightly(List<(Vector3 position, float radius)> circles)
    {
        // Max iterations to avoid infinite loops
        const int maxIterations = 100;
        const float epsilon = 0.01f; // Threshold for movement to consider settled

        List<Vector3> positions = new List<Vector3>();
        foreach (var circle in circles)
        {
            positions.Add(circle.position);
        }

        bool moved;
        int iteration = 0;

        do
        {
            moved = false;

            // Compare every circle against every other circle
            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = i + 1; j < circles.Count; j++)
                {
                    Vector3 posA = positions[i];
                    Vector3 posB = positions[j];
                    float radiusA = circles[i].radius;
                    float radiusB = circles[j].radius;

                    Vector3 direction = posB - posA;
                    float distance = direction.magnitude;
                    float minDistance = radiusA + radiusB;

                    // If overlapping, push circles apart
                    if (distance < minDistance)
                    {
                        Vector3 pushDirection = direction.normalized;
                        float overlap = minDistance - distance;
                        Vector3 push = pushDirection * (overlap / 2f);

                        positions[i] -= push;
                        positions[j] += push;

                        if (push.magnitude > epsilon)
                        {
                            moved = true;
                        }
                    }
                }
            }

            iteration++;
        }
        while (moved && iteration < maxIterations);

        return positions;
    }

}
