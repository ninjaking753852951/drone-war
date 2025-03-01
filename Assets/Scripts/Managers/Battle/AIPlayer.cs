using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/AI Player")]
public class AIPlayer : ScriptableObject
{
    public Difficulty difficulty;

    public float incomeMultiplier;
    
    public enum Difficulty
    {
        Easy,Medium,Hard, MasterStrategist
    }
    
    public string name;

    public int2 spawnSlots;
        
    // add slot indices
    public AIPlayer(string name)
    {
        this.name = name;
    }
}
