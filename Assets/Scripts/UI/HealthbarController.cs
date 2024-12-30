using UnityEngine;

public class HealthbarController : MonoBehaviour
{

    public Transform incrementParent;

    public GameObject emptyIncrement;
    
    public GameObject increment1;
    
    public GameObject increment2;

    public float totalHealth;

    public float incrementSize = 100;

    public float largeIncrementInterval = 10;
    
    public void GenerateHealthBar(float health)
    {
        totalHealth = health;

        int incrementCount = Mathf.FloorToInt( (totalHealth / incrementSize));

        for (int i = 0; i < incrementCount; i++)
        {
            if (i == 0)
            {
                SpawnIncrement(emptyIncrement);
            }
            else if (i % largeIncrementInterval == 0)
            {
                SpawnIncrement(increment2);
            }
            else
            {
                SpawnIncrement(increment1);   
            }
        }
    }

    void SpawnIncrement(GameObject prefab)
    {
        Instantiate(prefab, incrementParent);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GenerateHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
