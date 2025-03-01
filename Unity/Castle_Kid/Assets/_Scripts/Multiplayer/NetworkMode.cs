using UnityEngine;
using Unity.Netcode;

public class NetworkMode : MonoBehaviour
{

    public static void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public static void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    
}