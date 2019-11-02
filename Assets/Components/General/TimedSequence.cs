using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSequence : Sequence
{
    [SerializeField] float Duration = 1f;

    new protected IEnumerator DoRun()
    {
        /*
         * Do any custom activities here 
         */
        yield return new WaitForSeconds(Duration);
    }
}
