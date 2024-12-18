    using System.Collections.Generic;
    using Interfaces;
    using Unity.Netcode;
    using UnityEngine;
    [System.Serializable]
    public class MachineSaveData
    {
        public List<MachineSaveLoadManager.BlockSaveData> blocks = new List<MachineSaveLoadManager.BlockSaveData>();
        public float totalCost;

        public DroneController Spawn(Vector3 offset = default, Vector3 eulerRot = default, Transform parent = null, int teamID = 0, bool network = true)
        {
            // parent should be null for drones that are gonna deploy
            DroneController droneController = null;

            foreach (MachineSaveLoadManager.BlockSaveData blockSaveData in blocks)
            {
                IPlaceable blockData = BlockLibraryManager.Instance.BlockData(blockSaveData.blockID);
                if (blockData == null)
                    continue;
                
                Quaternion rotation = Quaternion.Euler(eulerRot) *Quaternion.Euler(blockSaveData.eulerRot);

                Vector3 position = (Quaternion.Euler(eulerRot) * (blockSaveData.pos) + offset);

                GameObject newBlock = blockData.Spawn(position, rotation, network);
                newBlock.transform.parent = parent;
                
                //Inject the saved meta data so that it can be pulled by whatever needs it

                DroneBlock droneBlock = newBlock.GetComponent<DroneBlock>();


                if (blockSaveData.meta != null && droneBlock != null)
                {
                    droneBlock.meta = blockSaveData.meta;     
                }

                
                DroneController curDroneController = newBlock.GetComponent<DroneController>();
                if (curDroneController != null)
                {
                    droneController = curDroneController;
                    
                    if (NetworkManager.Singleton.IsListening)
                    {
                        DroneNetworkController networkController = newBlock.GetComponent<DroneNetworkController>();

                        if (networkController != null)
                        {
                            networkController.blockCount.Value = blocks.Count;
                            networkController.curTeam.Value = teamID;
                        }
                    }
                    
                    droneController.curTeam = teamID;
                    //droneController.blockCount.Value = blocks.Count;
                }
            }

            return droneController;
        }


        public Sprite GenerateThumbnail()
        {
            Vector3 posOffset = Vector3.down * 1000; // Ensure it spawns out of view
            GameObject machineParent = new GameObject();
            DroneController droneController = Spawn(posOffset, new Vector3(-35, 35, 0), machineParent.transform, network: false);
            if (droneController == null)
            {
                return null;
            }

            Sprite thumbnail = ThumbnailGenerator.Instance.GenerateThumbnail(machineParent, 0.1f);

            return thumbnail;
        }
        
        public string SerializeToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static MachineSaveData DeserializeFromJson(string json)
        {
            return JsonUtility.FromJson<MachineSaveData>(json);
        }
    }