using System.Collections.Generic;
using UnityEngine;

namespace BuildTools
{
    public abstract class BaseGizmoController : MonoBehaviour
    {
        public List<Transform> xAxis;
        public List<Transform> yAxis;
        public List<Transform> zAxis;
        
        public float incrementSize = 1f;
        
        public Material xAxisMat;
        public Material yAxisMat;
        public Material zAxisMat;
        public Material highlightMat;

        protected Camera mainCamera;
        
        void Start()
        {
            mainCamera = Camera.main;
            ResetAxisColors();
        }
        
        protected void ResetAxisColors()
        {
            foreach (var axis in xAxis)
                SetAxisColor(axis, xAxisMat);
            foreach (var axis in yAxis)
                SetAxisColor(axis, yAxisMat);
            foreach (var axis in zAxis)
                SetAxisColor(axis, zAxisMat);
        }
        
        protected void SetAxisColor(Transform axis, Material mat)
        {
            var renderer = axis.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = mat;
            }
        }
        
        protected void SetAxisColor(List<Transform> axis, Material mat)
        {
            foreach (Transform axi in axis)
            {
                SetAxisColor(axi, mat);
            }
        }
    }
}
