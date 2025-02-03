using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class FollowScript : NetworkBehaviour
{
    public float cameraSpeed = 7;
    public float yOffset = 3;
    private Vector3 posParent;
    private Vector3 newPos;
    private Quaternion myRotation;
    

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
        else
        {
            myRotation = transform.rotation;
            newPos = transform.position;
        }
       
    }

    void Update()
    {
        transform.rotation = myRotation;
        posParent = transform.parent.transform.position;

        newPos = Vector3.Slerp(new Vector3(newPos.x, newPos.y, -10), new Vector3(posParent.x, posParent.y, -10), cameraSpeed * Time.deltaTime);
        transform.position = new Vector3(newPos.x, newPos.y + yOffset, -10);
    }
}

/*
 
    public float cameraSpeed = 7;
    public float yOffset = 3;
    private Transform posParent;
    private Quaternion myRotation;

    private void Awake()
    {
        posParent = transform.parent.transform;
        myRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = myRotation;

        Vector3 newPos = new Vector3(posParent.position.x, posParent.position.y + yOffset, -10);
        transform.position = Vector3.Slerp(transform.position, newPos, cameraSpeed * Time.deltaTime);

        //transform.position = new Vector3(posParent.position.x, posParent.position.y, -10);
    }
 */