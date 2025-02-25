using System;
using System.Runtime.InteropServices;
using _Scripts.Player.Movement;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace _Scripts.Player.Weapon
{
    public class WeaponScript : MonoBehaviour
    {
        public float offset;

        public float attackDelay = 2f;
        private float _curAttDelay;
        
        public float comboTimer = 1.6f;
        private float _curComboTimer;
        
        public float attackTime = 0.20f;
        private float _curAttackTime;

        public float bufferAttackTimer = 0.5f;
        private float _curBufferTimer;
        
        private PolygonCollider2D _polygonCollider2D;
        private Camera _playerCamera;

        private Animator _animator;
        private string _curAnimationState;
        private float _animationTime;

        private PlayerMovement _playerMovement;
        
        private float _reqZRotation;
        
        private void Awake()
        {
            _polygonCollider2D = GetComponent<PolygonCollider2D>();
            _playerCamera = transform.parent.Find("Main Camera").GetComponent<Camera>();
            _animator = transform.Find("SlashAnimation").GetComponent<Animator>();
            _playerMovement = transform.parent.GetComponent<PlayerMovement>();
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
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
            //DebugAttack();
            
            FixRotatingParent();

            FacingToMouse();
            
            UpdateTimer();
            
            CheckAttack();
            
            DisableCollider();
            
            CheckAnimation();

            if (transform.rotation.y % 180 != 0)
            {
                Debug.Log(transform.rotation.y);
            }
        }

        private void FixRotatingParent()
        {
            if (_playerMovement.Rotated)
            {
                transform.rotation = Quaternion.Euler(0f, -1 * transform.rotation.y, transform.rotation.z);
            }
        }

        private void FacingToMouse() // So that it face to the mouse (may need to change it so that it works in multiplayer)
        {
            if (_curAttackTime <= 0) // We only face to the mouse when the player is not attacking (else he could turn fast and attack everywhere)
            {
                // Don't ask me, I don't know : https://youtu.be/bY4Hr2x05p8?t=133
                Vector3 difference = _playerCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                _reqZRotation = rotZ + offset;
                transform.rotation = Quaternion.Euler(0f, transform.rotation.y, _reqZRotation);
            }
        }

        private void CheckAttack()
        {
            if (InputManager.AttackWasPressed) // init the buffer
            {
                _curBufferTimer = bufferAttackTimer;
            }
            
            if (_curBufferTimer > 0 && _curAttackTime <= 0 && (_curAttDelay <= 0 || _curComboTimer > 0)) // check if attack init and if either in a combo or starting a combo
            {
                _curBufferTimer = 0;
                Attack();
            }
        }

        private void Attack()
        {
            _curAttDelay = attackDelay;
            _curAttackTime = attackTime;
            
            _curComboTimer = _curComboTimer > 0 ? 0 : comboTimer; // basically : if he was doing a combo he can't anymore else he can combo
            
            _polygonCollider2D.enabled = true;
        }

        private void DisableCollider() // to disable the collider when the attack is finished
        {
            if (_polygonCollider2D.enabled && _curAttackTime <= 0)
            {
                _polygonCollider2D.enabled = false;
            }
        }
        
        private void ChangeAnimationState(string newState, float time = 0)
        {
            if (_curAnimationState == newState) return;
            
            if (_animationTime <= 0)
            {
                if (time != 0)
                {
                    _animationTime = time;
                }
                
                _animator.Play(newState);
                
                _curAnimationState = newState;
            }
        }

        private void CheckAnimation()
        {
            if (_curAttackTime > 0 && _curComboTimer <= 0)
            {
                ChangeAnimationState("SlashDown",0.267f); // need to put the time of the animation
            }
            else if (_curAttackTime > 0 && _curComboTimer > 0)
            {
                ChangeAnimationState("SlashUp",0.267f);
            }
            else
            {
                ChangeAnimationState("Passive");
            }
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
            if (_curAttackTime > 0)
            {
                _curAttackTime -= deltaTime;
            }
            if (_curBufferTimer > 0)
            {
                _curBufferTimer -= deltaTime;
            }
            if (_animationTime > 0)
            {
                _animationTime -= deltaTime;
            }
        }
    }
}