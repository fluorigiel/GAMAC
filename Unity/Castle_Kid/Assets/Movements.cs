using System;
using UnityEngine;

public class Movements : MonoBehaviour
{

    public Rigidbody2D rb;
    public float jumpHeight = 5f;
    private bool onGround = true;
    
    private int countJump = 0;
    private float multipleJumpCooldown = 0.1f;
    private float lastJumpTime = 0f;
    public int maxJump = 2;
    
    public float dashingPower = 30f;
    private float dashDuration = 0.1f;
    private float dashingCooldown = 1f;
    private float lastDashTime = 0f;
    private float originalGravity = 0f;


    private float movement;
    public float moveSpeed = 5;
    private bool facingRight = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.isPaused)
        {

            movement = Input.GetAxis("Horizontal");

            if (movement > 0 && !facingRight)
            {
                facingRight = true;
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else if (movement < 0 && facingRight)
            {
                facingRight = false;
                transform.eulerAngles = new Vector3(0, 180, 0);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (countJump == 0)
                {
                    Jump();
                    onGround = false;
                }
                else if (countJump < maxJump)
                {
                    Debug.Log("chaise2");
                    MultipleJump();
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log("chaise");
                Dash();
            }

            if (Time.time - lastDashTime > dashDuration && originalGravity != 0)
            {
                rb.gravityScale = originalGravity;
            }
        }
    }

    private void FixedUpdate()
    {
        transform.position += new Vector3(movement * moveSpeed, 0, 0) * Time.fixedDeltaTime;
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
        countJump++;
        lastJumpTime = Time.time;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            onGround = true;
            countJump = 0;
        }
    }

    void Dash()
    {
        if (Time.time - lastDashTime > dashingCooldown)
        {
            originalGravity = rb.gravityScale;
            if (facingRight)
            {
                rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0);
            }
            else
            {
                rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower * -1, 0);
            }
            lastDashTime = Time.time;
        }
        
    }

    void MultipleJump()
    {
        if (Time.time - lastJumpTime > multipleJumpCooldown)
        {
            Jump();
            countJump++;
        }

    }

}
