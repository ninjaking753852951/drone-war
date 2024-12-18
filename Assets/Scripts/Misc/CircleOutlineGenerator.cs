using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircleOutlineGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private List<(Vector2 center, float radius)> circles = new List<(Vector2, float)>();
    [SerializeField]
    private int totalSegments = 100;
    [SerializeField]
    private float lineWidth = 0.1f;
    [SerializeField]
    private float moveSpeed = 1f;

    public float speedMultiplier = 1;

    private List<Vector2> outlinePoints = new List<Vector2>();
    private List<GameObject> lineObjects = new List<GameObject>();
    private List<float> segmentProgress = new List<float>();

    public SphereCollider mainWheel;
    
    public List<SphereCollider> wheels;

    public TankTrack tankTrack;

    void Start()
    {


        foreach (var wheel in wheels)
        {
            Vector2 wheelPos = new Vector2(wheel.transform.position.x, wheel.transform.position.z);
            float radius = wheel.radius * 2;
            circles.Add((wheelPos, radius));
        }

        GenerateUniformOutline(circles);
        SpawnOutlineSegments();
    }

    void Update()
    {
        UpdateSegmentPositionsAndRotations();
    }

    private void GenerateUniformOutline(List<(Vector2 center, float radius)> circles)
    {
        List<Vector2> boundaryPoints = new List<Vector2>();

        foreach (var circle in circles)
        {
            Vector2 center = circle.center;
            float radius = circle.radius;

            for (int i = 0; i < totalSegments; i++)
            {
                float angle = (i / (float)totalSegments) * Mathf.PI * 2;
                boundaryPoints.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        List<Vector2> convexHull = ComputeConvexHull(boundaryPoints);
        outlinePoints = GenerateUniformPoints(convexHull, totalSegments);
    }

    private List<Vector2> ComputeConvexHull(List<Vector2> points)
    {
        if (points.Count < 3)
            return points;

        List<Vector2> hull = new List<Vector2>();
        Vector2 startPoint = points.OrderBy(p => p.x).ThenBy(p => p.y).First();
        Vector2 currentPoint = startPoint;

        do
        {
            hull.Add(currentPoint);
            Vector2 nextPoint = points[0];

            foreach (var testPoint in points)
            {
                if (nextPoint == currentPoint || IsLeftTurn(currentPoint, nextPoint, testPoint))
                {
                    nextPoint = testPoint;
                }
            }

            currentPoint = nextPoint;
        } while (currentPoint != startPoint);

        return hull;
    }

    private bool IsLeftTurn(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0;
    }

    private List<Vector2> GenerateUniformPoints(List<Vector2> convexHull, int numSegments)
    {
        List<Vector2> uniformPoints = new List<Vector2>();
        float totalPerimeter = 0f;

        for (int i = 0; i < convexHull.Count; i++)
        {
            Vector2 start = convexHull[i];
            Vector2 end = convexHull[(i + 1) % convexHull.Count];
            totalPerimeter += Vector2.Distance(start, end);
        }

        float segmentLength = totalPerimeter / numSegments;
        float distanceAccumulator = 0f;

        for (int i = 0; i < convexHull.Count; i++)
        {
            Vector2 start = convexHull[i];
            Vector2 end = convexHull[(i + 1) % convexHull.Count];
            float segmentDistance = Vector2.Distance(start, end);

            while (distanceAccumulator + segmentLength <= segmentDistance)
            {
                distanceAccumulator += segmentLength;
                float t = distanceAccumulator / segmentDistance;
                uniformPoints.Add(Vector2.Lerp(start, end, t));
            }

            distanceAccumulator -= segmentDistance;
        }

        return uniformPoints;
    }

    private void SpawnOutlineSegments()
    {
        for (int i = 0; i < outlinePoints.Count; i++)
        {
            Vector2 start = outlinePoints[i];
            Vector2 end = outlinePoints[(i + 1) % outlinePoints.Count];

            SpawnLineSegment(start, end);
        }
    }

    private void SpawnLineSegment(Vector2 start, Vector2 end)
    {
        Vector3 startPos = new Vector3(start.x, 0, start.y);
        Vector3 endPos = new Vector3(end.x, 0, end.y);
        Vector3 midPoint = (startPos + endPos) / 2;
        Vector3 direction = (endPos - startPos).normalized;

        float length = Vector3.Distance(startPos, endPos);
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject line = Instantiate(linePrefab, midPoint, rotation);
        //line.transform.localScale = new Vector3(lineWidth, lineWidth, length);

        lineObjects.Add(line);
        segmentProgress.Add(0f);
    }

    private void UpdateSegmentPositionsAndRotations()
    {
        for (int i = 0; i < lineObjects.Count; i++)
        {
            segmentProgress[i] += Time.deltaTime * (tankTrack.linearSpeed * 100 * speedMultiplier) / (outlinePoints.Count - 1);

            if (segmentProgress[i] > 1f)
            {
                segmentProgress[i] -= 1f;
            }
            else if (segmentProgress[i] < 0f)
            {
                segmentProgress[i] += 1f;
            }

            int nextIndex = (i + 1) % outlinePoints.Count;
            int nextNextIndex = (nextIndex + 1) % outlinePoints.Count;
            Vector2 currentStart = outlinePoints[i];
            Vector2 currentEnd = outlinePoints[nextIndex];
            
            Vector2 nextStart = outlinePoints[nextNextIndex];

            Vector2 interpolatedPosition = Vector2.Lerp(currentStart, currentEnd, segmentProgress[i]);
            Vector3 position = new Vector3(interpolatedPosition.x, 0, interpolatedPosition.y);

            Vector3 startDirection = currentStart - currentEnd;
            Vector3 endDirection = currentEnd - nextStart;
            
            //Vector3 direction = new Vector3(currentEnd.x - currentStart.x, 0, currentEnd.y - currentStart.y).normalized;
            Vector2 rotDir = Vector2.Lerp(startDirection, endDirection, segmentProgress[i]);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(rotDir.x,0,rotDir.y));

            lineObjects[i].transform.position = position;
            lineObjects[i].transform.rotation = rotation;
        }
    }
}
