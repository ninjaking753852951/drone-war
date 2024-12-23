using System;
using Unity.Netcode;
using UnityEngine;

public class NetVarTest : NetworkBehaviour
{

    public NetworkVariable<float> timeValue = new NetworkVariable<float>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeValue.Value += Time.deltaTime;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width/2, Screen.height/2, 100,100), "TIME " + timeValue.Value);
    }
}
