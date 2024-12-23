using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class MovingDroneBlockBase : MonoBehaviour
{

    [HideInInspector]
    public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public virtual void Deploy()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }
    
}
