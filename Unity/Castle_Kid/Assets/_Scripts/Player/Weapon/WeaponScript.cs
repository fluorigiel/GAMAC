using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace _Scripts.Player.Weapon
{
    public class WeaponScript : MonoBehaviour
    {
        public float offset;

        public float attackDelay = 2f;
        private float _curAttDelay;
        
        public float comboTimer = 1.6f;
        private float _curComboTimer;
        
        public float attackTime = 1f;
        private float _curAttackTime;

        public float bufferAttackTimer = 0.5f;
        private float _curBufferTimer;
        
        private PolygonCollider2D _polygonCollider2D;
        private Camera _playerCamera;
        
        private void Awake()
        {
            _polygonCollider2D = GetComponent<PolygonCollider2D>();
            _playerCamera = transform.parent.Find("Main Camera").GetComponent<Camera>();
        }

        private void DebugAttack()
        {
            if (InputManager.AttackWasPressed)
            {
                if (_curComboTimer > 0)
                {
                    Debug.Log("Combo Attack");
                }
                else if (_curAttDelay <= 0)
                {
                    Debug.Log("Normal Attack");
                }
                else
                {
                    Debug.Log($"Couldn't attack because : \nCurrent combo Timer : {_curComboTimer} \nCurrent attack Time : {_curAttackTime} \nCurrent attack delay {_curAttDelay}");
                }
            }
        }

        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        void Update()
        {
            DebugAttack();
            
            FacingToMouse();
            
            UpdateTimer();
            
            CheckAttack();
            
            DisableCollider();
        }

        private void FacingToMouse() // So that it face to the mouse (may need to change it so that it works in multiplayer)
        {
            if (_curAttackTime <= 0) // We only face to the mouse when the player is not attacking (else he could turn fast and attack everywhere)
            {
                // Don't ask me, I don't know : https://youtu.be/bY4Hr2x05p8?t=133
                Vector3 difference = _playerCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
            }
        }

        private void CheckAttack()
        {
            if (InputManager.AttackWasPressed) // init the buffer
            {
                _curBufferTimer = bufferAttackTimer;
            }
            
            if (_curBufferTimer > 0 && (_curAttDelay <= 0 || _curComboTimer > 0)) // check if attack init and if either in a combo or starting a combo
            {
                _curBufferTimer = 0;
                Attack();
            }
        }

        private void Attack()
        {
            _curAttDelay = attackDelay;
            _curAttackTime = attackTime;
            
            if (_curComboTimer > 0) // The player is doing a combo
            {
                _curComboTimer = 0;
                // animation combo
            }
            else // The player can start a combo
            {
                _curComboTimer = comboTimer;
                // animation not combo
            }

            _polygonCollider2D.enabled = true; // enable the collider for hitBox
        }

        private void DisableCollider() // to disable the collider when the attack is finished
        {
            if (_polygonCollider2D.enabled && _curAttackTime <= 0)
            {
                _polygonCollider2D.enabled = false;
            }
        }
        
        private void UpdateTimer()
        {
            float deltaTime = Time.fixedDeltaTime;
            
            if (_curAttDelay > 0)
            {
                _curAttDelay -= deltaTime;
            }
            if (_curComboTimer > 0)
            {
                _curComboTimer -= deltaTime;
            }
            if (_curAttackTime > 0)
            {
                _curAttackTime -= deltaTime;
            }
            if (_curBufferTimer > 0)
            {
                _curBufferTimer -= deltaTime;
            }
        }
    }
}
