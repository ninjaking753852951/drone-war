using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class WorldPositioner : MonoBehaviour
{
    
    public float worldPositionY = -1;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        position.y = worldPositionY;
        transform.position = position;
    }
}
