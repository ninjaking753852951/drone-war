using UnityEngine;
using System.Collections.Generic;

public class MoveGizmoController : MonoBehaviour
{
    public List<Transform> xAxis;
    public List<Transform> yAxis;
    public List<Transform> zAxis;
    
    public float stepSize = 1f;
    public float dragSensitivity = 0.01f;
    
    private Vector3 currentDragAxis;
    private bool isDragging;
    private Plane dragPlane;
    private Vector3 startDragPosition;
    private Vector3 lastHitPoint;
    
    private Camera mainCamera;
    private Transform selectedAxis;
    private Color xAxisColor = new Color(1f, 0.2f, 0.2f);
    private Color yAxisColor = new Color(0.2f, 1f, 0.2f);
    private Color zAxisColor = new Color(0.2f, 0.2f, 1f);
    private Color highlightColor = new Color(1f, 1f, 0f);

    

    void Start()
    {
        mainCamera = Camera.main;
        SetupAxisColors();
    }

    void SetupAxisColors()
    {
        foreach (var axis in xAxis)
            SetAxisColor(axis, xAxisColor);
        foreach (var axis in yAxis)
            SetAxisColor(axis, yAxisColor);
        foreach (var axis in zAxis)
            SetAxisColor(axis, zAxisColor);
    }

    void SetAxisColor(Transform axis, Color color)
    {
        var renderer = axis.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    void Update()
    {
        HandleMouseInput();
        
        if (isDragging)
        {
            HandleDragging();
        }
        else
        {
            HandleAxisHighlighting();
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDrag();
        }
    }

    void TryStartDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            selectedAxis = hit.transform;
            
            if (xAxis.Contains(selectedAxis))
            {
                StartDrag(Vector3.right);
                return;
            }
            else if (yAxis.Contains(selectedAxis))
            {
                StartDrag(Vector3.up);
                return;
            }
            else if (zAxis.Contains(selectedAxis))
            {
                StartDrag(Vector3.forward);
                return;
            }
        }
        
        /*if (Physics.Raycast(ray, out hit))
        {
            selectedAxis = hit.transform;
            
            if (xAxis.Contains(selectedAxis))
            {
                StartDrag(Vector3.right);
            }
            else if (yAxis.Contains(selectedAxis))
            {
                StartDrag(Vector3.up);
            }
            else if (zAxis.Contains(selectedAxis))
            {
                StartDrag(Vector3.forward);
            }
        }*/
    }

    void StartDrag(Vector3 axis)
    {
        isDragging = true;
        currentDragAxis = axis;
        startDragPosition = transform.position;

        // Create a plane perpendicular to the camera that contains the drag axis
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 planeNormal = Vector3.Cross(axis, Vector3.Cross(cameraForward, axis)).normalized;
        dragPlane = new Plane(planeNormal, transform.position);

        // Get initial hit point for relative movement
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (dragPlane.Raycast(ray, out distance))
        {
            lastHitPoint = ray.GetPoint(distance);
        }
    }

    void StopDrag()
    {
        isDragging = false;
        selectedAxis = null;
    }

    void HandleDragging()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 deltaMove = hitPoint - lastHitPoint;
            
            // Project the movement delta onto the current axis
            float dragDistance = Vector3.Dot(deltaMove, currentDragAxis);
            
            // Calculate steps moved
            float steps = Mathf.Round(dragDistance / stepSize);
            
            // Only move if we've accumulated enough movement for at least one step
            if (steps != 0)
            {
                ApplyMovement(currentDragAxis * (steps * stepSize));
                // Calculate new position based on step increments
                //Vector3 newPosition = transform.position + currentDragAxis * (steps * stepSize);
                //transform.position = newPosition;
                
                // Update last hit point to prevent accumulated sub-step movement
                lastHitPoint = hitPoint;
            }
        }
    }

    void ApplyMovement(Vector3 moveDelta)
    {
        transform.position += moveDelta;

        foreach (var selectedBlock in BuildingManager.Instance.blockSelector.selectedComponents)
        {
            selectedBlock.transform.position += moveDelta;
        }
    }

    void HandleAxisHighlighting()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Reset all axis colors
        foreach (var axis in xAxis)
            SetAxisColor(axis, xAxisColor);
        foreach (var axis in yAxis)
            SetAxisColor(axis, yAxisColor);
        foreach (var axis in zAxis)
            SetAxisColor(axis, zAxisColor);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit rayHit in hits)
        {
            Transform hitTransform = rayHit.transform;
            if (xAxis.Contains(hitTransform) || yAxis.Contains(hitTransform) || zAxis.Contains(hitTransform))
            {
                SetAxisColor(hitTransform, highlightColor);
            }
        }
        
        /*// Highlight hovered axis
        if (Physics.Raycast(ray, out hit))
        {
            Transform hitTransform = hit.transform;
            if (xAxis.Contains(hitTransform) || yAxis.Contains(hitTransform) || zAxis.Contains(hitTransform))
            {
                SetAxisColor(hitTransform, highlightColor);
            }
        }*/
    }

}