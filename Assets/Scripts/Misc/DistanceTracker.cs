using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceTracker : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Time duration to calculate the distance over (in seconds).")]
    public float timeWindow = 5f;

    [Tooltip("How often to sample the position (in seconds).")]
    public float sampleRate = 0.1f;

    private Queue<(Vector3 position, float time)> positionHistory = new Queue<(Vector3, float)>();
    public float totalDistance = 0f;
    private float nextSampleTime = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        // Initialize the last position to the object's starting position
        lastPosition = transform.position;
    }

    void Update()
    {
        float currentTime = Time.time;

        // Check if it's time to sample the position
        if (currentTime >= nextSampleTime)
        {
            Vector3 currentPosition = transform.position;

            // Check if the object has moved since the last sample
            float distance = Vector3.Distance(lastPosition, currentPosition);
            if (distance > 0.001f) // Threshold to ignore very small movements (floating-point precision)
            {
                // Add the current position and time to the history
                if (positionHistory.Count > 0)
                {
                    totalDistance += distance;
                }

                positionHistory.Enqueue((currentPosition, currentTime));
                lastPosition = currentPosition;
            }

            nextSampleTime = currentTime + sampleRate; // Schedule next sample
        }

        // Remove old positions (older than the time window)
        while (positionHistory.Count > 0 && currentTime - positionHistory.Peek().time > timeWindow)
        {
            // Adjust total distance by removing the contribution of the oldest segment
            if (positionHistory.Count > 1)
            {
                Vector3 oldestPosition = positionHistory.Dequeue().position;
                Vector3 nextPosition = positionHistory.Peek().position;
                totalDistance -= Vector3.Distance(oldestPosition, nextPosition);
            }
            else
            {
                positionHistory.Dequeue();
            }
        }
    }

    /// <summary>
    /// Resets the distance tracker.
    /// </summary>
    public void ResetTracker()
    {
        positionHistory.Clear();  // Clear all stored positions
        totalDistance = 0f;       // Reset the total distance
        nextSampleTime = Time.time + sampleRate; // Reset sampling time
        lastPosition = transform.position; // Reset the last position to the current position
    }
}