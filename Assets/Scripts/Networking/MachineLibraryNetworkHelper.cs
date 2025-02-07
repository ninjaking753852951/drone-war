using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(MachineLibraryManager))]
public class MachineLibraryNetworkHelper : NetworkBehaviour
{

    MachineLibraryManager library;

    void Awake()
    {
        library = GetComponent<MachineLibraryManager>();
    }

    void Start()
    {

    }
    
    [Rpc(SendTo.Server, RequireOwnership = false)]
    void SendMachineDataRpc(byte[] jsonBytes, ulong clientId, int machineID)
    {


        // Deserialize JSON on the server
        string json = Encoding.UTF8.GetString(jsonBytes);
        MachineSaveData machineData = MachineSaveData.DeserializeFromJson(json);

        AddMachine(machineData, NetworkUtils.NetworkMachineID(clientId, machineID));
    }

    
    void SendMachineData(MachineSaveData machineData, int id)
    {
        // Serialize to JSON and convert to byte array
        string json = machineData.SerializeToJson();
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        ///Debug.Log("SENDING MACHINE TO SERVER" );
        
        if (!IsSpawned)
        {
            Debug.LogError("NetworkObject is not spawned! RPCs will not work.");
        }
        
        // Send the data to the server
        SendMachineDataRpc(jsonBytes, NetworkManager.Singleton.LocalClientId, id);
    }


    void SendAllMachineData()
    {
        if(!GameManager.Instance.IsOnlineAndClient())
            return;
        
        for (int i = 0; i < library.loadedMachines.Count; i++)
        {
            MachineSaveData machine = library.loadedMachines[i];
            if(machine == null)
                continue;
            SendMachineData(machine, i);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); // Optional: Call the base implementation

        if (GameManager.Instance.IsOnlineAndClient())
        {
            SendAllMachineData();
        }

    }

    
    void AddMachine(MachineSaveData machine, int id)
    {
        library.loadedMachines.Add(id,machine);
    }
}