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
    public City city;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    Vector3 initPosition;
    Vector3 checkpoint;
    bool randomWalk = true;
    GraphSparse<Vector3>.Node current;
    GraphSparse<Vector3>.Node next;
    protected override void DoCreate()
    {
        /*currentCheckpoint = 0;
        initPosition = this.transform.position;
        checkpoint = checkpoints[currentCheckpoint] + initPosition;*/
        current = city.carRoads.nodes[Random.Range(0, city.carRoads.nodes.Count)];
        next = city.carRoads.nodes[current.links[Random.Range(0,current.links.Count)].to];
        this.transform.position = current.data;
        checkpoint = next.data;
    }

    protected override void DoDestroy()
    {

    }

    protected override void DoTick()
    {
        if ((this.transform.position - checkpoint).sqrMagnitude < 0.1f)
        {
            if (randomWalk) {
                int previousId = current.id;
                current = next;
                int nextId = previousId;
                do
                {
                    nextId = Random.Range(0, current.links.Count);
                } while (current.links.Count > 1 && nextId == previousId);

                Debug.Log("From:" + current.id + ", goto:" + nextId + "/" + current.links.Count);
                next =city.carRoads.nodes[current.links[nextId].to];
                checkpoint = next.data;
            }
            else
            {
                ++currentCheckpoint;
                if (currentCheckpoint >= checkpoints.Length)
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
