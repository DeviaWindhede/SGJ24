using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public delegate void OnTimerExpired();
    public OnTimerExpired TimerExpired;

    private float _timeRemaining;
    private bool _isRunning;

    public float TimeRemaining => _timeRemaining;
    public bool IsExpired => _timeRemaining <= 0;

    public void StartTimer(float aInitalValue)
    {
        _timeRemaining = aInitalValue;
        _isRunning = true;
    }

    void Update()
    {
        if (!_isRunning) { return; }

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining > 0) { return; }

        TimerExpired?.Invoke();

        Debug.Log("Timer has run out!");
        _isRunning = false;
    }
}
