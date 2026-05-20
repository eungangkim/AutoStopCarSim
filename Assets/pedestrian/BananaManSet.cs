using UnityEngine;

public class BananaManSet : MonoBehaviour
{
    [Header("세트id")]
    public int setId = 0;

    [Header("이동 설정")]
    public Pedestrian.MoveDirection moveDirection = Pedestrian.MoveDirection.Left;
    [Tooltip("거리")]
    public float moveDistance = 10f;
    [Tooltip("속도")]
    public float moveSpeed = 3f;

    void Awake()
    {
        foreach (Pedestrian p in GetComponentsInChildren<Pedestrian>())
        {
            p.setId         = setId;
            p.moveDirection = moveDirection;
            p.moveDistance  = moveDistance;
            p.moveSpeed     = moveSpeed;
            p.setTransform  = this.transform;
        }

        foreach (Trigger t in GetComponentsInChildren<Trigger>())
        {
            t.setId = setId;
        }
    }
}
