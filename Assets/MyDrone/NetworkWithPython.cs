using Fleck;
using UnityEngine;

public class NetworkWithPython : MonoBehaviour
{
    public Vector2 leftJoystickInput;
    public Vector2 rightJoystickInput;
    
    private WebSocketServer _ws;
    
    async void Start()
    {
        _ws = new WebSocketServer("ws://127.0.0.1:8080");
        _ws.Start(socket =>
        {
            socket.OnMessage = ParseJoystickInput;
        });
    }
    private void ParseJoystickInput(string data)
    {
        var values = data.Split(';');
        
        leftJoystickInput = new Vector2(float.Parse(values[0]), float.Parse(values[1]));
        rightJoystickInput = new Vector2(float.Parse(values[2]), float.Parse(values[3]));
    }

    private void OnDestroy()
    {
        _ws.Dispose();
    }
}