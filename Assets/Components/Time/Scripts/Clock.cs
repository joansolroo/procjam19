using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    #region Time
    [Header("Time")]
    [SerializeField] public int BPM = 120;
    float beatDuration;
    [SerializeField] int beats = 4;
    #endregion

    public float time = 0;
    #region Status
    [Header("Status")]
    [SerializeField] public int globalBeat = -1;
    [SerializeField] public int beat = -1;
    [SerializeField] float totalTime;
    [SerializeField] float expectedTime;
    [SerializeField] float error;
    #endregion

    #region Events
    public delegate void ClockEvent();
    public ClockEvent OnMinorTick;
    public ClockEvent OnMayorTick;
    public ClockEvent OnTick;
    #endregion

    private void Start()
    {
        StartCoroutine(DoTicking());
    }

    

    IEnumerator DoTicking()
    {
        WaitForSecondsRealtime wait;
        globalBeat = 0;
        beat = 0;
        float currentTime = Time.time;
        float startTime = currentTime;
        while (true)
        {
            beatDuration = 60f / BPM;
            OnTick?.Invoke();
            if (beat % beats == 0)
            {
                OnMayorTick?.Invoke();
            }
            else
            {
                OnMinorTick?.Invoke();
            }
            totalTime = Time.time - startTime;
            expectedTime = beatDuration * globalBeat;
            error = expectedTime- totalTime;
            float waitTime = beatDuration+error;
            wait = new WaitForSecondsRealtime(waitTime);
            yield return wait;
            ++globalBeat;
            beat = globalBeat % beats;
        }
    }
}
