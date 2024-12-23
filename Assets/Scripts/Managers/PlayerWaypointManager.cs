using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerWaypointManager : WaypointManager
{

    public float spreadMultiplier = 1;

    [Header("Drag Assignment Settings")]
    public int dragAssignmentThreshold = 3;
    public GameObject dragAssignmentIndicator;
    public float cursorPathMinDistance = 0.1f;
    List<(GameObject, DroneController)> curDragIndicators = new List<(GameObject, DroneController)>();
    public List<Vector3> cursorPathPoints = new List<Vector3>();
    
    CommandManager commandManager;

    public bool isDragAssigning;
    public float holdTime;
    
    protected override void Start()
    {
        base.Start();
        commandManager = FindObjectOfType<CommandManager>();
    }
    
    void Update()
    {
        if(GameManager.Instance.currentGameMode == GameMode.Build)
            return;

        // Create and assign waypoint on click
        if (Input.GetButtonUp("Fire2") && SelectionManager.Instance.selectedDrones.Count > 0 && !isDragAssigning)
        {
            int teamID = 0;
            if (GameManager.Instance.IsOnlineAndClient())
            {
                teamID = (int)NetworkManager.Singleton.LocalClientId;
            }
            
            List<DroneController> playerDrones = Utils.DronesFromTeam(SelectionManager.Instance.selectedDrones.ToList(), teamID);
            int droneCount = playerDrones.Count;
            Debug.Log(droneCount);
            if(droneCount == 0)
                return;
            
            float spread = FindBiggestRadius(playerDrones) * 2;// ASSUMING WORST CASE SCENARIO THE TWO BIGGEST ARE NEXT TO EACHOTHER
     
            List<Vector3> posOffsets = spreadPattern.GenerateSpread(droneCount, spread * spreadMultiplier);
            Vector3 pos = Utils.CursorScanPos();
            
            for (int i = 0; i < droneCount; i++)
            {
                CreateWaypointCommand(playerDrones[i], pos + posOffsets[i]);
            }
        }

        if (holdTime > 0)
        {
            UpdateDragAssignmentIndicators();
            
            if (Vector3.Distance(cursorPathPoints[0], cursorPathPoints[^1]) > dragAssignmentThreshold && !isDragAssigning)
            {
                SpawnDragAssignmentIndicators();
            }
        }
        
        if (Input.GetButton("Fire2"))
        {
            holdTime += Time.deltaTime;
        }
        
        if (Input.GetButtonUp("Fire2"))
        {
            holdTime = 0;
            FinishDragAssignment();
        }
        
    }

    void SpawnDragAssignmentIndicators()
    {
        isDragAssigning = true;
        
        Vector3 pos = Utils.CursorScanPos();
        
        int teamID = 0;
        if (GameManager.Instance.IsOnlineAndClient())
        {
            teamID = (int)NetworkManager.Singleton.LocalClientId;
        }
        
        List<DroneController> playerDrones = Utils.DronesFromTeam(SelectionManager.Instance.selectedDrones.ToList(), teamID);

        for (int i = 0; i < playerDrones.Count; i++)
        {
            DroneController curDrone = SelectionManager.Instance.selectedDrones[i];
            curDragIndicators.Add((Instantiate(dragAssignmentIndicator, pos, quaternion.identity), curDrone));
        }
    }

    void FinishDragAssignment()
    {
        if (isDragAssigning)
        {
            for (int i = 0; i < curDragIndicators.Count; i++)
            {
                CreateWaypointCommand(curDragIndicators[i].Item2, curDragIndicators[i].Item1.transform.position);
            }
        }
        
        isDragAssigning = false;
                
        DisposeDragIndicators();
        cursorPathPoints.Clear();
    }

    void UpdateDragAssignmentIndicators()
    {
        Vector3 cursorPos = Utils.CursorScanPos();
        
        if(cursorPathPoints.Count == 0)
            cursorPathPoints.Add(cursorPos);
        
        while (Vector3.Distance(cursorPathPoints[^1], cursorPos) > cursorPathMinDistance)
        {
            Vector3 newPos = Vector3.MoveTowards(cursorPathPoints[^1], cursorPos, cursorPathMinDistance);
            cursorPathPoints.Add(newPos);
        }
        
        for (int i = 0; i < curDragIndicators.Count; i++)
        {
            float samplePoint = i / (float)(curDragIndicators.Count -1);
            if (float.IsNaN(samplePoint))
                samplePoint = 0;
            Vector3 pos = SamplePointAlongPath(cursorPathPoints, samplePoint);
            
            
            curDragIndicators[i].Item1.transform.position = pos;   
        }
    }

    Vector3 SamplePointAlongPath(List<Vector3> pos, float point)
    {
        if (pos.Count == 0)
            return Vector3.zero;
        
        if (pos.Count < 2)
            return pos[0];
        
        float x = (pos.Count - 1) * point;
        
        int startLerpIndex = Mathf.FloorToInt(x);

        float lerpAmount = x % 1;

        if (startLerpIndex == pos.Count - 1)
            return pos[^1];

        return Vector3.Lerp(pos[startLerpIndex], pos[startLerpIndex + 1], lerpAmount);

    }

    void DisposeDragIndicators()
    {
        foreach (var curDragIndicator in curDragIndicators)
        {
            Destroy(curDragIndicator.Item1);
        }
        
        curDragIndicators.Clear();
    }

    void CreateWaypointCommand(DroneController drone, Vector3 pos)
    {
        ulong droneID = MachineInstanceManager.Instance.FetchID(drone.gameObject);
        
        Debug.Log("Drone ID" + droneID);

        commandManager = FindObjectOfType<CommandManager>();
        
        if (commandManager != null)
        {
            Debug.Log("Command manager is present");
            
            bool queue = Input.GetKey(KeyCode.LeftShift);

            if (GameManager.Instance.IsOnlineAndClient())
            {
                Debug.Log("Sending command to server");
                commandManager.AddCommandRPC(new CommandManager.Command(NetworkManager.Singleton.LocalClientId, droneID,pos, queue).GenerateData());   
            }
            else
            {
                commandManager.AddCommand(new CommandManager.Command(0,droneID,pos, queue));
            }
        }
    }
    
}