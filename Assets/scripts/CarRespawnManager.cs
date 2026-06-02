using UnityEngine;

public class CarRespawnManager : MonoBehaviour
{
    public GameObject car;
    public Transform respawnPoint;
    public PedestrianScenarioManager pedestrianScenarioManager;
    public PedestrianTrigger pedestrianTrigger;
    private Rigidbody carRigidbody;

    private void Start()
    {
        if (car != null)
        {
            carRigidbody = car.GetComponent<Rigidbody>();
        }
    }

    public void RespawnCar()
    {
        Debug.Log("리스폰 버튼 눌림");

        if (car == null || respawnPoint == null)
        {
            Debug.LogWarning("차량 또는 리스폰 위치가 연결되지 않았습니다.");
            return;
        }

        Rigidbody rb = car.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = car.GetComponentInChildren<Rigidbody>();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = respawnPoint.position;
            rb.rotation = respawnPoint.rotation;
        }
        else
        {
            car.transform.position = respawnPoint.position;
            car.transform.rotation = respawnPoint.rotation;
        }

        if (pedestrianScenarioManager != null)
        {
            pedestrianScenarioManager.ResetSelectedPedestrian();
        }

        if (pedestrianTrigger != null)
        {
            pedestrianTrigger.ResetTrigger();
        }

        Debug.Log("차량 및 보행자 리스폰 완료");
    }
}