using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CursorSelectionController<T> : MonoBehaviour where T : Component
{
    public List<Transform> selectedTransforms = new List<Transform>();
    public List<T> selectedComponents = new List<T>();
    
    public float selectionThreshold = 5f;
    
    private Vector2 startMousePosition;
    private Vector2 endMousePosition;
    private bool isDragging = false;

    protected abstract bool IsActive();
    
    protected virtual void Update()
    {
        if(!IsActive())
            return;
        
        CleanupNullReferences();
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit rayHit in hits)
            {
                if(IsSelectionException(rayHit.transform))
                    return;
            }
            
            Transform clickObject = Utils.CursorScan();

            /*if(IsSelectionException(clickObject))
                return;*/
            
            if (clickObject != null)
            {
                SelectObject(clickObject);
            }

            if (clickObject == null)
            {
                ClearSelectedObjects();
                return;
            }
            
            T component = clickObject.GetComponent<T>();
            if (component == null)
                ClearSelectedObjects();
        }
        
        HandleMouseInput();
    }

    protected virtual bool IsSelectionException(Transform clickObject)
    {
        return false;
    }

    protected virtual void CleanupNullReferences()
    {
        for (int i = selectedTransforms.Count - 1; i >= 0; i--)
        {
            if(selectedTransforms[i] == null)
            {
                if (i < selectedComponents.Count)
                {
                    HandleDeselection(selectedComponents[i]);
                    selectedComponents.RemoveAt(i);
                }
                selectedTransforms.RemoveAt(i);
            }
        }
    }

    protected virtual void SelectObjects(List<Transform> hits)
    {
        ClearSelectedObjects();
        foreach (var hit in hits)
        {
            if(hit != null && hit.transform != null)
            {
                SelectObject(hit);
            }
        }
    }
    
    // Abstract methods that must be implemented by child classes
    protected abstract void HandleSelection(T component);
    protected abstract void HandleDeselection(T component);

    protected virtual void SelectObject(Transform hitTransform)
    {
        if (hitTransform == null) return;

        // Check if the object has the required component
        T component = hitTransform.GetComponent<T>();
        if (component == null) return;

        if (!selectedTransforms.Contains(hitTransform))
        {
            selectedTransforms.Add(hitTransform);
            selectedComponents.Add(component);
            HandleSelection(component);
        }
    }

    protected virtual void ClearSelectedObjects()
    {
        // Invoke deselected event for all components
        foreach (var component in selectedComponents)
        {
            if (component != null)
            {
                HandleDeselection(component);
            }
        }

        selectedComponents.Clear();
        selectedTransforms.Clear();
    }

    protected virtual void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if ((startMousePosition - endMousePosition).magnitude > selectionThreshold)
            {
                SelectObjectsInRectangle();
            }
        }

        if (isDragging)
        {
            endMousePosition = Input.mousePosition;
        }
    }

    protected virtual void SelectObjectsInRectangle()
    {
        var objectsToSelect = new List<Transform>();

        Vector3 viewportStart = Camera.main.ScreenToViewportPoint(startMousePosition);
        Vector3 viewportEnd = Camera.main.ScreenToViewportPoint(endMousePosition);

        Vector3 min = Vector3.Min(viewportStart, viewportEnd);
        Vector3 max = Vector3.Max(viewportStart, viewportEnd);
        Rect viewportRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        // Only find objects with the required component type
        foreach (var component in FindObjectsOfType<T>())
        {
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(component.transform.position);

            if (viewportPosition.z > 0 && viewportRect.Contains(new Vector2(viewportPosition.x, viewportPosition.y)))
            {
                objectsToSelect.Add(component.transform);
            }
        }
        
        SelectObjects(objectsToSelect);
    }

    protected virtual void OnGUI()
    {
        if(!IsActive())
            return;
        
        if (isDragging)
        {
            var rect = GetScreenRect(startMousePosition, endMousePosition);
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 1f, 0.25f));
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 1f));
        }
    }

    protected Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
    {
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        var topLeft = Vector2.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector2.Max(screenPosition1, screenPosition2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    protected void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    protected void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }
}