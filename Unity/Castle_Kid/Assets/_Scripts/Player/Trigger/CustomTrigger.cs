using UnityEngine;

namespace _Scripts.Player.Trigger
{
    public class CustomTrigger : MonoBehaviour
    {

        public event System.Action<Collider2D> EnteredTrigger;
        public event System.Action<Collider2D> ExitedTrigger;

        private void OnTriggerEnter2D(Collider2D item)
        {
            EnteredTrigger?.Invoke(item);
        }

        private void OnTriggerExit2D(Collider2D item)
        {
            ExitedTrigger?.Invoke(item);
        }
    }
}