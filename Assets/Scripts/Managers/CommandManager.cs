using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityUtils;
public class CommandManager : NetworkBehaviour
{
    
    List<Command> commands = new List<Command>();

    public class Command
    {
        public enum Type
        {
            Waypoint, SpawnMachine
        }

        public Type type = Type.Waypoint;
        public Vector3 waypointPosition = Vector3.zero;
        public ulong machineID = 0;
        public int spawnSlot = -1;
        public ulong source = 0;

        public Command(ulong source,ulong machineID, Vector3 waypointPosition)
        {
            this.source = source;
            this.machineID = machineID;
            this.waypointPosition = waypointPosition;
            type = Type.Waypoint;
        }
        
        public Command(ulong source, int spawnSlot)
        {
            this.source = source;
            this.spawnSlot = spawnSlot;
            type = Type.SpawnMachine;
        }

        public Command(Data data)
        {
            this.source = data.source;
            this.type = data.type;
            this.waypointPosition = data.waypointPosition;
            this.machineID = data.machineID;
            this.spawnSlot = data.spawnSlot;
            this.source = data.source;
        }

        public Data GenerateData()
        {
            return new Data(type, waypointPosition, machineID, spawnSlot, source);
        }
        
        public struct Data : INetworkSerializable
        {
            public Type type;
            public Vector3 waypointPosition;
            public ulong machineID;
            public int spawnSlot;
            public ulong source;

            public Data(Type type, Vector3 waypointPosition, ulong machineID, int spawnSlot, ulong source)
            {
                this.type = type;
                this.waypointPosition = waypointPosition;
                this.machineID = machineID;
                this.spawnSlot = spawnSlot;
                this.source = source;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref type);
                serializer.SerializeValue(ref waypointPosition);
                serializer.SerializeValue(ref machineID);
                serializer.SerializeValue(ref spawnSlot);
                serializer.SerializeValue(ref source);
            }
        }
    }

    public void AddCommand(Command command)
    {
        commands.Add(command);
    }
    
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void AddCommandRPC(Command.Data commandData)
    {
        AddCommand(new Command(commandData));
    }
    
    void Update()
    {
        if(commands == null)
            return;
        
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            Command command = commands[i];
            switch (command.type)
            {

                case Command.Type.Waypoint:
                    ProcessWaypointCommand(command);
                    break;
                case Command.Type.SpawnMachine:
                    ProcessSpawnMachineCommand(command);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            commands.RemoveAt(i);
        }
    }

    void ProcessWaypointCommand(Command command)
    {
        DroneController controller = MachineInstanceManager.Instance.FetchGameObject(command.machineID).GetComponent<DroneController>();
        WaypointManager.Instance.CreateAndAssignToWaypoint(command.waypointPosition, controller);
    }

    void ProcessSpawnMachineCommand(Command command)
    {
        //DroneSpawner spawner = FindObjectsOfType<DroneSpawner>().FirstOrDefault(d => d.teamID == (int)command.source);
        DroneSpawner spawner = MatchManager.Instance.TeamSpawner((int)command.source);
        spawner.SpawnMachine(NetworkUtils.NetworkMachineID(command.source, command.spawnSlot));
    }


}
