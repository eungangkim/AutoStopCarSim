using UnityEngine;

public class RaceTrigger : MonoBehaviour
{
    public enum TriggerType { FinishLine, Checkpoint }
    public TriggerType type;
    public int checkpointID;

    [Header("매니저 연결")]
    public GameManager gameManager;

    void OnTriggerEnter(Collider other)
    {
        PrometeoCarController car = other.GetComponentInParent<PrometeoCarController>();
        if (car == null) return;

        if (type == TriggerType.FinishLine)
        {
            Debug.Log("결승선 통과");
            gameManager.OnCrossFinishLine();
        }
        else
        {
            gameManager.PassCheckpoint(checkpointID);
        }
    }
}