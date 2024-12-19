using System.Collections.Generic;
using System.Linq;
using Interfaces;using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class TankTrackBuilder : IPlaceable, IStepPlaceable
{
    public int TotalSteps => 3;

    List<Vector3> points = new List<Vector3>();
    private bool isComplete = false;

    public GameObject tankTrackPrefab;

    public GameObject marker;



    public bool IsPlacementComplete => isComplete;

    public void OnStepCompleted(Vector3 position)
    {
        
        if (points.Contains(position))
        {
            isComplete = true;
            FinalizePlacement();
            return;
        }

        points.Add(position);
    }

    public GameObject FinalizePlacement()
    {
        
        GameObject tankTrackClone = GameObject.Instantiate(this.tankTrackPrefab, BuildingManager.Instance.spawnPoint,
            Quaternion.identity);
        //brace.transform.localScale = new Vector3(0.1f, 0.1f, distance);

        DroneBlock droneBlock = tankTrackClone.GetComponent<DroneBlock>();
        droneBlock.meta.specialPositions = points.ToList();
        droneBlock.blockIdentity = this;

        Reset();
        
        return tankTrackClone;
    }

    public void Reset()
    {
        isComplete = false;
        points = new List<Vector3>();
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
        GameObject treadClone = GameObject.Instantiate(tankTrackPrefab, pos, rot);
        treadClone.GetComponent<DroneBlock>().blockIdentity = this;
        return treadClone;
    }

    public GameObject SpawnMarker(Vector3 pos, Quaternion rot)
    {
        return GameObject.Instantiate(marker, pos, rot);
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