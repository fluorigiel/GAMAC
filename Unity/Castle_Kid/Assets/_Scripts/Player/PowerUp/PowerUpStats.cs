using UnityEngine;

[CreateAssetMenu(menuName = "PowerUpStats")]
public class PowerUpStats : ScriptableObject
{
    [Header("Passive Powers")]
    [Range(0, 20)] public int numPassiv = 0;
    [Space(5)]

    [Header("Active Powers")]
    [Range(0, 20)] public int numActive = 0;
    [Space(5)]

    [Header("Conditional Powers")]
    [Range(0, 20)] public int numConditional = 0;
}
