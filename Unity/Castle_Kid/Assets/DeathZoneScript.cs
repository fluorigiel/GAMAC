using UnityEngine;
using UnityEngine.EventSystems;

public class DeathZoneScript : MonoBehaviour
{
    void onTriggerEnter2D (Collider2D collision){
        Debug.Log("Trigger!");
    }
}


