using System;

namespace Settings
{
    [Serializable]
    public class VolumeSettingProperty : SettingProperty<float>
    {

        public VolumeSettingProperty()
        {
            value = 1;
        }
        public override void ApplySetting()
        {
            SFXManager.Instance.SetMasterVolume(value);
        }
    }
}
