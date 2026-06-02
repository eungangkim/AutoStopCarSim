using UnityEngine;

public class PedestrianTrigger : MonoBehaviour
{
    public PedestrianScenarioManager scenarioManager;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger 들어온 오브젝트 이름: " + other.gameObject.name);
        Debug.Log("Trigger 들어온 오브젝트 태그: " + other.gameObject.tag);
        Debug.Log(other.CompareTag("Car"));
        if (triggered) return;

        if (other.CompareTag("Player") || other.CompareTag("Car"))
        {
            triggered = true;
            Debug.Log("TriggerSelectedPedestrian 진입");
            scenarioManager.TriggerSelectedPedestrian();
        }
    }
    public void ResetTrigger()
    {
        triggered = false;
        Debug.Log("보행자 트리거 초기화 완료");
    }
}