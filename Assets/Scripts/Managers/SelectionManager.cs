using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class SelectionManager : Singleton<SelectionManager>
{
    public List<DroneController> selectedDrones = new List<DroneController>();

    public float selectionThreshold = 5; // Minimum drag distance to trigger rectangle selection
    public float clickScanRadius = 5; // Radius for single-click selection
    public LayerMask scanMask; // Layer mask for selectable objects

    private Vector2 startMousePosition;
    private Vector2 endMousePosition;
    private bool isDragging = false;

    void Start()
    {
        GameManager.Instance.onExitBuildMode.AddListener(ClearSelectedDrones);
        GameManager.Instance.onEnterBuildMode.AddListener(ClearSelectedDrones);
    }

    void Update()
    {
        if (GameManager.Instance.currentGameMode == GameMode.Build)
            return;

        HandleMouseInput();

        // Clean up null references in the selectedDrones list
        for (int i = selectedDrones.Count - 1; i >= 0; i--)
        {
            if (selectedDrones[i] == null)
                selectedDrones.RemoveAt(i);
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            startMousePosition = Input.mousePosition;
            isDragging = true;

            // Single-click selection (if not dragging)
            if (!Input.GetKey(KeyCode.LeftShift))
                ClearSelectedDrones();

            Transform clickObject = Utils.CursorScan(clickScanRadius, scanMask);
            if (clickObject != null)
            {
                DroneController drone = clickObject.root.GetComponentInChildren<DroneController>();
                if (drone != null)
                    SelectObject(clickObject);
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            isDragging = false;

            // Check if the drag distance exceeds the threshold
            if ((startMousePosition - endMousePosition).magnitude > selectionThreshold)
                SelectObjectsInRectangle();
        }

        if (isDragging)
        {
            endMousePosition = Input.mousePosition;
        }
    }

    void SelectObjectsInRectangle()
    {
        List<Transform> selectedTransforms = new List<Transform>();

        // Convert screen positions to viewport points
        Vector3 viewportStart = Camera.main.ScreenToViewportPoint(startMousePosition);
        Vector3 viewportEnd = Camera.main.ScreenToViewportPoint(endMousePosition);

        // Create a viewport rectangle
        Vector3 min = Vector3.Min(viewportStart, viewportEnd);
        Vector3 max = Vector3.Max(viewportStart, viewportEnd);
        Rect viewportRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        // Find all objects within the selection rectangle
        foreach (var obj in FindObjectsOfType<Transform>())
        {
            // Convert object world position to viewport position
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(obj.position);

            // Check if the object is within the viewport rectangle and in front of the camera
            if (viewportPosition.z > 0 && viewportRect.Contains(new Vector2(viewportPosition.x, viewportPosition.y)))
            {
                if(!obj.gameObject.CompareTag("IgnoreSelection"))
                    selectedTransforms.Add(obj);
            }
        }

        // Select the objects
        SelectObjects(selectedTransforms);
    }

    void SelectObjects(List<Transform> hits)
    {
        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            SelectObject(hit);
        }
    }

    void SelectObject(Transform hitTransform)
    {
        if (hitTransform != null)
        {
            // Check if the root object has a DroneController component
            DroneController drone = hitTransform.root.GetComponentInChildren<DroneController>();

            if (drone != null && !selectedDrones.Contains(drone))
            {
                drone.Select(true);
                selectedDrones.Add(drone);
            }
        }
    }

    void ClearSelectedDrones()
    {
        foreach (DroneController drone in selectedDrones)
        {
            if (drone == null)
                continue;

            drone.Select(false);
        }
        selectedDrones.Clear();
    }

    void OnGUI()
    {
        if (GameManager.Instance.currentGameMode == GameMode.Build)
            return;

        // Draw the selection rectangle
        if (isDragging)
        {
            var rect = GetScreenRect(startMousePosition, endMousePosition);
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 1f, 0.25f));
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 1f));
        }
    }

    // Helper method to get a screen rectangle
    Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
    {
        // Move from bottom-left to top-right
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        var topLeft = Vector2.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector2.Max(screenPosition1, screenPosition2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    // Helper method to draw a filled rectangle
    void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    // Helper method to draw a rectangle border
    void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color); // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color); // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color); // Left
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color); // Right
    }
}