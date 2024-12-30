using UnityEngine;
using UnityEngine.Serialization;
public interface IProgressBar
{
        public Transform ProgressBarWorldTarget();
        public float ProgressBarFill();

        public float ProgressBarMaximum();
        
        public ProgressBarSettings ProgressBarSettings();

        public bool IsDestroyed();
}
[System.Serializable]
public class ProgressBarSettings
{
        public float worldOffsetHeight = 5;
        public float screenOffsetHeight = 0;
        public Color colour = Color.white;
}
