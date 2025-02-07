using System;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(MapObjectivePoint))]
public class MapObjectiveNetworkHelper : NetworkHelperBase
{
    MapObjectivePoint objective;

    public NetworkVariable<int> currentOwner = new NetworkVariable<int>();
    public NetworkVariable<float> fill = new NetworkVariable<float>();


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!NetworkManager.Singleton.IsServer)
            LocalInit();
    }
    
    void Awake()
    {
        objective = GetComponent<MapObjectivePoint>();
    }

    void Start()
    {
        currentOwner.OnValueChanged += UpdateCurrentOwner;
        fill.OnValueChanged += UpdateFill;
        //NetworkManager.Singleton.OnClientStarted += DisableLocalLogic;
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (objective.currentOwner.HasValue)
            {
                currentOwner.Value = objective.currentOwner.Value;
            }
            else
            {
                currentOwner.Value = -1;
            }

            fill.Value = objective.healthFill;
        }
    }

    void LocalInit()
    {
        objective.SetIndicatorSize();
        objective.enabled = false;
        //Debug.Log("FILL IS " + fill.Value);
        objective.SetFill(fill.Value);
        objective.SetIndicatorColour(currentOwner.Value);
    }

    void UpdateFill(float previousValue, float newValue)
    {
        objective.SetFill(newValue);
    }

    void UpdateCurrentOwner(int previousValue, int newValue)
    {
        objective.SetIndicatorColour(newValue);
    }


}
