using UnityEngine;

namespace _Scripts.Player.Trigger
{
    public class CustomTrigger : MonoBehaviour
    {

        private string GroundLayer = "Ground";
        private string PlayerTag = "Player";

        public event System.Action<Collider2D> EnteredTrigger;
        public event System.Action<Collider2D> ExitedTrigger;

        private void OnTriggerEnter2D(Collider2D item)
        {
            //EnteredTrigger.Invoke(item);
            if (item.tag == PlayerTag || LayerMask.LayerToName(item.transform.parent.gameObject.layer) == GroundLayer) // long line to access the layer of the collider2D
            {
                EnteredTrigger.Invoke(item);
            }
        }

        private void OnTriggerExit2D(Collider2D item)
        {
            //ExitedTrigger.Invoke(item);
            if (item.tag == PlayerTag || LayerMask.LayerToName(item.transform.parent.gameObject.layer) == GroundLayer) // long line to access the layer of the collider2D
            {
                ExitedTrigger.Invoke(item);
            }
        }
    }
}