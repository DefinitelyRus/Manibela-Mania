using UnityEngine;

public class DropOff : MonoBehaviour
{
    public PassengerCarrier passengerCarrier;
    public PassengerSeating passengerSeating;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (passengerCarrier == null)
        {
            Debug.LogError("[DropOff] PassengerCarrier reference is missing!");
        }
        if (passengerSeating == null)
        {
            Debug.LogError("[DropOff] PassengerSeating reference is missing!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DropOffPoint"))
        {
            // Get all passengers that need to be dropped off
            foreach (BoardedPassenger passenger in passengerCarrier.Passengers)
            {
                if (passenger.ToDropOff && passenger.FullyPaid)
                {
                    // Clear the passenger's seat
                    passengerSeating.ClearSeat($"Char{passengerCarrier.Passengers.IndexOf(passenger) + 1}");
                    
                    // Remove the passenger from the carrier
                    passengerCarrier.RemovePassenger(passenger);
                    
                    Debug.Log($"[DropOff] Dropped off passenger at seat {passengerCarrier.Passengers.IndexOf(passenger) + 1}");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
