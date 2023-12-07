using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovment : MonoBehaviour
{
    private Rigidbody _rb;

    [SerializeField] private List<GameObject> propellers;

    private Controls _controls;
    private Vector2 _leftStickVector;
    private Vector2 _rifgtStickVector;
    
    private float _targetAngleY; // Целевой угол наклона
    private float _currentAngleY; // Текущий угол наклона

    private void Awake()
    {
        _controls = new Controls();
        _controls.Main.LeftStick.performed += ctx => _leftStickVector = ctx.ReadValue<Vector2>();
        _controls.Main.LeftStick.canceled += ctx => _leftStickVector = Vector2.zero;

        _controls.Main.RightStick.performed += ctx => _rifgtStickVector = ctx.ReadValue<Vector2>();
        _controls.Main.RightStick.canceled += ctx => _rifgtStickVector = Vector2.zero;

        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _controls.Main.Enable();
    }

    private void OnDisable()
    {
        _controls.Main.Disable();
    }

    private void FixedUpdate()
    {
        Movment();
    }

    private void Movment()
    {
        MoveUpDown();
        MoveForward();
        MoveLeftRight();
        MoveRotation();
    }

    private void MoveUpDown()
    {
        float speed = 40f;
        float speedInput = speed * _leftStickVector.y;

        float soaring = 24.6f; //парение в воздухе
        EngionTraction(new List<float>() { soaring, soaring, soaring, soaring });

        if (_leftStickVector.y >= 0)
        {
            EngionTraction(new List<float>() { speedInput, speedInput, speedInput, speedInput });
        }
        else
        {
            EngionTraction(new List<float>() { 0, 0, 0, 0 });
        }
    }

    private void MoveForward()
    {
        //speed propeller
        float speed = 40f;
        float speedPropeller = speed * _rifgtStickVector.y;

        //speed secondary propeller
        float ratio = 0.90f;
        float speedPropellerSecondary = speedPropeller * ratio;

        //forward
        if (_rifgtStickVector.y >= 0)
        {
            EngionTraction(new List<float>()
                { speedPropeller, speedPropeller, speedPropellerSecondary, speedPropellerSecondary });
        }

        //backward
        if (_rifgtStickVector.y <= 0)
        {
            EngionTraction(new List<float>()
                { -speedPropellerSecondary, -speedPropellerSecondary, -speedPropeller, -speedPropeller });
        }
    }

    private void MoveLeftRight()
    {
        //speed propeller
        float speed = 40f;
        float speedPropeller = speed * _rifgtStickVector.x;

        //speed secondary propeller
        float ratio = 0.90f;
        float speedPropellerSecondary = speedPropeller * ratio;

        //forward
        if (_rifgtStickVector.x >= 0)
        {
            EngionTraction(new List<float>()
                { speedPropellerSecondary, speedPropeller, speedPropellerSecondary, speedPropeller });
        }

        //backward
        if (_rifgtStickVector.x <= 0)
        {
            EngionTraction(new List<float>()
                { -speedPropeller, -speedPropellerSecondary, -speedPropeller, -speedPropellerSecondary });
        }
    }

    private void MoveRotation()
    {
        _rb.AddTorque(transform.up * _leftStickVector.x);
    }

    private void EngionTraction(List<float> engine)
    {
        for (int i = 0; i < propellers.Count; i++)
        {
            Vector3 propellerPosition = propellers[i].transform.position;
            //calculate force direction 
            Vector3 propellersBottomPoints = propellers[i].transform.Find("BottomPoint").transform.position;
            Vector3 forceDirection = new Vector3(propellerPosition.x - propellersBottomPoints.x,
                propellerPosition.y - propellersBottomPoints.y,
                propellerPosition.z - propellersBottomPoints.z);

            _rb.AddForceAtPosition(forceDirection.normalized * engine[i], propellerPosition);
        }
    }
}