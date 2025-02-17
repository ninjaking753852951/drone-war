using System;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using UnityEngine;
using Random = UnityEngine.Random;

public class DroneLogicController : MonoBehaviour
{
    [HideInInspector]
    public DroneController controller;

    public List<IDroneOrder> orders = new List<IDroneOrder>();

    public float debugOrderCount;
    public List<Transform> targetObjectives2;

    float logicCD;
        
    void Awake()
    {
        controller = GetComponent<DroneController>();
    }

    public void Init()
    {
        if (GameManager.Instance.currentGameMode == GameMode.Battle)
        {
            if (MatchManager.Instance.Team(controller.curTeam).isAI)
            {
                DebugLogger.Instance.Log("AI ORDER APPLIED ", 2);
                orders.Add(new CaptureNearestObjective());
                //orders.Add(new WanderAround(50, 3));
                //orders.Add(new WanderAround(25,5));
            }   
        }
    }

    void Update()
    {

        logicCD += Time.deltaTime;
        if (logicCD > 1)
        {
            logicCD = 0;
            LogicUpdate();
        }

    }

    void LogicUpdate()
    {
        debugOrderCount = orders.Count;
        foreach (var order in orders)
        {
            if (!order.IsSatisfied(this))
            {
                order.Execute(this);
                break;
            }
            else
            {
                DebugLogger.Instance.Log("IM SATISFIED ", 5);
            }
        }
    }
    
    void OnDestroy()
    {
        foreach (var order in orders)
        {
            order.Dispose();
        }
    }
}

public class GoToLocationOrder : IDroneOrder
{
    public Vector3 destination;
    
    const float statisfiedDistance = 10;

    public GoToLocationOrder(Vector3 destination)
    {
        this.destination = destination;
    }

    public bool IsSatisfied(DroneLogicController logicController)
    {
        return Vector3.Distance(logicController.transform.position, destination) < statisfiedDistance;
    }

    public void Execute(DroneLogicController logicController)
    {
        WaypointManager.Instance.CreateAndSetWaypoint(destination, logicController.controller);
        //logicController.controller.SetDestination(destination);
    }

    public void Dispose()
    {
        
    }
}

public class CaptureNearestObjective : IDroneOrder
{

    

    List<MapObjectivePoint> objectives;
    List<Transform> targetObjectives = new List<Transform>();
    
    
    public CaptureNearestObjective()
    {

        objectives = GameObject.FindObjectsOfType<MapObjectivePoint>().ToList();

    }

    void UpdateTargetObjectives(DroneLogicController logic)
    {
        DebugLogger.Instance.Log("OBJECTIVE UPDATE " + targetObjectives.Count, 1);
        targetObjectives.Clear();
        
        foreach (var objective in objectives)
        {
            if (!objective.currentOwner.HasValue)
            {
                targetObjectives.Add(objective.transform);
            }
            else
            {
                if (objective.currentOwner.Value != logic.controller.curTeam)
                {
                    targetObjectives.Add(objective.transform);
                }
            }
        }
    }

    public bool IsSatisfied(DroneLogicController logicController)
    {
        UpdateTargetObjectives(logicController);
        
        return targetObjectives.Count == 0;
    }

    public void Execute(DroneLogicController logicController)
    {
        UpdateTargetObjectives(logicController);

        logicController.targetObjectives2 = targetObjectives;
        
        if (targetObjectives.Count == 0)
        {
            return;
        }

        Transform closestTarget = Utils.ClosestTo(targetObjectives, logicController.transform.position);
        Vector3 destination = closestTarget.position;
        

        
       Debug.Log("SETTING WAYPOINT");
        WaypointManager.Instance.CreateAndSetWaypoint(destination, logicController.controller);
    }

    public void Dispose()
    {
        
    }
}

public class WanderAround : IDroneOrder
{
    Vector3 wanderLocation;
    float wanderDistance;

    float wanderCooldown;

    CountdownTimer cooldownTimer;

    public WanderAround(float wanderDistance, float wanderCooldown)
    {
        this.wanderDistance = wanderDistance;
        this.wanderCooldown = wanderCooldown;
    }

    public bool IsSatisfied(DroneLogicController logicController)
    {
        return false;
    }

    public void Execute(DroneLogicController logicController)
    {
        if (cooldownTimer == null)
        {
            cooldownTimer = new CountdownTimer(wanderCooldown);
            cooldownTimer.Start();
        }

        if (cooldownTimer.IsFinished)
        {
            Vector3 offset = Random.insideUnitSphere * wanderDistance;
            offset.y = 0;
            wanderLocation = logicController.transform.position + offset;
            WaypointManager.Instance.CreateAndSetWaypoint(wanderLocation, logicController.controller);
            cooldownTimer.Reset(wanderCooldown);
            cooldownTimer.Start();
        }
    }

    public void Dispose()
    {
        cooldownTimer.Dispose();
    }
}

public interface IDroneOrder
{
    public bool IsSatisfied(DroneLogicController logicController);

    public void Execute(DroneLogicController logicController);

    public void Dispose();
}