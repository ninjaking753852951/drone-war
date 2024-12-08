using UnityEngine;
public interface IDamageable
{
        public void DealDamage();

        public int Team();

        public Transform Transform();

        public void RegisterDamageable();
        public void DeregisterDamageable();
}
