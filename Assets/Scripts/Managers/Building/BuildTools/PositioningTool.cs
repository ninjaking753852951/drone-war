using System.Collections.Generic;
using UnityEngine;

namespace BuildTools
{
    public class PositioningTool : BaseTool
    {
        
        public Transform toolGizmo;
        public OperationPoints operationPoint;
        
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
                    break;
            }
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            toolGizmo.gameObject.SetActive(active);
        }
        
        Vector3 ComputeAveragePositionOfSelectedBlocks()
        {
            List<Transform> blockTransforms = Utils.GetTransformsFromComponents(builder.blockSelector.selectedComponents);
            return Utils.CalculateAveragePosition(blockTransforms);
        }
        
        void SetGizmoPosition(Vector3 pos)
        {
            toolGizmo.transform.position = pos;
        }
        
        public override void Init(BuildingManager builder)
        {
            base.Init(builder);
            builder.blockSelector.onSelected.AddListener(OnSelectedBlock);
        }
        
    }
}
