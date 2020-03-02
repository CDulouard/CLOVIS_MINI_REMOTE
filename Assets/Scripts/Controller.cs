using System;
using System.Collections;
using System.Collections.Generic;
using ConsoleApplication1;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public List<Servo> lServo;

    private Dictionary<string, Servo> _motorDict = new Dictionary<string, Servo>();


    // Start is called before the first frame update
    private void Start()
    {
        foreach (var servo in lServo)
        {
            servo.Init();
            _motorDict[servo.motorName] = servo;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        foreach (var servo in lServo)
        {
            servo.Refresh();
        }
    }

    public void UpdatePos(Dictionary<string, float> posDict)
    {
        foreach (var key in posDict.Keys)
        {
            lock (_motorDict)
            {
                if (_motorDict.ContainsKey(key))
                {
                    _motorDict[key].targetPos = posDict[key];
                }
            }
        }
    }


    public Dictionary<string, float> ConvertRealPosToSimulationPos(Dictionary<string, float> posDict)
    {
        var dictToReturn = new Dictionary<string, float>();
        foreach (var key in posDict.Keys)
        {
            if (_motorDict.ContainsKey(key))
            {
                dictToReturn[key] = posDict[key];
            }
            else if (key == "rKneeRX")
            {
                dictToReturn["rKneeRX_part1"] = posDict[key];
                dictToReturn["rKneeRX_part2"] = posDict[key];
            }
            else if (key == "lKneeRX")
            {
                dictToReturn["lKneeRX_part1"] = posDict[key];
                dictToReturn["lKneeRX_part2"] = posDict[key];
            }
        }

        return dictToReturn;
    }
}