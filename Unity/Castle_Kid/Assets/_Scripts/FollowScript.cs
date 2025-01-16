using UnityEngine;

public class FollowScript : MonoBehaviour
{

    public float cameraSpeed = 7;
    public float yOffset = 3;
    private GameObject player;
    public int numPlayer = 1;

    private void Awake()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[numPlayer - 1];    
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(player.transform.position.x, player.transform.position.y + yOffset, -10);
        transform.position = Vector3.Slerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
    }
}
