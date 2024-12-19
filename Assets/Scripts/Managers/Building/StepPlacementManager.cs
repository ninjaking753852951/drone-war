using UnityEngine;

[System.Serializable]
public class StepPlacementManager
{
    /*IStepPlaceable currentStepPlaceable;
    public int currentStepIndex;

    public bool IsActive => currentStepPlaceable != null;

    public void StartPlacement(IStepPlaceable stepPlaceable)
    {
        currentStepPlaceable = stepPlaceable;
        currentStepIndex = 0;
    }

    public void ProcessStep(Vector3 position)
    {
        if (currentStepPlaceable == null)
            return;

        currentStepPlaceable.OnStepCompleted(position);
        currentStepIndex++;

        if (currentStepPlaceable.IsPlacementComplete)
        {
            CompletePlacement();
        }
    }

    private void CompletePlacement()
    {
        currentStepPlaceable.FinalizePlacement();
        currentStepPlaceable = null;
        currentStepIndex = 0;
    }*/
}