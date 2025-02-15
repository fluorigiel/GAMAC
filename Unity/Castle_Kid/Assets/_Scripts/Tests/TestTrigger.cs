using _Scripts.Player.Trigger;
using UnityEngine;

namespace _Scripts.Tests
{
    public class TestTrigger : MonoBehaviour
    {
    
        // Trigger 
        public CustomTrigger feetTriger;
        public CustomTrigger headTriger;
        public CustomTrigger bodyRightTriger;
        public CustomTrigger bodyLeftTriger;
    
        void Start()
        {
            feetTriger.EnteredTrigger += OnFeetTriggerEntered;
            feetTriger.ExitedTrigger += OnFeetTriggerExited;
        
            headTriger.EnteredTrigger += OnHeadTriggerEntered;
            headTriger.ExitedTrigger += OnHeadTriggerExited;
        
            bodyRightTriger.EnteredTrigger += OnBodyRightTriggerEntered;
            bodyRightTriger.ExitedTrigger += OnBodyRightTriggerExited;
        
            bodyLeftTriger.EnteredTrigger += OnBodyLeftTriggerEntered;
            bodyLeftTriger.ExitedTrigger += OnBodyLeftTriggerExited;
        }
    
        void OnFeetTriggerEntered(Collider2D item)
        {
            Debug.Log("Item entered feet :" + item.name);
        }

        void OnFeetTriggerExited(Collider2D item)
        {
            Debug.Log("Item exited feet :" + item.name);
        }
    
        void OnHeadTriggerEntered(Collider2D item)
        {
            Debug.Log("Item entered head :" + item.name);
        }

        void OnHeadTriggerExited(Collider2D item)
        {
            Debug.Log("Item exited head :" + item.name);
        }
    
        void OnBodyRightTriggerEntered(Collider2D item)
        {
            Debug.Log("Item entered right :" + item.name);
        }

        void OnBodyRightTriggerExited(Collider2D item)
        {
            Debug.Log("Item exited right :" + item.name);
        }
    
        void OnBodyLeftTriggerEntered(Collider2D item)
        {
            Debug.Log("Item entered left :" + item.name);
        }

        void OnBodyLeftTriggerExited(Collider2D item)
        {
            Debug.Log("Item exited left :" + item.name);
        }
    }
}
