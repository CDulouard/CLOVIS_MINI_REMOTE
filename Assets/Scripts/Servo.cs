using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Servo
{
    public string motorName;
    public Transform rotationPoint;
    public Vector3 axis;

    //Pilotage moteur
    public float speed;
    public float offset;
    [Range(-180, 180)] public float targetPos;

    private Quaternion _initialRotation;
    private Vector3 _initialAngle;

    public void Init()
    {
        _initialRotation = rotationPoint.localRotation;
        _initialAngle = _initialRotation.eulerAngles;


        // axis.x = Mathf.Cos(_initialAngle.z) + Mathf.Sin(_initialAngle.z);
        // axis.y = Mathf.Cos(_initialAngle.z) - Mathf.Sin(_initialAngle.z);
    }

    public void Refresh()
    {
        var currentRotation = rotationPoint.localRotation;
        var currentAngle = currentRotation.eulerAngles;
        var targetAngle = axis * (targetPos + offset);

        if (_initialAngle != new Vector3(0, 0, 0))
        {
            // ROTATION Z
            var xComp = Mathf.Cos(_initialAngle.z) * _initialAngle.x + Mathf.Sin(_initialAngle.z) * _initialAngle.y;
            var yComp = Mathf.Cos(_initialAngle.z) * _initialAngle.y - Mathf.Sin(_initialAngle.z) * _initialAngle.x;
            var zComp = 1 * _initialAngle.z;

            var newSpace = new Vector3(xComp, yComp, zComp);


            var targetX = Mathf.Cos(currentAngle.z) * targetAngle.x + Mathf.Sin(currentAngle.z) * targetAngle.x;
            var xRot = targetX - currentAngle.x;
            
            var targetY = Mathf.Cos(xRot) * currentAngle.y + Mathf.Sin(xRot) * currentAngle.z;
            var targetZ = - Mathf.Sin(xRot * currentAngle.y) + Mathf.Cos(xRot) * currentAngle.z;
            
            targetAngle = new Vector3(targetX, targetY, targetZ);
        }


        // Debug.Log(targetAngle);
        // targetAngle.x = targetAngle.x * (float) Math.Cos(_initialRotation.z);

        var targetRotation = new Quaternion {eulerAngles = targetAngle};


        // rotation.eulerAngles = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime * speed) * axis.x,
        //     Mathf.LerpAngle(currentAngle.y, targetAngle.y, Time.deltaTime * speed)* axis.y,
        //     Mathf.LerpAngle(currentAngle.z, targetAngle.z, Time.deltaTime * speed)* axis.z);

        currentRotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * speed);

        rotationPoint.localRotation = currentRotation;
    }
}