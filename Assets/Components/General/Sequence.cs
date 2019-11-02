using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] bool RunOnStart = false;
    [SerializeField] bool Restartable = false;

    [Header("Status")]
    [SerializeField] bool running = false;

    #region Events
    public delegate void SequenceEvent();
    public SequenceEvent OnEnter;
    public SequenceEvent OnCancel;
    public SequenceEvent OnExit;
    private SequenceEvent OnEndCallback;
    #endregion


    private void Start()
    {
        if (RunOnStart)
        {
            Run(null);
        }
    }
    private void OnDisable()
    {
        TryCancel();
    }
    public void TryCancel()
    {
        if (running)
        {
            StopCoroutine(DoRunSkelleton(OnEndCallback));
            running = false;
            OnCancel?.Invoke();
        }
        OnEndCallback?.Invoke();
    }
    public void Run(SequenceEvent callback)
    {
        if(running && Restartable)
        {
            TryCancel();
        }
        if(!running)
        {
            StartCoroutine(DoRunSkelleton(callback));
        }
    }

    private IEnumerator DoRunSkelleton(SequenceEvent callback)
    {
        if (!running)
        {
            OnEndCallback = callback;

            running = true;
            OnEnter?.Invoke();

            yield return DoRun();

            OnExit?.Invoke();
            callback?.Invoke();
            running = false;
        }
    }

    protected virtual IEnumerator DoRun() {
        yield return null;
    }
}
