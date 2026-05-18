using UnityEngine;

public class PedestrianMover : MonoBehaviour
{
    public float moveSpeed = 1f;
    public Vector3 moveDirection = new Vector3(-1, 0, 2);

    [Header("Move Limit")]
    public float maxMoveDistance = 3f;

    public bool isMoving = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 startPosition;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        startPosition = transform.position;
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        if (!isMoving) return;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float movedDistance = Vector3.Distance(startPosition, transform.position);

        if (movedDistance >= maxMoveDistance)
        {
            StopMove();
        }
    }

    public void StartMove()
    {
        startPosition = transform.position;
        isMoving = true;
    }

    public void StopMove()
    {
        isMoving = false;
    }
    
    public void ResetPedestrian()
    {
        isMoving = false;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        startPosition = initialPosition;
    }
}