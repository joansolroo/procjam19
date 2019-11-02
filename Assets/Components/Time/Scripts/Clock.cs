using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    #region Time
    [Header("Time")]
    [SerializeField] int BPM = 120;
    float beatDuration;
    [SerializeField] int beats = 4;
    #endregion

    #region Status
    [Header("Status")]
    [SerializeField] int globalBeat = -1;
    [SerializeField] int beat = -1;
    #endregion

    #region Events
    public delegate void ClockEvent();
    public ClockEvent OnMinorTick;
    public ClockEvent OnMayorTick;
    public ClockEvent OnTick;
    #endregion

    private void Start()
    {
        beatDuration = 60f / BPM;
        StartCoroutine(DoTicking());
    }

    IEnumerator DoTicking()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(beatDuration);
        
        while (true)
        {
            OnTick?.Invoke();
            if (beat % beats == 0)
            {
                OnMayorTick?.Invoke();
            }
            else
            {
                OnMinorTick?.Invoke();
            }

            if (wait.waitTime != beatDuration)
            {
                wait = new WaitForSecondsRealtime(beatDuration);
            }
            yield return wait;

            ++globalBeat;
            beat = globalBeat % beats;
        }
    }
}
