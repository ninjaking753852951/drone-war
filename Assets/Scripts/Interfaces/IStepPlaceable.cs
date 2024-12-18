using UnityEngine;

public interface IStepPlaceable
{
    int TotalSteps { get; } // Number of steps required
    void OnStepCompleted(int stepIndex, Vector3 position); // Called when each step is completed
    bool IsPlacementComplete { get; } // Whether the placement is fully complete
    GameObject FinalizePlacement(); // Finalize and return the resulting GameObject

    GameObject SpawnMarker(Vector3 pos, Quaternion rot);
}