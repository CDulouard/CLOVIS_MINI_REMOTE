using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ConsoleApplication1;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public InputField localIp;
    public InputField localPort;
    public InputField serverIp;
    public InputField serverPort;
    public InputField serverPassword;
    public Text errorDisplayer;
    public Button connectButton;
    public Text connectStatus;
    public List<Selecter> selecters;
    private Dictionary<string, int> _values = new Dictionary<string, int>();
    public static IPEndPoint RemoteUser = null;
    public Controller robotController;

    public static string msg;

    private bool _isRunning;
    private bool _remoteControl = false;

    private UdpSocket _socket;
    private float _lastRefresh;
    private const float DelayRefreshStatus = 0.2f;

    // Start is called before the first frame update
    private void Start()
    {
        _isRunning = true;
        _socket = new UdpSocket();
        connectButton.onClick.AddListener(Connection);
        foreach (var selecter in selecters)
        {
            lock (_values)
            {
                _values.Add(selecter.selecterTag, int.Parse(selecter.textInput.text));
            }
        }

        var dataRefresh = new Thread(SendData);
        dataRefresh.Start();
        errorDisplayer.text = "";
        msg = "";
        _lastRefresh = Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        lock (_values)
        {
            foreach (var selecter in selecters)
            {
                if (!selecter.textInput.isFocused)
                {
                    _values[selecter.selecterTag] = int.Parse(selecter.textInput.text);
                }
            }
        }

        if (RemoteUser != null)
        {
            connectStatus.color = Color.green;
            connectStatus.text = "Connected";
            if (_remoteControl)
            {
                RefreshStatus();
            }
        }
        else
        {
            connectStatus.color = Color.red;
            connectStatus.text = "Disconnected";
        }

        errorDisplayer.text = msg;
    }

    private void OnApplicationQuit()
    {
        if (_socket.IsActive)
        {
            _socket.Stop();
        }

        _isRunning = false;
    }

    private void Connection()
    {
        try
        {
            _socket.Start(localIp.text, int.Parse(localPort.text));
            _socket.SendTo(serverIp.text, int.Parse(serverPort.text),
                Message.CreateOldConnectionMessage(serverPassword.text, int.Parse(localPort.text), true)
                    .ToJson());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void SendData()
    {
        while (_isRunning)
        {
            if (RemoteUser != null || _remoteControl)
            {
                // _socket.Send(5, ValuesToJson());
                lock (_values)
                {
                    _socket.SendTo(RemoteUser, Message.CreateOldCommandsMessage(_values).ToJson());
                }

                Thread.Sleep(200);
            }
            else if (!_remoteControl)
            {
                lock (_values)
                {
                    robotController.UpdatePos(
                        robotController.ConvertRealPosToSimulationPos(ConvertDictIntToFloat(_values)));
                }
                Thread.Sleep(10);
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }

    // private string ValuesToJson()
    // {
    //     var json = "{";
    //     lock (_values)
    //     {
    //         foreach (var i in _values)
    //         {
    //             json = $"{json}\\\"{i.Key}\\\" : {i.Value}, ";
    //         }
    //     }
    //
    //     json = json.Remove(json.Length - 2);
    //     json += "}";
    //     return json;
    // }

    private void RefreshStatus()
    {
        if (Time.time - _lastRefresh >= DelayRefreshStatus && RemoteUser != null)
        {
            _socket.SendTo(RemoteUser, Message.CreatOldRefreshStatusMessage().ToJson());
        }

        if (Time.time - _lastRefresh >= 0.1f && RemoteUser != null)
        {
            robotController.UpdatePos(robotController.ConvertRealPosToSimulationPos(_socket.GetLastPos()));
        }
    }

    public Dictionary<string, float> ConvertDictIntToFloat(Dictionary<string, int> toConvert)
    {
        var toReturn = new Dictionary<string, float>();
        foreach (var key in toConvert.Keys)
        {
            toReturn[key] = (float) toConvert[key];
        }

        return toReturn;
    }

    public void SetRemoteControl(bool value)
    {
        _remoteControl = value;
    }

    public bool GetRemoteControl()
    {
        return _remoteControl;
    }

    public void ChangeRemoteControl()
    {
        _remoteControl = !_remoteControl;
    }
}