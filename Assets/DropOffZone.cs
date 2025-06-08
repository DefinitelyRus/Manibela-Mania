using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class DropOffZone : MonoBehaviour
{
    public GameObject timerBarPrefab;  // Assign the TimerBarUI prefab in Inspector
    public float dropOffTimer = 2f;  // How long the player needs to stay stopped
    public float speedThreshold = 0.1f;  // Speed below which we consider the vehicle stopped

    private Dictionary<string, GameObject> activeTimerBars = new Dictionary<string, GameObject>();
    private Dictionary<string, float> passengerTimers = new Dictionary<string, float>();
    private VehicleMovement vehicleMovement;  // Reference to the vehicle's movement script
    private InputManager inputManager;  // Reference to the input manager

    private void Start()
    {
        // Find the vehicle movement script
        vehicleMovement = FindAnyObjectByType<VehicleMovement>();
        if (vehicleMovement == null)
        {
            Debug.LogError("VehicleMovement script not found in scene!");
        }

        // Find the input manager
        inputManager = FindAnyObjectByType<InputManager>();
        if (inputManager == null)
        {
            Debug.LogError("InputManager script not found in scene!");
        }
    }

    private void Update()
    {
        if (vehicleMovement == null || inputManager == null) return;

        // Check each passenger that needs to be dropped off
        foreach (var passenger in Sakay.passengerDropOffs.ToList())
        {
            int remainingAreas = passenger.Value - Sakay.currentArea;
            if (remainingAreas <= 0)
            {
                // Check if vehicle is stopped AND no movement inputs are being pressed
                bool isStopped = Mathf.Abs(vehicleMovement.Speed) < speedThreshold && 
                               !inputManager.IsAccelerating && 
                               !inputManager.IsDecelerating && 
                               !inputManager.IsHandBraking && 
                               Mathf.Approximately(inputManager.Steering, 0f);

                Debug.Log($"Vehicle speed: {vehicleMovement.Speed}, IsStopped: {isStopped}");

                if (isStopped)
                {
                    // Start or continue timer
                    if (!passengerTimers.ContainsKey(passenger.Key))
                    {
                        passengerTimers[passenger.Key] = 0f;
                        // Create timer bar if it doesn't exist
                        if (!activeTimerBars.ContainsKey(passenger.Key))
                        {
                            Debug.Log($"Attempting to create timer bar for {passenger.Key}");
                            CreateTimerBar(passenger.Key);
                        }
                    }

                    passengerTimers[passenger.Key] += Time.deltaTime;

                    // Update timer bar
                    if (activeTimerBars.ContainsKey(passenger.Key))
                    {
                        Slider timerSlider = activeTimerBars[passenger.Key].GetComponentInChildren<Slider>();
                        if (timerSlider != null)
                        {
                            timerSlider.value = passengerTimers[passenger.Key] / dropOffTimer;
                            Debug.Log($"Updated timer bar for {passenger.Key}: {timerSlider.value * 100}%");
                        }
                        else
                        {
                            Debug.LogError($"Timer bar for {passenger.Key} is missing Slider component!");
                            CleanupTimerBar(passenger.Key);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Timer bar for {passenger.Key} not found in activeTimerBars!");
                    }

                    // Check if timer is complete
                    if (passengerTimers[passenger.Key] >= dropOffTimer)
                    {
                        DropOffPassenger(passenger.Key);
                    }
                }
                else
                {
                    // Reset timer if vehicle is moving or inputs are pressed
                    if (passengerTimers.ContainsKey(passenger.Key))
                    {
                        Debug.Log($"Resetting timer for {passenger.Key} - Vehicle moving or inputs pressed");
                        CleanupTimerBar(passenger.Key);
                        passengerTimers.Remove(passenger.Key);
                    }
                }
            }
            else
            {
                Debug.Log($"Passenger {passenger.Key} needs {remainingAreas} more areas to reach destination");
            }
        }
    }

    private void CreateTimerBar(string passengerKey)
    {
        if (timerBarPrefab == null)
        {
            Debug.LogError("Timer bar prefab is not assigned!");
            return;
        }

        try
        {
            GameObject timerBar = Instantiate(timerBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            if (timerBar == null)
            {
                Debug.LogError($"Failed to instantiate timer bar for {passengerKey}");
                return;
            }

            activeTimerBars[passengerKey] = timerBar;
            
            // Make the timer bar face the camera
            if (Camera.main != null)
            {
                timerBar.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }

            // Verify the slider component
            Slider timerSlider = timerBar.GetComponentInChildren<Slider>();
            if (timerSlider == null)
            {
                Debug.LogError($"Timer bar prefab for {passengerKey} is missing a Slider component!");
                CleanupTimerBar(passengerKey);
            }
            else
            {
                timerSlider.value = 0f;
                Debug.Log($"Successfully created timer bar for {passengerKey} with slider value {timerSlider.value}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating timer bar for {passengerKey}: {e.Message}");
            CleanupTimerBar(passengerKey);
        }
    }

    private void CleanupTimerBar(string passengerKey)
    {
        if (activeTimerBars.ContainsKey(passengerKey))
        {
            GameObject timerBar = activeTimerBars[passengerKey];
            if (timerBar != null)
            {
                Debug.Log($"Destroying timer bar for {passengerKey}");
                Destroy(timerBar);
            }
            activeTimerBars.Remove(passengerKey);
        }
    }

    private void DropOffPassenger(string passengerKey)
    {
        Debug.Log($"Passenger {passengerKey} has been dropped off");
        
        // Remove passenger from tracking
        if (Sakay.passengerDropOffs.ContainsKey(passengerKey))
        {
            Sakay.passengerDropOffs.Remove(passengerKey);
        }
        
        if (Sakay.activePassengerTypes.Contains(passengerKey))
        {
            Sakay.activePassengerTypes.Remove(passengerKey);
        }
        
        GlobalCounter.passengernum--;

        // Clear the passenger's seat
        if (PassengerSeating.Instance != null)
        {
            PassengerSeating.Instance.ClearSeat(passengerKey);
        }
        else
        {
            Debug.LogError("PassengerSeating.Instance is null! Cannot clear seat.");
        }

        // Clean up timer bar
        CleanupTimerBar(passengerKey);
        
        if (passengerTimers.ContainsKey(passengerKey))
        {
            passengerTimers.Remove(passengerKey);
        }

        Debug.Log($"Fully cleaned up all references for {passengerKey}");
    }

    private void OnDestroy()
    {
        // Clean up all timer bars when this object is destroyed
        foreach (var timerBar in activeTimerBars.Values)
        {
            if (timerBar != null)
            {
                Destroy(timerBar);
            }
        }
        activeTimerBars.Clear();
        passengerTimers.Clear();
    }
} 