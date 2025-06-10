using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{

public static GameManager Instance { get; private set; }

    public GameObject winPanel;
    public GameObject failPanel;

    public FareManager fareManager;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dayText;


    [Header("Day System")]
    public int currentDay = 1;
    public int startingQuota = 100;
    public int quotaIncrementPerDay = 50;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    [Header("Game Settings")]
    private int targetBalance;
    public float timeLimit = 120f;         // Set game time limit in seconds

    public float timer = 0f;
    public bool gameEnded = false;


    void Start()
    {
    Time.timeScale = 1f;
    void Start()
{
    currentDay = PlayerPrefs.GetInt("CurrentDay", 1); // Should now load 1 if New Game was used
    Debug.Log($"Loaded Day {currentDay} from PlayerPrefs");
    UpdateTargetBalance();
}
      UpdateTargetBalance();

    if (fareManager == null)
        fareManager = FindObjectOfType<FareManager>();
    }


    //recursion solved the UI finding issue thank god
GameObject RecursiveFind(Transform parent, string name)
{
    foreach (Transform child in parent)
    {
        if (child.name == name)
            return child.gameObject;

        GameObject result = RecursiveFind(child, name);
        if (result != null)
            return result;
    }
    return null;
}


public void ReconnectUI()
    // Reconnect UI elements after next day, scene reload or if they were lost
    {

      var canvas = GameObject.Find("Canvas");
    if (canvas != null)
    {
    winPanel = RecursiveFind(canvas.transform, "WinPanel");
    failPanel = RecursiveFind(canvas.transform, "FailPanel");
    }

    quotaText = GameObject.Find("Quota")?.GetComponent<TextMeshProUGUI>();
    timerText = GameObject.Find("Timer")?.GetComponent<TextMeshProUGUI>();
    dayText = GameObject.Find("Day")?.GetComponent<TextMeshProUGUI>();
    fareManager = FindObjectOfType<FareManager>();

    Debug.Log("Reconnecting UI...");
    Debug.LogWarning(winPanel ? "WinPanel found" : "WinPanel not found!");
    Debug.LogWarning(failPanel ? "FailPanel found" : "FailPanel not found!");
    }
   
   
   public void StartNextDay()
{
    currentDay += 1;
    PlayerPrefs.SetInt("CurrentDay", currentDay);
    PlayerPrefs.Save();

    UpdateTargetBalance();
    timer = 0f;
    gameEnded = false;

    if (fareManager != null)
        fareManager.Balance = 0;

    if (winPanel != null) winPanel.SetActive(false);
    if (failPanel != null) failPanel.SetActive(false);

    Time.timeScale = 1f;

    Debug.Log($"Starting Day {currentDay}");
}

public void NewGame()
{
    PlayerPrefs.SetInt("CurrentDay", 1);
    PlayerPrefs.Save();
    
    currentDay = 1;  //Also update the in-memory value
    UpdateTargetBalance();
    timer = 0f;
    gameEnded = false;

    if (fareManager != null)
        fareManager.Balance = 0;

    Debug.Log("New game started. Day reset to 1.");
}


  public void UpdateTargetBalance()
    {
        targetBalance = startingQuota + quotaIncrementPerDay * (currentDay - 1);
    }

    void Update()
    {
        if (gameEnded) return;

        timer += Time.deltaTime;

      
        UpdateQuotaUI();
        UpdateTimerUI();
        CheckWinCondition();

    
     // DEBUG: Press Q to simulate collecting fare
    if (Input.GetKeyDown(KeyCode.Q))
    {
        fareManager.Balance += 25; // simulate P25 fare
        Debug.Log($"[DEBUG] Simulated fare: P25. Total: P{fareManager.Balance}");
    }
    }

    void UpdateQuotaUI()
    {
        if (fareManager == null || quotaText == null) return;

        int current = fareManager.Balance;
        quotaText.text = $"Quota: P{current} / P{targetBalance}";
        
        if (dayText != null)
        dayText.text = $"Day {currentDay}";

    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        float timeLeft = Mathf.Max(0f, timeLimit - timer);
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
    }


   
   void CheckWinCondition()
{
    if (fareManager == null)
    {
        Debug.LogError("fareManager is null in CheckWinCondition");
        return;
    }

    // 🔄 Lazy reconnect if needed
    if (winPanel == null || failPanel == null)
    {
        Debug.LogWarning("UI references lost. Reconnecting...");
        ReconnectUI();
    }

    if (fareManager.Balance >= targetBalance)
    {
        gameEnded = true;
        Debug.Log("Player met quota! WIN!");
        winPanel?.SetActive(true);
        Time.timeScale = 0f;
    }
    else if (timer >= timeLimit)
    {
        gameEnded = true;
        Debug.Log("Time’s up!");
        failPanel?.SetActive(true);
        Time.timeScale = 0f;
    }
}


           
}
