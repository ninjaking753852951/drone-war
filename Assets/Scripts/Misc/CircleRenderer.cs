using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleRenderer : MonoBehaviour
{
    [SerializeField] private int resolution = 100; // Number of points in the circle
    [SerializeField] private float radius = 1f; // Radius of the circle

    private LineRenderer lineRenderer;

    MapObjectivePoint objective;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = false; // Not a closed loop since the fill might not be 100%
        lineRenderer.useWorldSpace = false; // Local space drawing

        objective = GetComponentInParent<MapObjectivePoint>();
        if (objective != null)
            radius = objective.radius;
    }

    void Start()
    {
        DrawCircle(1f); // Default to a full circle
    }
    
    /*public void DrawCircle(float fill)
    {
        fill = Mathf.Clamp01(fill); // Ensure fill is between 0 and 1

        int pointCount = Mathf.CeilToInt(resolution * fill); // Adjust the number of points based on fill
        Vector3[] points = new Vector3[pointCount + 1]; // +1 to close the gap at the end

        for (int i = 0; i <= pointCount; i++)
        {
            float angle = (2 * Mathf.PI * i * fill) / resolution; // Calculate angle for each point
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, y, 0f);
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }*/
    
    public void DrawCircle(float fill)
    {
        fill = Mathf.Clamp01(fill); // Ensure fill is between 0 and 1
        
        Vector3[] circlePoints = new Vector3[resolution +1]; // +1 to close the gap at the end
        
        for (int i = 0; i <= resolution; i++)
        {
            float angle = (2 * Mathf.PI * i) / resolution; // Calculate angle for each point
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            circlePoints[i] = new Vector3(x, y, 0f);
        }
        

        int pointCount = Mathf.FloorToInt(resolution * fill); // Adjust the number of points based on fill
        Vector3[] points = new Vector3[pointCount + 1]; // +1 to close the gap at the end

        if (pointCount > 1)
        {
            for (int i = 0; i <= pointCount; i++)
            {
                if (i == pointCount)
                {
                    float lerpAmount = (resolution * fill) % 1;
                    Debug.Log(i);
                    points[i] = Vector3.Lerp(circlePoints[i], circlePoints[(i+1) % resolution], lerpAmount);
                }
                else
                {
                    points[i] = circlePoints[i];     
                }
            }   
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
    
    public void SetResolution(int newResolution)
    {
        resolution = Mathf.Max(3, newResolution); // Minimum resolution of 3 to make a shape 
    }
    
    public void SetRadius(float newRadius)
    {
        radius = Mathf.Max(0.01f, newRadius); // Prevent a radius of 0
    }
    
    public void SetFill(float fill)
    {
        DrawCircle(fill); // Redraw the circle with the new radius
    }
}
