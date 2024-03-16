using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class VideoServer : MonoBehaviour
{
    private Texture2D texture;
    public Camera mainCamera;
    private Socket socket;
    private IPEndPoint endPoint;
    
    WaitForSeconds waitTime = new WaitForSeconds(0.1F);
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    void Start()
    {
        
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
    }

    private void FixedUpdate()
    {
        StartCoroutine(nameof(TakeSnapshot));
    }

    public IEnumerator TakeSnapshot()
    {
        yield return waitTime;
        yield return frameEnd;
        
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.LoadRawTextureData(texture.GetRawTextureData());
        texture.Apply();

        byte[] data = texture.EncodeToJPG(20);
        socket.SendTo(data, endPoint);
    }
}