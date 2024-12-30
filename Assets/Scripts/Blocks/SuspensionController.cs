using UnityEngine;

public class SuspensionController : MovingDroneBlockBase
{

    public ConfigurableJoint joint;

    public override void Deploy()
    {
        base.Deploy();
        joint.connectedBody = Utils.FindParentRigidbody(transform.parent, rb);
    }

}
