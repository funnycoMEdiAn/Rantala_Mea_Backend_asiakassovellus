using TMPro;
using UnityEngine;

// Main class for managing clicks
public class Manager : MonoBehaviour
{
    // Reference to the TextMeshProUGUI element displaying total clicks
    public TextMeshProUGUI ClicksTotalText;

    // Variable to store the total number of clicks
    float TotalClicks;

    // Method to add clicks and update the UI
    public void AddClicks()
    {
        // Increment the total number of clicks
        TotalClicks++;

        // Update the TextMeshProUGUI element to display the new total clicks
        ClicksTotalText.text = TotalClicks.ToString("0");
    }

}
