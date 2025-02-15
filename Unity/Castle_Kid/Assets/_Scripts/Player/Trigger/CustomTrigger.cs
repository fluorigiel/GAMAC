using UnityEngine;

namespace _Scripts.Player.Trigger
{
    public class CustomTrigger : MonoBehaviour
    {

        public event System.Action<Collider> EnteredTrigger;
        public event System.Action<Collider> ExitedTrigger;

        private void OnTriggerEnter(Collider item)
        {
            EnteredTrigger?.Invoke(item);
        }

        private void OnTriggerExit(Collider item)
        {
            ExitedTrigger?.Invoke(item);
        }
    }
}
