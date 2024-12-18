using UnityEngine;

namespace Interfaces
{
    public interface IPlaceable
    {
        public string PlaceableName();

        public float Cost();

        public GameObject Spawn(Vector3 pos, Quaternion rot, bool network = true);

        public Sprite Thumbnail();

        public BlockType Category();

    }
}
