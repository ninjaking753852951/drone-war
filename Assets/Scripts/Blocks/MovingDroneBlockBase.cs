using UnityEngine;


public abstract class MovingDroneBlockBase : MonoBehaviour
{

    PhysBlock physBlock;

    void Awake()
    {
        physBlock = GetComponent<PhysBlock>();
        physBlock.onBuildFinalized.AddListener(Deploy);
    }

    public virtual void Deploy()
    {
        
    }
    
}
