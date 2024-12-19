using Interfaces;using UnityEngine;

[System.Serializable]
public class BraceBlock : IPlaceable, IStepPlaceable
{
    public int TotalSteps => 3;

    private Vector3[] points = new Vector3[2];
    private bool isComplete = false;

    public GameObject brace;

    public void OnStepCompleted(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public bool IsPlacementComplete => isComplete;

    public void OnStepCompleted(int stepIndex, Vector3 position)
    {
        points[stepIndex] = position;
    }

    public GameObject FinalizePlacement()
    {
        // Instantiate the brace between points[0] and points[1]
        Vector3 midpoint = (points[0] + points[1]) / 2;
        Vector3 direction = points[1] - points[0];
        float distance = direction.magnitude;

        GameObject brace = GameObject.Instantiate(this.brace, midpoint, Quaternion.LookRotation(direction));
        brace.transform.localScale = new Vector3(0.1f, 0.1f, distance);

        isComplete = true;
        return brace;
    }

    public GameObject SpawnMarker(Vector3 pos, Quaternion rot)
    {
        throw new System.NotImplementedException();
    }

    public string PlaceableName()
    {
        return "BRACE";
    }

    public float Cost()
    {
        return 0;
    }

    public GameObject Spawn(Vector3 pos, Quaternion rot, bool network = true)
    {
        return GameObject.Instantiate(this.brace, pos, rot);
    }

    public Sprite Thumbnail()
    {
        return null;
    }

    public BlockType Category()
    {
        return BlockType.Basic;
    }
}