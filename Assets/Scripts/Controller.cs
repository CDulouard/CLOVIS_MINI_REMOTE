using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public List<Servo> lServo;

    private List<string> _motorNames;
    
    // Start is called before the first frame update
    private void Start()
    {
        foreach (var servo in lServo)
        {
            servo.Init();
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
}
