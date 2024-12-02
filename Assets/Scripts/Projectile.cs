using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public float damageMultiplier = 1;

    public int originTeam;
    public Transform originRoot;

    public Transform body;

    float drag;
    public bool isMissile;
    public float missileMultiplier = 1;

    float missileAcceleration;
    
    Rigidbody rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        body.rotation = Quaternion.LookRotation(rb.velocity);
    }

    void FixedUpdate()
    {
        //velocity *= (1 - turret.drag * Time.fixedDeltaTime);
        if (isMissile)
        {
            rb.AddForce(rb.velocity.normalized * ( missileAcceleration * Time.fixedDeltaTime * missileMultiplier ), ForceMode.VelocityChange);
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        Projectile otherProjectile = other.transform.root.GetComponent<Projectile>();
        if(otherProjectile!= null)
            return;
        
        DroneBlock droneBlock = other.gameObject.GetFirstComponentInHierarchy<DroneBlock>();

        if (droneBlock != null)
        {
            if(droneBlock.controller != null && droneBlock.controller.curTeam != originTeam)
                droneBlock.TakeDamage(Mathf.Pow(rb.velocity.magnitude, 2)* rb.mass *damageMultiplier);
        }
        
        // remove projectile after hitting anything except where it came from
        if(other.transform.root != originRoot)
            Destroy(gameObject);
    }

    public void Init(DroneController originController, float drag, float missileAcceleration = 0)
    {
        rb = GetComponent<Rigidbody>();
        this.drag = drag;
        this.missileAcceleration = missileAcceleration;

        if (!isMissile)
        {

            rb.drag = drag;
        }

        originRoot = originController.transform.root;
        originTeam = originController.curTeam;
    }
}
