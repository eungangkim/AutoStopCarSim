using UnityEngine;
using TMPro;

public class PedestrianScenarioManager : MonoBehaviour
{
    public TMP_InputField distanceInput;

    public PedestrianMover pedestrian10m;
    public PedestrianMover pedestrian20m;
    public PedestrianMover pedestrian30m;
    public PedestrianMover pedestrian40m;
    public PedestrianMover pedestrian50m;
    public PedestrianMover pedestrian60m;
    public PedestrianMover pedestrian70m;
    public PedestrianMover pedestrian80m;
    public PedestrianMover pedestrian90m;
    public PedestrianMover pedestrian100m;

    private PedestrianMover selectedPedestrian;

    private void Start()
    {
        distanceInput.text = "30";
        selectedPedestrian = pedestrian30m;

        Debug.Log("기본값: 30m 사람 선택됨");
    }
    public void SelectDistance()
    {
        int distance;

        if (!int.TryParse(distanceInput.text, out distance))
        {
            Debug.Log("숫자를 입력하세요.");
            return;
        }
        Debug.Log(distance);
        selectedPedestrian = null;

        switch (distance)
        {
            case 10:
                selectedPedestrian = pedestrian10m;
                break;
            case 20:
                selectedPedestrian = pedestrian20m;
                break;
            case 30:
                selectedPedestrian = pedestrian30m;
                break;
            case 40:
                selectedPedestrian = pedestrian40m;
                break;
            case 50:
                selectedPedestrian = pedestrian50m;
                break;
            case 60:
                selectedPedestrian = pedestrian60m;
                break;
            case 70:
                selectedPedestrian = pedestrian70m;
                break;
            case 80:
                selectedPedestrian = pedestrian80m;
                break;
            case 90:
                selectedPedestrian = pedestrian90m;
                break;
            case 100:
                selectedPedestrian = pedestrian100m;
                break;
            default:
                Debug.Log("10, 20, 30 ... 100 중 하나를 입력하세요.");
                break;
        }
        if (selectedPedestrian != null)
        {
            HideAllPedestrians();
            selectedPedestrian.gameObject.SetActive(true);
            Debug.Log(distance + "m 사람 선택됨");
        }
    }

    public void TriggerSelectedPedestrian()
    {
        Debug.Log("TriggerSelectedPedestrian");
        if (selectedPedestrian != null)
        {
            selectedPedestrian.StartMove();
        }
        else
        {
            Debug.Log("선택된 사람이 없습니다.");
        }
    }
    private void HideAllPedestrians()
    {
        pedestrian10m.gameObject.SetActive(false);
        pedestrian20m.gameObject.SetActive(false);
        pedestrian30m.gameObject.SetActive(false);
        pedestrian40m.gameObject.SetActive(false);
        pedestrian50m.gameObject.SetActive(false);
        pedestrian60m.gameObject.SetActive(false);
        pedestrian70m.gameObject.SetActive(false);
        pedestrian80m.gameObject.SetActive(false);
        pedestrian90m.gameObject.SetActive(false);
        pedestrian100m.gameObject.SetActive(false);
    }
    public void ResetSelectedPedestrian()
    {
        pedestrian10m.ResetPedestrian();
        pedestrian20m.ResetPedestrian();
        pedestrian30m.ResetPedestrian();
        pedestrian40m.ResetPedestrian();
        pedestrian50m.ResetPedestrian();
        pedestrian60m.ResetPedestrian();
        pedestrian70m.ResetPedestrian();
        pedestrian80m.ResetPedestrian();
        pedestrian90m.ResetPedestrian();
        pedestrian100m.ResetPedestrian();
    }
}