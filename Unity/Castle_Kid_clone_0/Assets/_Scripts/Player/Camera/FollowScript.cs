using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class FollowScript : NetworkBehaviour
{

    public float cameraSpeed = 7;
    public float yOffset = 3;
    private Vector3 posParent;
    private Quaternion myRotation;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) Destroy(this);
        myRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = myRotation;
        posParent = transform.parent.transform.position;

        // Vector3 newPos = new Vector3(_player.x, _player.y + yOffset, -10);
        // transform.position = Vector3.Slerp(transform.position, newPos, cameraSpeed * Time.deltaTime);

        Vector3 newPos = Vector3.Slerp(new Vector3(transform.position.x, transform.position.y, -10), new Vector3(posParent.x, posParent.y, -10), cameraSpeed * Time.deltaTime);
        transform.position = new Vector3(newPos.x, newPos.y, -10);
        
        //Debug.Log($"{posParent.x}, {posParent.y}, {posParent.z}");
    }
    // public float cameraSpeed = 7;
    // public float yOffset = 3;

    // // public int numPlayer = 1;
    // private Vector3 _player;

    // //private Camera _camera;

    // public override void OnNetworkSpawn()
    // {
    //     if (!IsOwner) Destroy(this); //If you are not the owner this script is destroyed, to prevent other to control the same thing
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     _player = transform.parent.transform.position;
    //     Vector3 newPos = new Vector3(_player.x, _player.y + yOffset, -10);
    //     transform.position = Vector3.Slerp(transform.position, newPos, cameraSpeed * Time.deltaTime);

    //     // Vector3 newPos = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, 0), new Vector3(_player.x, _player.y, 0), cameraSpeed * Time.deltaTime);
    //     // transform.position = new Vector3(newPos.x, newPos.y, -10);
    // }
}
