using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretRangeIndicator : MonoBehaviour
{
    public LineRenderer lineRend;

    // the number of vertices in the indicator
    public float resolution = 15;

    float range;
    
    // Start is called before the first frame update
    void Start()
    {
        lineRend.gameObject.SetActive(true);
        Select(false);
    }

    // Update is called once per frame
    void Update()
    {
        DrawRange(range);
    }

    public void SetRange(float range) => this.range = range;

    // using the line rend draw a circle with radius range around the transform
    void DrawRange(float range)
    {
        // Ensure the LineRenderer is set up
        if (lineRend == null) return;

        // Calculate the number of vertices for the circle based on resolution
        int segments = Mathf.CeilToInt(resolution);
        lineRend.positionCount = segments + 1; // +1 to close the circle

        // Angle step for each segment
        float angleStep = 360f / segments;

        // Generate the circle points
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep); // Convert angle to radians
            float x = Mathf.Cos(angle) * range;
            float z = Mathf.Sin(angle) * range;

            // Set the position of the current vertex
            Vector3 newPosition = new Vector3(x, 0, z) + lineRend.transform.position;
            newPosition.y = 0;
            
            lineRend.SetPosition(i, newPosition);
        }
    }

    void UpdateRange()
    {
        
    }

    public void Select(bool selected)
    {
        lineRend.enabled = selected;
    }

}
