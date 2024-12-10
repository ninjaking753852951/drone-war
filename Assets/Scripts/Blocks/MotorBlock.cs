using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorBlock : MonoBehaviour
{

    public float torque;
    public float energyCost;

    DroneController controller;
    
    public void Init()
    {
        controller = transform.root.GetComponent<DroneController>();
        
        controller.movementController.motorTorque += torque;
        controller.movementController.movementEnergyCost += energyCost;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
