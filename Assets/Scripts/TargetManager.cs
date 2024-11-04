using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class TargetManager : Singleton<TargetManager>
{

    public List<Transform> targets;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Transform> GetTargets()
    {
        return targets;
    }
}
