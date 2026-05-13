using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("레이스 설정")]
    public int totalLaps = 3;

    [Header("레이스 UI")]
    public Text lapText;
    public Text currentLapTimeText;
    public Text speedText;

    [Header("메인 패널 (시작/종료 겸용)")]
    public GameObject mainPanel;
    public Text mainCurrentTimeText;
    public Button startButton;

    [Header("일시정지 패널")]
    public GameObject pausePanel;

    [Header("차량")]
    public PrometeoCarController car;

    // 레이스 상태
    private int currentLap = 0;
    private int checkpointsPassed = 0;
    private int totalCheckpoints = 0;
    private bool[] checkpointHit;

    private float totalTime = 0f;
    private bool raceStarted = false;
    private bool raceFinished = false;
    private bool isRacing = false;

    private Rigidbody carRigidbody;

    void Start()
    {
        mainCurrentTimeText.gameObject.SetActive(false);
        carRigidbody = car.GetComponent<Rigidbody>();

        pausePanel.SetActive(false);
        mainPanel.SetActive(true);

        mainCurrentTimeText.text = "--:--.---";
        startButton.GetComponentInChildren<Text>().text = "시작";

        totalCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint").Length;
        checkpointHit = new bool[totalCheckpoints];
        UpdateLapUI();

        StartCoroutine(InitialFreeze());
    }

    System.Collections.IEnumerator InitialFreeze()
    {
        yield return null;
        yield return null;
        car.enabled = false;
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (raceStarted && !raceFinished && isRacing)
        {
            totalTime += Time.deltaTime;
            if (currentLapTimeText != null)
                currentLapTimeText.text = FormatTime(totalTime);
            if (speedText != null)
                speedText.text = Mathf.RoundToInt(Mathf.Abs(car.carSpeed)) + " km/h";
        }

        if (isRacing && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel.activeSelf)
                OnResumeButton();
            else
                ShowPausePanel();
        }
    }

    // === 레이스 로직 ===
    public void OnStartButton()
    {
        if (raceFinished)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        mainPanel.SetActive(false);
        UnfreezeGame();
        isRacing = true;
    }
    public void OnCrossFinishLine()
    {
        if (raceFinished) return;

        if (!raceStarted)
        {
            raceStarted = true;
            currentLap = 1;
            totalTime = 0f;
            ResetCheckpoints();
            UpdateLapUI();
            Debug.Log("시작");
            return;
        }

        if (checkpointsPassed < totalCheckpoints)
        {
            Debug.Log($"({checkpointsPassed}/{totalCheckpoints})");
            return;
        }

        Debug.Log($"Lap {currentLap} 완료: {FormatTime(totalTime)}");

        currentLap++;
        ResetCheckpoints();

        if (currentLap > totalLaps)
        {
            raceFinished = true;
            ShowResult(FormatTime(totalTime));
        }

        UpdateLapUI();
    }

    public void PassCheckpoint(int id)
    {
        if (!raceStarted || raceFinished) return;
        if (id < checkpointHit.Length && !checkpointHit[id])
        {
            checkpointHit[id] = true;
            checkpointsPassed++;
            Debug.Log($"({checkpointsPassed}/{totalCheckpoints})");
        }
    }

    void ResetCheckpoints()
    {
        checkpointsPassed = 0;
        if (checkpointHit != null)
        {
            for (int i = 0; i < checkpointHit.Length; i++)
                checkpointHit[i] = false;
        }
    }

    void UpdateLapUI()
    {
        if (lapText != null)
            lapText.text = raceFinished
                ? $"Lap {totalLaps}/{totalLaps}"
                : $"Lap {Mathf.Max(currentLap, 1)}/{totalLaps}";
    }

    // === 메인 패널 ===

    void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        FreezeGame();
    }

    void ShowResult(string finalTime)
    {
        isRacing = false;
        mainCurrentTimeText.gameObject.SetActive(true);
        mainCurrentTimeText.text = "기록: " + finalTime;
        startButton.GetComponentInChildren<Text>().text = "재시작";
        ShowMainPanel();
    }

    // === 일시정지 패널 ===

    void ShowPausePanel()
    {
        pausePanel.SetActive(true);
        FreezeGame();
    }

    public void OnResumeButton()
    {
        pausePanel.SetActive(false);
        UnfreezeGame();
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // === 게임 정지/해제 ===

    void FreezeGame()
    {
        car.enabled = false;
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        Time.timeScale = 0f;
    }

    void UnfreezeGame()
    {
        Time.timeScale = 1f;
        car.enabled = true;
    }

    public string FormatTime(float t)
    {
        int min = (int)(t / 60);
        int sec = (int)(t % 60);
        int ms = (int)((t * 1000) % 1000);
        return $"{min:00}:{sec:00}.{ms:000}";
    }
}