using UnityEngine;
using TMPro;

public class Clock : MonoBehaviour
{
    public TextMeshProUGUI timeDisplay;
    public float timeMultiplier = 1f;
    private float timeElapsed = 0f;

    void Update()
    {
        timeElapsed += Time.deltaTime * timeMultiplier;

        int totalMinutes = Mathf.FloorToInt(timeElapsed);

        int currentHour = 12 + (totalMinutes / 60) % 12;
        int currentMinute = totalMinutes % 60;

        if (currentHour > 12)
        {
            currentHour -= 12;
        }

        string period = (totalMinutes < 7 * 60) ? "PM" : "AM";

        int currentSecond = Mathf.FloorToInt((timeElapsed % 1) * 60);

        timeDisplay.text = $"{currentHour}:{currentMinute:00} {period}";

        if (totalMinutes >= 7 * 60)
        {
            timeDisplay.text = "7:00 AM";
        }
    }
    public void ResetClock()
    {
        timeElapsed = 0f;
        timeDisplay.text = "12:00 PM";
    }
}