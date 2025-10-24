using UnityEngine;

public class StopWatch
{
    private float _startTime;
    private float _elapsedTime;
    private bool _isTiming;
    
    public void Start()
    {
        _startTime = Time.time;
        _isTiming = true;
    }

    public float Stop()
    {
        if (_isTiming)
        {
            _elapsedTime = Time.time - _startTime;
            _isTiming = false;
        }
        return _elapsedTime;
    }

    public float GetElapsedTime()
    {
        return _isTiming ? Time.time - _startTime : _elapsedTime;
    }
}
