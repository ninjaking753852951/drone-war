using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace BuildTools
{
    public class MoveGizmoController : BaseGizmoController
    {
        public float dragSensitivity = 0.01f;
        
        private Vector3 currentDragAxis;
        private bool isDragging;
        private Plane dragPlane;
        private Vector3 startDragPosition;
        private Vector3 lastHitPoint;
        
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

                deltaMove *= 0.5f; // Fix weird double travel
                
                // Project the movement delta onto the current axis
                float dragDistance = Vector3.Dot(deltaMove, currentDragAxis);
                
                // Calculate steps moved
                float steps = Mathf.Round(dragDistance / incrementSize);
                
                // Only move if we've accumulated enough movement for at least one step
                if (steps != 0)
                {
                    ApplyMovement(currentDragAxis * (steps * incrementSize));
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
}