using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public InputField localIP;
    public InputField localPort;
    public InputField serverIP;
    public InputField serverPort;
    public InputField serverPassword;
    public Text errorDisplayer;
    public Button connectButton;
    public Text connectStatus;
    public  List<Selecter> selecters;
    private Dictionary<string, int> _values = new Dictionary<string, int>();

    private bool _isRunning;
    private UdpSocket _socket; 
    // Start is called before the first frame update
    private void Start()
    {
        _isRunning = true;
        _socket = new UdpSocket();
        connectButton.onClick.AddListener(Connection);
        foreach (var selecter in selecters)
        {
            _values.Add(selecter.selecterTag, int.Parse(selecter.textInput.text));
        }
        var dataRefresh = new Thread(SendData);
        dataRefresh.Start();
        errorDisplayer.text = "";
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (var selecter in selecters)
        {
            _values[selecter.selecterTag] = int.Parse(selecter.textInput.text);
        }

        if (_socket.IsConnected)
        {
            connectStatus.color = Color.green;
            connectStatus.text = "Connected";
        }
        else
        {
            connectStatus.color = Color.red;
            connectStatus.text = "Disconnected";
        }
        
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
        _socket.Start(localIP.text, int.Parse(localPort.text));
        _socket.ConnectionToServer(serverIP.text, int.Parse(serverPort.text), serverPassword.text);
    }

    private void SendData()
    {
        while (_isRunning)
        {
            if (!_socket.IsConnected || !_socket.IsActive) continue;
            _socket.Send(ValuesToJson());
            Thread.Sleep(200);
        }
    }

    private string ValuesToJson()
    {
        var json = "{";
        foreach (var i in _values)
        {
            json = $"{json}\" {i.Key}\" : {i.Value}, ";
        }
        json = json.Remove(json.Length - 2);
        json += "}";
        return json;
    }
}
