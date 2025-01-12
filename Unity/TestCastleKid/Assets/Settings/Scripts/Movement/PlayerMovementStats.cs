using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{

    [Header("Walk")] // Header is an organizer for unity debug (to organize the values that we can tweak values while the game is running)
    // the little f behind the values indicates that those values are of type float (not integers)
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    // Range is once again to help beautifully change the value in debug in unity whith a slide from min to max // bruh, it's not working
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;
    [Space(10)]
    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    [Header("Grounded/Collision Checks")] // need to tweak a little the values so that the player can jump just a little before touching the 
                                          // ground so that it feels more reactive 
    public LayerMask GroundLayer; // (it's like tag) to understand layers : https://www.youtube.com/watch?v=Zn3x48TxQqQ
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetctionRayLength = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;

    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f; // quite clear

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f; // to store jump input just before the player touch ground to not need to do frame perfect jump when landing

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f; // time after you leave a platform when you are still refered as on the platform

    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    public void OnValidate()
    {
        CalculateValues();
    }

    public void OnEnable()
    {
        CalculateValues();
    }

    public void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f); // to understand that formula : https://www.youtube.com/watch?v=hG9SzQxaCm8 ( 4 min 24 )
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex; ;
    }

}
