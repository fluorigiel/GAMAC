using UnityEngine;

namespace _Scripts.Player.Movement
{
    [CreateAssetMenu(menuName = "PlayerMovementStats")]
    public class PlayerMovementStats : ScriptableObject
    {

        [Header("Walk")] // Header is an organizer for unity debug (to organize the values that we can tweak values while the game is running)
        // the little f behind the values indicates that those values are of type float (not integers)
        [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f; // Maximum walking speed
        // Range is once again to help beautifully change the value in debug in unity whith a slide from min to max // bruh, it's not working
        [Range(0.25f, 50f)] public float GroundAcceleration = 5f; // To calculate the rate at how fast you attain max speed on the ground
        [Range(0.25f, 50f)] public float GroundDeceleration = 20f; // To calculate the rate at how fast you will completely stop moving on the ground
        [Range(0.25f, 50f)] public float AirAcceleration = 5f; // To calculate the rate at how fast you attain max speed in the air
        [Range(0.25f, 50f)] public float AirDeceleration = 5f; // To calculate the rate at how fast you will stop moving forward in the air
        [Space(5)]

        [Header("Run")]
        [Range(1f, 100f)] public float MaxRunSpeed = 20f; // Maximum running speed
        [Space(5)]

        [Header("Jump")]
        [Range(0f, 100f)] public float JumpHeight = 20f; // Maximum jump height
        [Range(5f, 50f)] public float MaxFallSpeed = 26f; // Maximum speed at which you fall
        [Range(0f, 10f)] public int NumberOfJumpsAllowed = 3; // Number of jump you can perfom (Total of grounded and aerial jumps)
        [Range(0f, 1f)] public float MultipleJumpStrengthPercent = 0.85f; // Height of the jump in the air compared to the grounded one
        //[Range(0f, 5f)] public float FastFallingStrength = 2f; 
        [Space(5)]

        [Header("Wall Slide and Wall Jump")]
        [Range(1f, 20f)] public float WallSlideMaxSpeed = 4f; // Maximum speed at which you slide from walls
        [Range(1f, 50f)] public float WallJumpStrength = 20f; // Strength of the jump perfomed from a wall
        //[Range(0f, 2f)] public float WallJumpTime = 0.2f; // the player can't decide of where to go just after doing a wall jump
        [Space(5)]

        [Header("Short Up / Jump Cancel")]
        [Tooltip("Jump Cancel = Short Up")]
        [Range(0f, 1f)] public float JumpCancelTime = 0.1f;
        [Range(0f, 1f)] public float JumpCancelStrength = 0.5f;
        [Tooltip("Need to be superior to JumpCancelTime")]
        [Range(0f, 1f)] public float JumpCancelMoment = 0.15f;
        [Space(5)]

        [Header("Jump Buffer")]
        [Range(0f, 1.5f)] public float JumpBufferTime = 0.125f; // to store jump input just before the player touch ground to not need to do frame perfect jump when landing
        [Space(5)]

        /*[Header("Jump Coyote Time")]
        [Range(0f, 5f)] public float JumpCoyoteTime = 0.1f; // time after you leave a platform when you are still refered as on the platform
        [Space(5)]*/

        [Header("Dash")]
        [Range(0f, 5f)] public float DashStrength = 3f;
        [Range(0f, 5f)] public float DashTimer = 0.3f;
        [Range(0f, 5f)] public float DashDuration = 0.1f; 
        [Range(0f, 3f)] public float DashBufferTime = 0.125f;
        [Space(5)]

        [Header("Player Gravity")]
        [Range(0f, 50f)] public float GravityForce = 1.5f;
        [Range(0f, 50f)] public float GravityFallForce = 3f;

    }
}
