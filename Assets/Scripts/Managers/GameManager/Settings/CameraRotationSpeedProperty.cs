using System;
using UnityEngine;

namespace Settings
{
    [Serializable]
    public class CameraRotationSpeedProperty : SettingProperty<float>
    {

        public CameraRotationSpeedProperty()
        {
            value = 1;
        }
        
        public override void ApplySetting()
        {
            CameraController cameraController = GameObject.FindFirstObjectByType<CameraController>();
            
            if(cameraController == null)
                return;

            cameraController.curPanSpeed = cameraController.panSpeed * value;
        }
    }
}
