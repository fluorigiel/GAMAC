using UnityEngine;

namespace _Scripts.Player.Weapon
{
    public class SlashScript : MonoBehaviour
    {
        private WeaponScript _weaponScript;
    
        public void Awake()
        {
            _weaponScript = GetComponentInParent<WeaponScript>();
        }

        private void DisableSlash()
        {
            _weaponScript.DisableCollider();
        }
    }
}
