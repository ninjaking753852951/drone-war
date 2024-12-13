using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBobber : MonoBehaviour
{

    public Vector3 offset;
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = offset + Vector3.up * Mathf.Sin(Time.time);
    }
}
