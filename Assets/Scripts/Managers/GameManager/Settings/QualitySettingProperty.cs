using System;
using UnityEngine;

namespace Settings
{
    [Serializable]
    public class QualitySettingProperty : SettingProperty<int>
    {

        public QualitySettingProperty()
        {
            value = 2;
        }
        public override void ApplySetting()
        {
            QualitySettings.SetQualityLevel(value);
        }
    }
}
