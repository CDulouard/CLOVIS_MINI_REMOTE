using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public List<Servo> lServo;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var servo in lServo)
        {
            servo.Init();
        }
    }

    private void FixedUpdate()
    {
        foreach (var servo in lServo)
        {
            servo.Refresh();
        }
    }
}
