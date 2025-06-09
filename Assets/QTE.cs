using UnityEngine;
using UnityEngine.UI;

public class QTE : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;        // Speed of hand movement
    public float moveDistance = 8f;     // How far the hand moves
    public float returnSpeed = 8f;      // Speed when returning to start

    [Header("Spawn Position")]
    public Vector2 spawnOffset = Vector2.zero;  // Offset from the QTE object's position
    public bool useCustomSpawnPosition = false; // Whether to use custom spawn position
    public Vector2 customSpawnPosition;         // Custom spawn position if useCustomSpawnPosition is true

    [Header("Timer Settings")]
    public float clickTimeLimit = 2f;   // Time limit to click
    public Slider timerSlider;          // UI Slider to show remaining time
    public float timerBarXOffset = 0f;  // X offset of timer bar from hand in screen space
    public float timerBarYOffset = -50f; // Y offset of timer bar from hand in screen space
    public GameObject handObject;        // The hand sprite to move

    [Header("References")]
    public BoxCollider2D handCollider;   // Collider for the hand

    [Header("Reward Settings")]
    public GameObject[] coinPrefabs;     // Array of different coin prefabs
    public int coinCount = 10;           // Number of coins to spawn
    public float coinSpreadRadius = 2f;  // How far coins spread from the hand
    public float coinForce = 5f;         // Force applied to coins when spawned
    public float vibrationDuration = 0.2f; // How long the hand vibrates
    public float vibrationIntensity = 0.1f; // How intense the vibration is

    [Header("Auto Activation Settings")]
    public float activationInterval = 30f; // Time between possible activations
    public float activationChance = 0.5f;  // Chance of activation (0.5 = 50%)

    private Vector3 startPosition;       // Original position of the hand
    private Vector3 targetPosition;      // Position to move to
    private bool isMoving = false;       // Whether the hand is moving
    private bool isReturning = false;    // Whether the hand is returning
    private bool isTimerActive = false;  // Whether the timer is running
    private float timer = 0f;            // Current timer value
    private float currentMoveDistance = 0f; // Current distance moved
    private RectTransform timerRectTransform; // Reference to the timer's RectTransform
    private Vector3 originalHandPosition; // Store original position during vibration
    private float vibrationTimer = 0f;    // Timer for vibration effect
    private float activationTimer = 0f;   // Timer for auto activation
    private bool isAutoActivationEnabled = true; // Whether auto activation is enabled

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (handObject == null)
        {
            Debug.LogError("[QTE] Hand object not assigned!");
            return;
        }

        // Validate coin prefabs
        if (coinPrefabs == null || coinPrefabs.Length == 0)
        {
            Debug.LogError("[QTE] No coin prefabs assigned!");
            return;
        }

        // Validate collider setup
        if (handCollider == null)
        {
            handCollider = handObject.GetComponent<BoxCollider2D>();
            if (handCollider == null)
            {
                handCollider = handObject.AddComponent<BoxCollider2D>();
                Debug.Log("[QTE] Added BoxCollider2D to hand object");
            }
        }

        // Configure collider
        handCollider.isTrigger = false; // We want physical collision for click detection
        handCollider.size = new Vector2(1f, 1f); // Adjust size to match your hand sprite
        handCollider.offset = Vector2.zero; // Center the collider on the sprite

        // Set the initial position based on settings
        if (useCustomSpawnPosition)
        {
            startPosition = customSpawnPosition;
        }
        else
        {
            startPosition = transform.position + (Vector3)spawnOffset;
        }

        // Set the hand's initial position
        handObject.transform.position = startPosition;
        targetPosition = startPosition + Vector3.left * moveDistance;

        // Initialize timer slider
        if (timerSlider != null)
        {
            timerRectTransform = timerSlider.GetComponent<RectTransform>();
            timerSlider.value = 1f;
            timerSlider.gameObject.SetActive(false);
            // Set initial timer bar position
            UpdateTimerBarPosition();
        }

        // Initialize activation timer
        activationTimer = activationInterval;

        Debug.Log($"[QTE] Hand spawned at position: {startPosition}");
    }

    // Update is called once per frame
    void Update()
    {
        // For testing: Activate QTE when numpad 1 is pressed
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            StartQTE();
        }

        // Handle auto activation
        if (isAutoActivationEnabled && !isMoving && !isReturning && !isTimerActive)
        {
            activationTimer -= Time.deltaTime;
            if (activationTimer <= 0f)
            {
                // Reset timer
                activationTimer = activationInterval;
                
                // 50% chance to activate
                if (Random.value <= activationChance)
                {
                    Debug.Log("[QTE] Auto activation triggered!");
                    StartQTE();
                }
            }
        }

        if (isMoving)
        {
            // Move hand to the left
            float step = moveSpeed * Time.deltaTime;
            handObject.transform.position = Vector3.MoveTowards(handObject.transform.position, targetPosition, step);
            currentMoveDistance = Vector3.Distance(startPosition, handObject.transform.position);

            // Check if we've reached the target position
            if (currentMoveDistance >= moveDistance)
            {
                isMoving = false;
                StartTimer();
            }
        }
        else if (isReturning)
        {
            // Return hand to start position
            float step = returnSpeed * Time.deltaTime;
            handObject.transform.position = Vector3.MoveTowards(handObject.transform.position, startPosition, step);

            // Check if we've returned to start
            if (Vector3.Distance(handObject.transform.position, startPosition) < 0.01f)
            {
                isReturning = false;
                ResetQTE();
            }
        }

        if (isTimerActive)
        {
            // Update timer
            timer -= Time.deltaTime;
            if (timerSlider != null)
            {
                timerSlider.value = timer / clickTimeLimit;
                // Update timer bar position to follow hand
                UpdateTimerBarPosition();
            }

            // Check for click
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (handCollider.OverlapPoint(mousePosition))
                {
                    // Success! Clicked in time
                    Debug.Log("[QTE] Success! Clicked in time.");
                    isTimerActive = false;
                    if (timerSlider != null)
                    {
                        timerSlider.gameObject.SetActive(false);
                    }
                    // Start vibration and spawn coins
                    StartVibration();
                    SpawnCoins();
                    isReturning = true;
                }
            }

            // Check if timer ran out
            if (timer <= 0f)
            {
                // Failed! Timer ran out
                Debug.Log("[QTE] Failed! Timer ran out.");
                isTimerActive = false;
                if (timerSlider != null)
                {
                    timerSlider.gameObject.SetActive(false);
                }
                isReturning = true;
            }
        }

        // Update vibration effect
        if (vibrationTimer > 0)
        {
            vibrationTimer -= Time.deltaTime;
            if (vibrationTimer <= 0)
            {
                // Stop vibration
                handObject.transform.position = originalHandPosition;
            }
            else
            {
                // Apply vibration
                Vector3 randomOffset = new Vector3(
                    Random.Range(-vibrationIntensity, vibrationIntensity),
                    Random.Range(-vibrationIntensity, vibrationIntensity),
                    0
                );
                handObject.transform.position = originalHandPosition + randomOffset;
            }
        }
    }

    private void StartVibration()
    {
        originalHandPosition = handObject.transform.position;
        vibrationTimer = vibrationDuration;
    }

    private void SpawnCoins()
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0)
        {
            Debug.LogWarning("[QTE] No coin prefabs assigned!");
            return;
        }

        for (int i = 0; i < coinCount; i++)
        {
            // Calculate random position within spread radius
            Vector2 randomOffset = Random.insideUnitCircle * coinSpreadRadius;
            Vector3 spawnPosition = handObject.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Randomly select a coin prefab
            int randomIndex = Random.Range(0, coinPrefabs.Length);
            GameObject selectedCoinPrefab = coinPrefabs[randomIndex];

            // Spawn coin
            GameObject coin = Instantiate(selectedCoinPrefab, spawnPosition, Quaternion.identity);

            // Add random force to the coin
            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * coinForce, ForceMode2D.Impulse);
            }
        }
    }

    private void UpdateTimerBarPosition()
    {
        if (timerSlider != null && timerRectTransform != null)
        {
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(handObject.transform.position);
            // Apply offsets in screen space
            screenPos.x += timerBarXOffset;
            screenPos.y += timerBarYOffset;
            // Set the timer bar's position
            timerRectTransform.position = screenPos;
        }
    }

    public void StartQTE()
    {
        if (!isMoving && !isReturning && !isTimerActive)
        {
            isMoving = true;
            currentMoveDistance = 0f;
            Debug.Log("[QTE] Starting QTE sequence");
        }
    }

    private void StartTimer()
    {
        isTimerActive = true;
        timer = clickTimeLimit;
        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(true);
            timerSlider.value = 1f;
            UpdateTimerBarPosition();
        }
        Debug.Log("[QTE] Timer started - Click the hand within 2 seconds!");
    }

    private void ResetQTE()
    {
        isMoving = false;
        isReturning = false;
        isTimerActive = false;
        timer = 0f;
        currentMoveDistance = 0f;
        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(false);
            timerSlider.value = 1f;
        }
        Debug.Log("[QTE] QTE reset - Ready for next activation");
    }

    // Public method to enable/disable auto activation
    public void SetAutoActivation(bool enabled)
    {
        isAutoActivationEnabled = enabled;
        if (enabled)
        {
            activationTimer = activationInterval;
        }
    }
}
