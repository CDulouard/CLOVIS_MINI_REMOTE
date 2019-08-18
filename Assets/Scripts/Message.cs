using System;
using System.Text;
using UnityEngine;

public class Message
{
    private int _messageId;
    private int _len;
    private int _parity;
    private string _message;

    public Message(int id, string message)
    {
        _messageId = id;
        _message = message;
        _len = message.Length;
        parityCheck();
    }

    private void parityCheck()
    {
        var bytes = Encoding.UTF8.GetBytes(_message);
        var binary = "";
        foreach (var i in bytes)
        {
            binary += Convert.ToString(i, 2);
        }
        _parity = 0;
        foreach (var i in binary)
        {
            _parity ^= int.Parse(i.ToString());
        }
    }

    public string ToJson()
    {
        var json = "{\"id\": ";
        json += _messageId.ToString();
        json += ", \"parity\": ";
        json += _parity.ToString();
        json += ", \"len\": ";
        json += _len;
        json += ", \"message\": \"";
        json += _message;
        json += "\"}";
        return json;
    }
}
