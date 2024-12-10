using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityUtils;
public class CommandManager : Singleton<CommandManager>
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
        public int source = 0;

        public Command(ulong machineID, Vector3 waypointPosition)
        {
            this.machineID = machineID;
            this.waypointPosition = waypointPosition;
            type = Type.Waypoint;
        }
        
        public Command(int spawnSlot)
        {
            this.spawnSlot = spawnSlot;
            type = Type.SpawnMachine;
        }

        Data GenerateData()
        {
            return new Data(type, waypointPosition, machineID, spawnSlot, source);
        }
        
        struct Data
        {
            public Type type;
            public Vector3 waypointPosition;
            public ulong machineID;
            public int spawnSlot;
            public int source;
            public Data(Type type, Vector3 waypointPosition, ulong machineID, int spawnSlot, int source)
            {
                this.type = type;
                this.waypointPosition = waypointPosition;
                this.machineID = machineID;
                this.spawnSlot = spawnSlot;
                this.source = source;
            }
        }
    }

    public void AddCommand(Command command)
    {
        commands.Add(command);
    }

    void Update()
    {
        if(commands == null)
            return;
        
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            Command command = commands[i];
            Debug.Log("PROCESSING COMMAND " + command);
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
        DroneSpawner spawner = FindObjectsOfType<DroneSpawner>().FirstOrDefault(d => d.teamID == command.source);
        spawner.SpawnMachine(command.spawnSlot);
    }


}
