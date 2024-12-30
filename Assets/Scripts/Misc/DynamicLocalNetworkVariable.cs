using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class DynamicLocalNetworkVariable<T>
{
    [SerializeField] private T initialValue; // Serialized for editor access
    private T value; // Local variable
    private NetworkVariable<T> netValue;

    public DynamicLocalNetworkVariable()
    {
        netValue = new NetworkVariable<T>(initialValue); // Initialize with initial value
    }

    public T GetValue()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            return netValue.Value;
        }
        else
        {
            return value;
        }
    }

    public void SetValue(T newValue)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                netValue.Value = newValue;
            }
            else
            {
                Debug.Log("Cannot set a NetworkVariable's value from a non-server instance.");
                //throw new System.InvalidOperationException("Cannot set a NetworkVariable's value from a non-server instance.");
            }
        }
        else
        {
            value = newValue;
        }
    }

    public void Initialize(NetworkVariable<T> networkVariable)
    {
        netValue = networkVariable;
    }
}
