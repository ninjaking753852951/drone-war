using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneController : MonoBehaviour
{

    public Transform targetDestination;
    
    
    public float currentSteer; // ranges from -1 to 1
    public float motorTorque; // range will be defined in the slider

    public float steerMultiplier;
    public float topSpeed;

    public float targetvelocityDebug;
    
    public float brakingDistance;
    public AnimationCurve wheelBrakingCurve;

    public Outline outline;
    public float selectionWidth = 2;
    
    List<WheelController> wheelControllers = new List<WheelController>();
    List<HingeController> hingeControllers = new List<HingeController>();
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Deploy( false);
    }

    void Update()
    {
        if(targetDestination == null)
            return;

        currentSteer = CalculateSteerAmount(targetDestination.position);
        
        // Update wheels whenever values change, you may want to call this in response to actual changes
        UpdateComponents();
    }

    public void Deploy(bool deploy)
    {
        rb.isKinematic = !deploy;
        rb.useGravity = deploy;



        if (deploy)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineWidth = selectionWidth;
            outline.enabled = false;
            
            wheelControllers = GetComponentsInChildren<WheelController>().ToList();
            hingeControllers = GetComponentsInChildren<HingeController>().ToList();
        }
    }

    void UpdateComponents()
    {
        foreach (WheelController wheelController in wheelControllers)
        {
            wheelController.SetForwardTorque(motorTorque/wheelControllers.Count);
        }
        
        foreach (WheelController wheelController in wheelControllers)
        {
            wheelController.SetTargetVelocity(topSpeed * wheelBrakingCurve.Evaluate(Vector3.Distance(targetDestination.position, transform.position)/brakingDistance));

            targetvelocityDebug = topSpeed * wheelBrakingCurve.Evaluate(Vector3.Distance(targetDestination.position, transform.position) / brakingDistance);
        }

        foreach (HingeController hingeController in hingeControllers)
        {
            hingeController.SetSteerRot(currentSteer);
        }
    }

    float CalculateSteerAmount(Vector3 targetPos)
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward; // The drone's forward direction
        Vector3 directionToTarget = (targetPos - origin).normalized; // Direction to the target, normalized

        // Calculate the angle between the drone's forward direction and the direction to the target
        float angle = Vector3.SignedAngle(forward, directionToTarget, Vector3.up); 

        // Convert the angle to a steer value between -1 (hard left) and 1 (hard right)
        float steerAmount = Mathf.Clamp(angle / 90f, -1f, 1f);
        
        return angle * steerMultiplier;
    }

    /*
    void OnGUI()
    {
        // Set the position and size of the sliders
        GUI.Label(new Rect(10, 10, 150, 20), "Motor Torque");
        motorTorque = GUI.HorizontalSlider(new Rect(10, 30, 150, 20), motorTorque, -100f, 100f);

        GUI.Label(new Rect(10, 50, 150, 20), "Steering");
        currentSteer = GUI.HorizontalSlider(new Rect(10, 70, 150, 20), currentSteer, -1f, 1f);

        // Call UpdateWheels to apply the changes
        UpdateWheels();
    }
    */
}
