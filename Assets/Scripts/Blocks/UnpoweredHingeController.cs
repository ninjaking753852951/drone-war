using UnityEngine;

public class UnpoweredHingeController : MovingDroneBlockBase
{

    public HingeJoint joint;
    public Transform body;

    public override void Deploy()
    {
        base.Deploy();
        joint.connectedBody = Utils.FindParentRigidbody(transform.parent, rb);
        body.transform.parent = transform.parent;
    }

}
