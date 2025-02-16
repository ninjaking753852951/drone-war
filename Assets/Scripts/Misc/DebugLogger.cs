using UnityEngine;
using UnityUtils;

public class DebugLogger : Singleton<DebugLogger>
{

    public float priorityThreshold;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Log(string msg, float priority = 1)
    {
        if(priority > priorityThreshold)
            UnityEngine.Debug.Log(msg);
    }
}
