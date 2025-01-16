using UnityEngine;
using System.Collections.Generic;
using BuildTools;
using UnityEngine.Serialization;

public class RotateGizmoController : BaseGizmoController
{

    public float rotationSpeed = 200f; // Degrees per second

    private Vector3 currentRotationAxis;
    private bool isDragging;
    private Plane dragPlane;
    private Vector3 lastHitPoint;
    private float currentAngle;      // For snapped rotation of objects
    private float gizmoAngle;       // For smooth gizmo visual feedback
    private Quaternion initialGizmoRotation;
    
    private Transform selectedAxis;
    
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
    }

    void StartDrag(Vector3 axis)
    {
        isDragging = true;
        currentRotationAxis = axis;
        currentAngle = 0f;
        gizmoAngle = 0f;
        initialGizmoRotation = transform.rotation;

        // Create a plane perpendicular to the rotation axis
        dragPlane = new Plane(currentRotationAxis, transform.position);

        // Get initial hit point
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
        ResetGizmoRotation();
    }

    void ResetGizmoRotation()
    {
        // Reset the gizmo to its initial rotation and clear accumulated angles
        transform.rotation = initialGizmoRotation;
        gizmoAngle = 0f;
        currentAngle = 0f;
    }

    void HandleDragging()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            
            // Calculate rotation angle based on the change in position relative to the rotation center
            Vector3 lastDir = (lastHitPoint - transform.position).normalized;
            Vector3 currentDir = (hitPoint - transform.position).normalized;
            
            float angle = Vector3.SignedAngle(lastDir, currentDir, currentRotationAxis);
            
            // Apply rotation speed modifier
            angle *= rotationSpeed * Time.deltaTime;
            
            // Handle smooth gizmo rotation
            gizmoAngle += angle;
            transform.rotation = initialGizmoRotation * Quaternion.AngleAxis(gizmoAngle, currentRotationAxis);
            
            // Handle snapped object rotation
            currentAngle += angle;
            float previousSnappedAngle = Mathf.Round((currentAngle - angle) / incrementSize) * incrementSize;
            float currentSnappedAngle = Mathf.Round(currentAngle / incrementSize) * incrementSize;
            
            // Only apply object rotation when crossing snap threshold
            if (Mathf.Abs(currentSnappedAngle - previousSnappedAngle) > 0.001f)
            {
                ApplyObjectRotation(currentRotationAxis, currentSnappedAngle - previousSnappedAngle);
            }
            
            lastHitPoint = hitPoint;
        }
    }

    void ApplyObjectRotation(Vector3 axis, float angle)
    {
        // Apply rotation only to selected objects, not the gizmo itself
        foreach (var selectedBlock in BuildingManager.Instance.blockSelector.selectedComponents)
        {
            selectedBlock.transform.RotateAround(transform.position, axis, angle);
        }
    }

    void HandleAxisHighlighting()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        ResetAxisColors();

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit rayHit in hits)
        {
            Transform hitTransform = rayHit.transform;
            if (xAxis.Contains(hitTransform))
            {
                SetAxisColor(xAxis, highlightMat);
            }
            if (yAxis.Contains(hitTransform))
            {
                SetAxisColor(yAxis, highlightMat);
            }
            if (zAxis.Contains(hitTransform))
            {
                SetAxisColor(zAxis, highlightMat);
            }
        }
    }
}