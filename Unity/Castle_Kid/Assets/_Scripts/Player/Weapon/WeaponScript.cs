using System;
using System.Runtime.InteropServices;
using _Scripts.Player.Movement;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace _Scripts.Player.Weapon
{
    public class WeaponScript : MonoBehaviour
    {
        public float offset;

        public float attackDelay = 1.3f;
        private float _curAttDelay;
        
        public float comboTimer = 0.6f;
        private float _curComboTimer;

        public float bufferAttackTimer = 0.2f;
        private float _curBufferTimer;
        
        private PolygonCollider2D _polygonCollider2D;
        private Camera _playerCamera;

        private Animator _animator;
        
        private float _fixedYRotation;
        private Quaternion _slashRotation;
        
        private void Awake()
        {
            _polygonCollider2D = GetComponent<PolygonCollider2D>();
            _polygonCollider2D.enabled = false;
            _playerCamera = transform.parent.Find("Main Camera").GetComponent<Camera>();
            _animator = transform.Find("SlashAnimation").GetComponent<Animator>();
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _fixedYRotation = transform.eulerAngles.y;
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
                    Debug.Log($"Couldn't attack because : \nCurrent combo Timer : {_curComboTimer} \nCurrent attack delay {_curAttDelay}");
                }
            }
        }

        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        void Update()
        {
            //DebugAttack();
            
            FixRotatingParent();

            FacingToMouse();
            
            UpdateTimer();
            
            CheckAttack();
        }

        private void FixRotatingParent()
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, _fixedYRotation, transform.eulerAngles.z);

            if (_polygonCollider2D.enabled)
            {
                transform.rotation = _slashRotation;
            }
        }

        private void FacingToMouse() // So that it face to the mouse (may need to change it so that it works in multiplayer)
        {
            if (!_polygonCollider2D.enabled) // We only face to the mouse when the player is not attacking (else he could turn fast and attack everywhere)
            {
                // Don't ask me, I don't know : https://youtu.be/bY4Hr2x05p8?t=133
                Vector3 difference = _playerCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
                _slashRotation = transform.rotation;
            }
        }

        private void CheckAttack()
        {
            if (InputManager.AttackWasPressed) // init the buffer
            {
                _curBufferTimer = bufferAttackTimer;
            }
            
            if (_curBufferTimer > 0 && !_polygonCollider2D.enabled && (_curAttDelay <= 0 || _curComboTimer > 0)) // check if attack init and if either in a combo or starting a combo
            {
                _curBufferTimer = 0;
                Attack();
            }
        }

        private void Attack()
        {
            _curAttDelay = attackDelay;

            if (_curComboTimer > 0) // basically : if he was doing a combo he can't anymore else he can combo // is in a combo
            {
                _curComboTimer = 0;
                _animator.Play("SlashDown");
            }
            else // is not in a combo
            {
                _curComboTimer = comboTimer;
                _animator.Play("SlashUp");
            }
            
            _polygonCollider2D.enabled = true;
        }

        public void DisableCollider() // to disable the collider when the attack is finished
        {
            _polygonCollider2D.enabled = false;
            _animator.Play("Passive");
        }

        private void UpdateTimer()
        {
            float deltaTime = Time.deltaTime;

            if (_curAttDelay > 0)
            {
                _curAttDelay -= deltaTime;
            }
            if (_curComboTimer > 0)
            {
                _curComboTimer -= deltaTime;
            }
            if (_curBufferTimer > 0)
            {
                _curBufferTimer -= deltaTime;
            }
        }
    }
}