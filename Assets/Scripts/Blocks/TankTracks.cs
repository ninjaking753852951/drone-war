using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TankTrack : MonoBehaviour
{
    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private float segmentLength = 1f;
    public float speedMultiplier = 1;

    private List<Vector2> outlinePoints = new List<Vector2>();
    private List<GameObject> lineObjects = new List<GameObject>();
    private List<float> segmentProgress = new List<float>();

    public List<Vector3> wheelPositions = new List<Vector3>();
    public List<WheelData> wheels = new List<WheelData>();

    public DroneBlock droneBlock;
    private SphereCollider primaryWheel;
    public float linearSpeed;

    float xOffset;

    [System.Serializable]
    public class WheelData
    {
        public Rigidbody rb;
        public SphereCollider wheelCollider;

        public WheelData(Rigidbody rb, SphereCollider wheelCollider)
        {
            this.rb = rb;
            this.wheelCollider = wheelCollider;
        }
    }

    void Awake()
    {
        droneBlock = GetComponent<DroneBlock>();
    }

    void Start()
    {
        wheelPositions = droneBlock.meta.specialPositions;
        ScanForWheels();

        if (wheels == null || wheels.Count == 0)
            return;

        droneBlock.meta.specialPositions = wheelPositions;
        primaryWheel = wheels[0].wheelCollider;
        
        xOffset = wheels[0].wheelCollider.transform.position.x;

        GenerateOutlineFromWheels();
        SpawnOutlineSegments();
    }

    public void Deploy()
    {
        List<Collider> childColliders = GetComponentsInChildren<Collider>().ToList();
        foreach (var collider in childColliders)
        {
            Destroy(collider);
        }
    }

    void Update()
    {
        UpdateSegmentPositionsAndRotations();
    }

    void FixedUpdate()
    {
        if (wheels == null || wheels.Count == 0)
            return;

        Vector3 averageAngularVelocity = Vector3.zero;

        foreach (WheelData wheel in wheels)
        {
            if (wheel != null)
            {
                averageAngularVelocity += wheel.rb.angularVelocity;
            }
        }

        averageAngularVelocity /= wheels.Count;
        linearSpeed = averageAngularVelocity.x * primaryWheel.radius * 2;

        foreach (WheelData wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.rb.angularVelocity = averageAngularVelocity;
            }
        }
    }

    void ScanForWheels()
    {
        foreach (var wheelPosition in wheelPositions)
        {
            SphereCollider wheelCollider = Utils.FindSphereColliderAtPosition(wheelPosition, 3);
            Rigidbody rb = Utils.FindParentRigidbody(wheelCollider.transform);
            wheels.Add(new WheelData(rb, wheelCollider));
        }
    }

    void GenerateOutlineFromWheels()
    {
        List<(Vector2 center, float radius)> circles = new List<(Vector2, float)>();

        foreach (var wheel in wheels)
        {
            Vector2 wheelPos = new Vector2(wheel.wheelCollider.transform.position.z, wheel.wheelCollider.transform.position.y);
            float radius = wheel.wheelCollider.radius * 2;
            circles.Add((wheelPos, radius));
        }

        GenerateUniformOutline(circles);
    }

    void GenerateUniformOutline(List<(Vector2 center, float radius)> circles)
    {
        List<Vector2> boundaryPoints = new List<Vector2>();

        foreach (var circle in circles)
        {
            Vector2 center = circle.center;
            float radius = circle.radius;

            for (int i = 0; i < 100; i++) // Approximate each circle with 100 points
            {
                float angle = (i / 100f) * Mathf.PI * 2;
                boundaryPoints.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        List<Vector2> convexHull = ComputeConvexHull(boundaryPoints);
        outlinePoints = GenerateSegments(convexHull, segmentLength);
    }

    List<Vector2> ComputeConvexHull(List<Vector2> points)
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

    bool IsLeftTurn(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0;
    }

    List<Vector2> GenerateSegments(List<Vector2> convexHull, float segmentLength)
    {
        List<Vector2> uniformPoints = new List<Vector2>();
        float totalPerimeter = 0f;

        for (int i = 0; i < convexHull.Count; i++)
        {
            Vector2 start = convexHull[i];
            Vector2 end = convexHull[(i + 1) % convexHull.Count];
            totalPerimeter += Vector2.Distance(start, end);
        }

        int numSegments = Mathf.CeilToInt(totalPerimeter / segmentLength);
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

    void SpawnOutlineSegments()
    {
        for (int i = 0; i < outlinePoints.Count; i++)
        {
            Vector3 start = Vector2To3(outlinePoints[i]);
            Vector3 end = Vector2To3(outlinePoints[(i + 1) % outlinePoints.Count]);

            SpawnLineSegment(start, end);
        }
    }

    void SpawnLineSegment(Vector3 start, Vector3 end)
    {
        Vector3 startPos = start;
        Vector3 endPos = end;
        Vector3 midPoint = (startPos + endPos) / 2;
        Vector3 direction = (endPos - startPos).normalized;

        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject line = Instantiate(linePrefab, midPoint, rotation);
        line.transform.parent = transform;

        lineObjects.Add(line);
        segmentProgress.Add(0f);
    }

    void UpdateSegmentPositionsAndRotations()
    {
        for (int i = 0; i < lineObjects.Count; i++)
        {
            segmentProgress[i] += Time.deltaTime * (linearSpeed * speedMultiplier) / outlinePoints.Count;

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
            Vector3 currentStart = Vector2To3(outlinePoints[i]);
            Vector3 currentEnd = Vector2To3(outlinePoints[nextIndex]);
            Vector3 nextStart = Vector2To3(outlinePoints[nextNextIndex]);
            Vector3 interpolatedPosition = Vector3.Lerp(currentStart, currentEnd, segmentProgress[i]);

            interpolatedPosition.y -= 1.5f;
            
            Vector3 startDir = (currentEnd - currentStart);
            Vector3 endDir = nextStart - currentEnd;
            Quaternion rotation = Quaternion.LookRotation(Vector3.Lerp(startDir, endDir, segmentProgress[i]), Vector3.right);

            lineObjects[i].transform.localPosition = interpolatedPosition;
            lineObjects[i].transform.localRotation = rotation;
        }
    }

    Vector3 Vector2To3(Vector2 pos)
    {
        return new Vector3(xOffset, pos.y, pos.x);
    }
}