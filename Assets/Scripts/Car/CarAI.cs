using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : Particle
{
    [Header("AI")]
    [SerializeField] Car car;

    [SerializeField] Vector3[] checkpoints;
    [SerializeField] bool loop = true;
    public int currentCheckpoint = 0;
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    Vector3 initPosition;
    Vector3 checkpoint;
    protected override void DoCreate()
    {
        currentCheckpoint = 0;
        initPosition = this.transform.position;
        checkpoint = checkpoints[currentCheckpoint] + initPosition;
    }

    protected override void DoDestroy()
    {

    }

    protected override void DoTick()
    {
        if ((this.transform.position - checkpoint).sqrMagnitude < 0.1f)
        {
            ++currentCheckpoint;
            if (currentCheckpoint > checkpoints.Length)
            {
                if (loop)
                {
                    currentCheckpoint = 0;
                    checkpoint = checkpoints[currentCheckpoint] + initPosition;
                }
                else
                {
                    Destroy();
                }
            }
            else
            {
                checkpoint = checkpoints[currentCheckpoint] + initPosition;
            }
        }
        if (!Destroyed)
        {
            car.MoveTowards(checkpoint);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        int prev = 0;
        Vector3 position = Application.isPlaying? initPosition: this.transform.position;
        for (int current = 1; current < checkpoints.Length; ++current)
        {
            Gizmos.DrawLine(position+checkpoints[prev], position+checkpoints[current]);
            prev = current;
        }
        if(loop)
        {
            Gizmos.DrawLine(position+checkpoints[prev], position+checkpoints[0]);
        }
    }
}
