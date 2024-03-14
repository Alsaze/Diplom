using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MoveObjectWithJoystick : MonoBehaviour
{
    
    
    static Socket listener;
    private CancellationTokenSource source;
    public ManualResetEvent allDone;
    public Rigidbody objectRigidbody;  // Assuming you have a Rigidbody component attached to the object
    private Vector2 leftJoystickInput;
    private Vector2 rightJoystickInput;

    public static readonly int PORT = 1755;
    public static readonly int WAITTIME = 1;

    MoveObjectWithJoystick()
    {
        source = new CancellationTokenSource();
        allDone = new ManualResetEvent(false);
    }

    // Start is called before the first frame update
    async void Start()
    {
        objectRigidbody = GetComponent<Rigidbody>();
        await Task.Run(() => ListenEvents(source.Token));   
    }

    // Update is called once per frame
    void Update()
    {
        // Apply joystick input to move the object
        ApplyJoystickInput();
    }

    private void ListenEvents(CancellationToken token)
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            while (!token.IsCancellationRequested)
            {
                allDone.Reset();

                print("Waiting for a connection... host :" + ipAddress.MapToIPv4().ToString() + " port : " + PORT);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                while (!token.IsCancellationRequested)
                {
                    if (allDone.WaitOne(WAITTIME))
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }

    void ApplyJoystickInput()
    {
        // Adjust the speed factors as needed
        float forwardSpeed = 5.0f;
        float turnSpeed = 2.0f;

        // Combine left and right joystick input for movement
        Vector3 movement = new Vector3(leftJoystickInput.x, 0, leftJoystickInput.y) * forwardSpeed;
        Vector3 turn = new Vector3(0, rightJoystickInput.x, 0) * turnSpeed;

        // Apply combined movement and turn
        objectRigidbody.velocity = movement;
        objectRigidbody.angularVelocity = turn;
        print($"{leftJoystickInput} | {rightJoystickInput}");
    }
    
    void AcceptCallback(IAsyncResult ar)
    {
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        allDone.Set();

        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
    }

    void ReadCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        int read = handler.EndReceive(ar);

        if (read > 0)
        {
            state.inputCode.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

            // Parse joystick input and update the vectors
            string content = state.inputCode.ToString();
            // print($"Read {content.Length} bytes from socket.\n Data : {content}");
            ParseJoystickInput(content);
        }
        else
        {
            handler.Close();
        }
    }

    // Parse joystick input data
    private void ParseJoystickInput(string data)
    {
        print(data);
        string[] values = data.Split(',');
        if (values.Length >= 4)
        {
            float leftX = (float.Parse(values[0]) / 127.5f) - 1.0f;
            float leftY = (float.Parse(values[1]) / 127.5f) - 1.0f;
            float rightX = (float.Parse(values[2]) / 127.5f) - 1.0f;
            float rightY = (float.Parse(values[3]) / 127.5f) - 1.0f;

            leftJoystickInput = new Vector2(leftX, leftY);
            rightJoystickInput = new Vector2(rightX, rightY);
            
            print($" pizda {leftX}-{leftY} | {rightX}-{rightY}");
            print($"{values[0]}|||{values[1]}|||{values[2]}|||{values[3]}");
        }
    }

    private void OnDestroy()
    {
        source.Cancel();
    }

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder inputCode = new StringBuilder();
    }
}
