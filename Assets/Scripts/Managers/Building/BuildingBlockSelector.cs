using System;
using UnityEngine;
using UnityEngine.Events;
using BuildTools;

[RequireComponent(typeof(BuildingManager))]
public class BuildingBlockSelector : CursorSelectionController<DroneBlock>
{
    public float blockSelectionWidth;
    public Color selectionColour;
    BuildingManager builder;

    public DroneBlock lastSelected;

    public UnityEvent<DroneBlock> onSelected;
    
    void Awake()
    {
        builder = GetComponent<BuildingManager>();
    }

    protected override void HandleSelection(DroneBlock component)
    {
        lastSelected = component;
        Debug.Log("SELECTED " + component.name);
        Outline outline = component.gameObject.AddComponent<Outline>();
        if (outline != null)
        {
            outline.OutlineColor = selectionColour;
            outline.OutlineWidth = blockSelectionWidth;   
        }
        
        onSelected.Invoke(component);
    }
    protected override void HandleDeselection(DroneBlock component)
    {
        if(component == null)
            return;
        
        Debug.Log("DESELECTED " + component.name);

        Outline outline = component.gameObject.GetComponent<Outline>();
        if(outline != null)
            Destroy(outline);
    }

    protected override bool IsActive()
    {
        return(builder.curTool == BuildingManager.ToolMode.Move || builder.curTool == BuildingManager.ToolMode.Rotate) && GameManager.Instance.currentGameMode == GameMode.Build;
    }
    protected override bool IsSelectionException(Transform clickObject)
    {
        if (clickObject == null)
            return false;
        
        RotateGizmoController rotateGizmoController = clickObject.GetComponentInParent<RotateGizmoController>();

        
        MoveGizmoController gizmoController = clickObject.GetComponentInParent<MoveGizmoController>();
        return gizmoController != null || rotateGizmoController != null;
        
    }
}
