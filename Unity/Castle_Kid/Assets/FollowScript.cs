using UnityEngine;

public class FollowScript : MonoBehaviour
{

    public float cameraSpeed = 5;
    public float yOffset = 3;
    public Transform target;
    

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x, target.position.y + yOffset, -10);
        transform.position = Vector3.Slerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
    }
}
