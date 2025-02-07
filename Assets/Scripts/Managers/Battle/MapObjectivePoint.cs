using System.Collections.Generic;
using UnityEngine;
public class MapObjectivePoint : MonoBehaviour
{
    public enum PointState
    {
        Unowned,
        Owned,
        Occupied,
        Contested
    }

    public int forceOwner = -1;//-1 means dont force an owner

    public int? currentOwner;
    public List<int> currentOccupants = new List<int>();

    public float income = 100;
    public float radius = 5f;
    public float pointMaxHealth = 10f;
    public LayerMask droneLayer;
    public float pollingInterval = 0.5f;
    public Transform indicator;
    public CircleRenderer healthIndicatorCircle;

    public Color unownedColour;
    
    [HideInInspector]
    public float healthFill;
    
    private float pointHealth;
    public PointState currentState;
    private float timeSinceLastPoll;

    public void Start()
    {
        if(GameManager.Instance.currentGameMode != GameMode.Battle)
            gameObject.SetActive(false);
        // set indicator size correct
        SetIndicatorSize();
        /*if (forceOwner != -1)
        {
            currentOwner = forceOwner;
            currentState = PointState.Owned;
            SetIndicatorColour(currentOwner.Value);
        }*/
        
        healthIndicatorCircle.SetRadius(radius);
        
        pointHealth = pointMaxHealth;
        UpdatePointState();
    }

    void Update()
    {
        //Health Indicator

        healthFill = 1 - (pointHealth / pointMaxHealth);
        SetFill(healthFill);
        
        timeSinceLastPoll += Time.deltaTime;

        if (timeSinceLastPoll >= pollingInterval)
        {
            PollForOccupants();
            UpdatePointState();
            timeSinceLastPoll = 0f;
        }

        if (currentState == PointState.Occupied || currentState == PointState.Contested)
        {
            UpdatePointHealth();
        }
    }

    public void SetIndicatorSize()
    {
        indicator.transform.localScale = new Vector3(radius * 2, indicator.transform.localScale.y, radius * 2);

    }
    
    public void SetFill(float amount)
    {
        healthIndicatorCircle.SetFill(amount);
        //healthIndicator.transform.localScale = new Vector3(radius * 2 * amount, healthIndicator.transform.localScale.y, radius * 2 *amount);
    }
    
    void PollForOccupants()
    {
        currentOccupants.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, droneLayer);
        foreach (Collider collider in hitColliders)
        {
            DroneController drone = collider.transform.root.GetComponentInChildren<DroneController>();
            if (drone != null)
            {
                int team = drone.curTeam;
                if (!currentOccupants.Contains(team))
                {
                    currentOccupants.Add(team);
                }
            }
        }
    }

    void UpdatePointState()
    {
        if (!currentOwner.HasValue && currentOccupants.Count == 0)
        {
            currentState = PointState.Unowned;
        }
        else if (currentOccupants.Count == 0)
        {
            currentState = currentOwner.HasValue ? PointState.Owned : PointState.Occupied;
        }
        else if (currentOccupants.Count == 1 && currentOccupants[0] == currentOwner)
        {
            currentState = PointState.Owned;
            SetIndicatorColour(currentOwner.Value);

        }
        else if (currentOccupants.Count == 1)
        {
            currentState = PointState.Occupied;
        }
        else
        {
            currentState = PointState.Contested;
        }
    }

    public void SetIndicatorColour(int team)
    {
        Color colour;
        
        Renderer rend = indicator.GetComponentInChildren<Renderer>();
        if (team == -1)
        {
            colour = unownedColour;

        }
        else
        {
            colour = MatchManager.Instance.Team(team).colour;
        }

        rend.material.SetColor("_MainColour", colour);   
    }

    void UpdatePointHealth()
    {
        if (currentState == PointState.Contested)
        {
            return;
        }

        int occupyingTeam = currentOccupants.Find(team => team != currentOwner);
        if (occupyingTeam != null)
        {
            pointHealth -= Time.deltaTime;

            if (pointHealth <= 0)
            {
                if(currentOwner != null)
                    MatchManager.Instance.Team(currentOwner.Value).curIncome -= income;
                currentOwner = occupyingTeam;
                MatchManager.Instance.Team(currentOwner.Value).curIncome += income;
                pointHealth = pointMaxHealth;
                UpdatePointState();
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Reset()
    {
        currentOwner = null;
        currentOccupants.Clear();
        pointHealth = pointMaxHealth;
        currentState = PointState.Unowned;

        // Reset visual indicators
        SetIndicatorColour(-1); // Reset to default color or clear
        healthIndicatorCircle.SetFill(0);
        //healthIndicator.transform.localScale = new Vector3(0, healthIndicator.transform.localScale.y, 0);
    }
}