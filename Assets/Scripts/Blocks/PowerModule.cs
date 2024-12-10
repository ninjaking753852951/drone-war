using UnityEngine;
public class PowerModule : MonoBehaviour
{

    public float powerCapacity = 0;
    public float powerRegen = 0;

    DroneController controller;
    
    public void Init()
    {
        controller = transform.root.GetComponent<DroneController>();

        controller.energy.maxEnergy += powerCapacity;
        controller.energy.energyRegenRate += powerRegen;
    }
}
