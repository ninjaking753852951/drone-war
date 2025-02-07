using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorBlock : MonoBehaviour
{


    DroneController controller;
    
    public void Init()
    {
        controller = transform.root.GetComponentInChildren<DroneController>();

        DroneBlock block = GetComponent<DroneBlock>();
        
        controller.movementController.motorTorque += block.stats.QueryStat(Stat.Torque);
        controller.movementController.movementEnergyCost += block.stats.QueryStat(Stat.EnergyCost);
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
