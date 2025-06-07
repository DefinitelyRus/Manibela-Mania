using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController2D : MonoBehaviour
{
    public float acceleration = 10f;
    public float steering = 100f;
    public float maxSpeed = 20f;
    public float drag = 2f;

    public CameraHandler camHandler;
    private Rigidbody2D rb;


    public SoundData idleSound;
    public SoundData gear1;
    public SoundData gear2;
    public SoundData brakeSound;

    public SoundData crashSound;
    public SoundData gearShiftSound;

    public float[] gearSpeedThresholds = {1f, 30f, 50f}; // Define thresholds between gears
    private int currentGear = 0;
  

    private string currentSoundKey = "";



    private void CrossfadeIfNeeded(string key, SoundData data)
    {
     if (currentSoundKey == key) return;

        currentSoundKey = key;
        SoundManager.Instance.CrossfadeSound(data, 0f); // adjustable fade
    }


    void Update()
    {
         float speed = rb.linearVelocity.magnitude;
    //Debug.Log("Car Speed: " + speed.ToString("F2") + " units/second");

    // Determine the gear based on thresholds
    int newGear = currentGear;
    for (int i = 0; i < gearSpeedThresholds.Length - 1; i++)
    {
        if (speed >= gearSpeedThresholds[i] && speed < gearSpeedThresholds[i + 1])
        {
            newGear = i;
            break;
        }
    }

    // Check if the car surpassed the last threshold (highest gear)
    if (speed >= gearSpeedThresholds[gearSpeedThresholds.Length - 1])
    {
        newGear = gearSpeedThresholds.Length - 1;
    }

    // Trigger gear shift and gear-specific sound if needed
    if (newGear != currentGear)
    {
        currentGear = newGear;
        Debug.Log("Gear changed to: " + currentGear);

        if (gearShiftSound != null)
            TriggerGearShift();

        // Update acceleration sound to match gear
        switch (currentGear)
        {
            case 0:
                CrossfadeIfNeeded("gear1", gear1);
                break;
            case 1:
                CrossfadeIfNeeded("gear1", gear1);
                break;
            case 2:
                CrossfadeIfNeeded("gear2", gear2);
                break;
            default:
                CrossfadeIfNeeded("gear2", gear2); // fallback to highest accel sound
                break;
        }
    }

    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = drag;
        rb.angularDamping = 1f;
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");   // W/S or Up/Down
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right
            //Debug.Log("Move Input: " + moveInput + ", Turn Input: " + turnInput);
        // Limit speed
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.up * moveInput * acceleration);
        }

        // Steering (only while moving)
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float rotation = -turnInput * steering * Time.fixedDeltaTime * Mathf.Sign(moveInput);
            rb.MoveRotation(rb.rotation + rotation);
        }

          UpdateEngineSound(moveInput);
    }


   private void UpdateEngineSound(float moveInput)
{
    float speed = rb.linearVelocity.magnitude;

    if (moveInput == 0)
    {
        CrossfadeIfNeeded("idle", idleSound);
    }
    else if (moveInput < 0)
    {
        CrossfadeIfNeeded("brake", brakeSound);
    }
}
    private void PlaySoundOnce(string key, SoundData data)
    {
        if (currentSoundKey == key) return;

        currentSoundKey = key;
        SoundManager.Instance.PlaySound(data, transform.position);
    }

    public void TriggerGearShift()
    {
        var audioSource = SoundManager.Instance.sfxSource;
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(gearShiftSound.clip, gearShiftSound.volume);
        audioSource.pitch = 1f; // reset to avoid affecting future sounds
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
      
        if (collision.relativeVelocity.magnitude > 3f)
        {
            Camera mainCam  = Camera.main;
            if (camHandler != null)
            {
                camHandler.TriggerShake();
                SoundManager.Instance.PlaySound(crashSound, transform.position);
    }
            else
            {
                Debug.LogError("CameraHandler reference is null.");
            }
            }
        }
    }