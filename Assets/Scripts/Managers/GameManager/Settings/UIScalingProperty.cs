using System;
using UnityEngine;

namespace Settings
{
    [Serializable]
    public class UIScalingProperty : SettingProperty<int>
    {

        
        public override void ApplySetting()
        {
            UIManager.Instance.SetUIScaling(value);
        }
    }
}
