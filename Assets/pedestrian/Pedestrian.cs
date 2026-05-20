using UnityEngine;
using System.Collections;


public class Pedestrian : MonoBehaviour
{
    public enum MoveDirection { Left, Right, Forward, Back }

    [Header("Number")]
    public int objectNumber = 1;
    [HideInInspector] public int setId = 0;

    [HideInInspector] public MoveDirection moveDirection = MoveDirection.Left;
    [HideInInspector] public float moveDistance = 10f;
    [HideInInspector] public float moveSpeed    = 3f;
    [HideInInspector] public Transform setTransform;

    public enum State { OnRight, Moving, OnLeft }
    public State currentState { get; private set; } = State.OnRight;

    public void Activate()
    {
        if (currentState != State.OnRight) return;
        currentState = State.Moving;

        Vector3 destination = transform.position + GetLocalDirection() * moveDistance;
        StartCoroutine(SlideTo(destination));
    }

    Vector3 GetLocalDirection()
    {
        Transform basis = setTransform != null ? setTransform : transform;
        switch (moveDirection)
        {
            case MoveDirection.Left:    return -basis.right;
            case MoveDirection.Right:   return  basis.right;
            case MoveDirection.Forward: return  basis.forward;
            case MoveDirection.Back:    return -basis.forward;
            default:                    return -basis.right;
        }
    }

    IEnumerator SlideTo(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = destination;
        currentState = State.OnLeft;
    }
}
