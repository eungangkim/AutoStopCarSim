
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class inputManager : MonoBehaviour
{



    [HideInInspector] public float vertical;
    [HideInInspector] public float horizontal;
    [HideInInspector] public bool handbrake;
    [HideInInspector] public bool boosting;
    
    public InputActionReference moveAction;
    public InputActionReference handbrakeAction;
    public InputActionReference boostAction;
    
    private void OnEnable()
    {
        moveAction.action.Enable();
        handbrakeAction.action.Enable();
        boostAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        handbrakeAction.action.Disable();
        boostAction.action.Disable();
    }

    void Update()
    {
        Vector2 move = moveAction.action.ReadValue<Vector2>();

        horizontal = move.x;
        vertical = move.y;
        handbrake = handbrakeAction.action.IsPressed();
        boosting = boostAction.action.IsPressed();
    }

}
