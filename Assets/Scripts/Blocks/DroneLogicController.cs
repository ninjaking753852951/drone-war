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

    public bool isSatisfied;

    
    void Awake()
    {
        controller = GetComponent<DroneController>();
    }

    void Start()
    {
        if (MatchManager.Instance.Team(controller.curTeam).isAI)
        {
            orders.Add(new CaptureNearestObjective());
            //orders.Add(new WanderAround(25,5));
        }
    }

    void Update()
    {
        debugOrderCount = orders.Count;
        isSatisfied = false;
        foreach (var order in orders)
        {
            if (!order.IsSatisfied(this))
            {
                order.Execute(this);
                isSatisfied = true;
                break;
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
        logicController.controller.SetDestination(destination);
    }

    public void Dispose()
    {
        
    }
}

public class CaptureNearestObjective : IDroneOrder
{
    public Vector3 destination;
    
    const float statisfiedDistance = 10;

    List<MapObjectivePoint> objectives;
    List<Transform> targetObjectives = new List<Transform>();
    
    
    public CaptureNearestObjective()
    {

        objectives = GameObject.FindObjectsOfType<MapObjectivePoint>().ToList();

    }

    void UpdateTargetObjectives(DroneLogicController logic)
    {
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
        Debug.Log(closestTarget.transform.position);
        
        logicController.controller.SetDestination(closestTarget.position);
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
            logicController.controller.SetDestination(wanderLocation);
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