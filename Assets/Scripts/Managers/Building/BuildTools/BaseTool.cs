using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildTools
{
    [System.Serializable]
    public abstract class BaseTool
    {
        public Transform toolGizmo;
        public OperationPoints operationPoint;
        BuildingManager builder;

        public BuildingManagerUI.BuildToolUI ui;
        
        public void Update()
        {
            if (builder.blockSelector.selectedComponents.Count > 0)
            {
                toolGizmo.gameObject.SetActive(true);
            }
            else
            {
                toolGizmo.gameObject.SetActive(false);
            }
        }
        
        void OnSelectedBlock(DroneBlock block)
        {
            switch (operationPoint)
            {
                case OperationPoints.Center:
                    SetGizmoPosition(ComputeAveragePositionOfSelectedBlocks());
                    break;
                case OperationPoints.Pivot:
                    SetGizmoPosition(builder.blockSelector.lastSelected.transform.position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        void SetGizmoPosition(Vector3 pos)
        {
            toolGizmo.transform.position = pos;
        }
        
        Vector3 ComputeAveragePositionOfSelectedBlocks()
        {
            List<Transform> blockTransforms = Utils.GetTransformsFromComponents(builder.blockSelector.selectedComponents);
            return Utils.CalculateAveragePosition(blockTransforms);
        }
        
        public void Init(BuildingManager builder)
        {
            this.builder = builder;
            builder.blockSelector.onSelected.AddListener(OnSelectedBlock);
        }

        public void SetActive(bool active)
        {
            if(active)
                builder.DisableAllBuildTools();
            
            toolGizmo.gameObject.SetActive(active);
            if(active)
                ui.SetOpen();
            else
                ui.SetClosed();
        }

        public void SetIncrementSize(float value)
        {
            
        }
    }
}
