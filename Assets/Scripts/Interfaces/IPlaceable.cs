using UnityEngine;

namespace Interfaces
{
    public interface IPlaceable
    {

        /*public Placeable(BlockData block, MachineSaveLoadManager.SubAssemblySaveData subAssembly = null)
        {
            isSubAssembly = block == null;
            this.block = block;
            this.subAssembly = subAssembly;
        }*/

        public string PlaceableName();

        public float Cost();

        public GameObject Spawn(Vector3 pos, Quaternion rot, bool network = true);

        public Sprite Thumbnail();

        public BuildingManagerUI.PlaceableCategories Category();

    }
}
