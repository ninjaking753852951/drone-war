using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
public class NetworkDroneSpawnerHelper : NetworkHelperBase
{
        PlayerDroneSpawner spawner;

        public Color colour;

        public NetworkVariable<ulong> playerClientID;
        public NetworkVariable<Color> teamColour;
        public NetworkVariable<float> budget;
        public NetworkVariable<float> income;
        
        void Awake()
        {
                if (!NetworkManager.Singleton.IsListening)
                {
                        this.enabled = false;
                        return;
                }
                //teamColour.Value = colour;
                playerClientID.Initialize(this);
                teamColour.Initialize(this);
                
                spawner = GetComponent<PlayerDroneSpawner>();
        }

        void Start()
        {
                Debug.Log(playerClientID.Value + "   " + teamColour.Value);

                spawner.teamData.colour = teamColour.Value;
                
                if (playerClientID.Value == NetworkManager.Singleton.LocalClientId)
                {
                        spawner.Init();
                }
        }

        void Update()
        {
                SyncValue(budget, ref spawner.teamData.budget);
                SyncValue(income, ref spawner.teamData.curIncome);
        }
        
        public void Init(ulong clientID)
        {
                teamColour.Value = Random.ColorHSV(0, 1, 0.96f, 0.9f, 0.9f, 0.9f);
                playerClientID.Value = clientID;
                
                NetworkObject netObj = GetComponent<NetworkObject>();
                netObj.Spawn();
        }
}
