using System;
using UnityEngine;

namespace Settings
{
    [Serializable]
    public abstract class SettingProperty<T>
    {

        public T value;
        
        public void SetValue(T newValue)
        {
            //Debug.Log("Set value " + newValue);
            value = newValue;
            ApplySetting();
        }

        public abstract void ApplySetting();
    }
}
