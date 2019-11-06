using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : Particle
{
    [Header("links")]
    [SerializeField] Car car;
    public City city;

    [Header("AI")]
    [SerializeField] float checkpointMinDistance = 1f;
    [SerializeField] bool fancySteer = false;
    [SerializeField] bool randomWalk = true;
    GraphSparse<Vector3>.Node current;
    GraphSparse<Vector3>.Node next;

    [SerializeField] Vector3[] checkpoints;
    [SerializeField] bool loop = true;
    public int currentCheckpoint = 0;
    Vector3 initPosition;
    Vector3 checkpoint;

    [Header("Status")]
    [SerializeField]  float gas = 0;
    [SerializeField]  float steer = 0;
    [SerializeField] float steerUnclamped = 0;
    [SerializeField] float vertical = 0;
    public Vector3 forwardPlane;
    public Vector3 targetPlane;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    protected override void DoCreate()
    {
        if (randomWalk)
        {
            current = city.carRoads.nodes[Random.Range(0, city.carRoads.nodes.Count)];
            next = city.carRoads.nodes[current.links[Random.Range(0, current.links.Count)].to];
            this.transform.position = current.data;
            checkpoint = next.data;
        }
        else
        {
            currentCheckpoint = 0;
            initPosition = this.transform.position;
            checkpoint = checkpoints[currentCheckpoint] + initPosition; 
        }
        this.transform.LookAt(checkpoint);
    }

    protected override void DoDestroy()
    {

    }

    
    protected override void DoTick()
    {
        if ((this.transform.position - checkpoint).sqrMagnitude < checkpointMinDistance* checkpointMinDistance)
        {
            if (randomWalk) {
                int previousId = current.id;
                current = next;
                int nextId = previousId;
                do
                {
                    nextId = Random.Range(0, current.links.Count);
                } while (current.links.Count > 1 && nextId == previousId);

               // Debug.Log("From:" + current.id + ", goto:" + nextId + "/" + current.links.Count);
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
            gas = Mathf.MoveTowards(gas, 1, Time.deltaTime);

            if (fancySteer)
            {
                forwardPlane = new Vector3(this.transform.forward.x, 0, this.transform.forward.z);
                targetPlane = new Vector3(checkpoint.x - this.transform.position.x, 0, checkpoint.z - this.transform.position.z).normalized;
                steerUnclamped = Mathf.Atan2(forwardPlane.z, forwardPlane.x) - Mathf.Atan2(targetPlane.z, targetPlane.x);
                steerUnclamped = (steerUnclamped / Mathf.PI + 1) % (2.0f) - 1f;
                steer = Mathf.Clamp(steerUnclamped, -1, 1);
            }
            else
            {
                this.transform.LookAt(checkpoint);
                steer = 0;
            }
            vertical = Mathf.Clamp(checkpoint.y - this.transform.position.y, -1,1);
            
            car.Move(gas, steer, vertical);
        }
    }

    private void OnDrawGizmos()
    {
        
        int prev = 0;
        Vector3 position = Application.isPlaying? initPosition: this.transform.position;
        if (randomWalk)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(this.transform.position, checkpoint);
        }
        else {
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(this.transform.position, position + checkpoints[currentCheckpoint]);
            Gizmos.color = Color.black;
            for (int current = 1; current < checkpoints.Length; ++current)
            {
                Gizmos.DrawLine(position + checkpoints[prev], position + checkpoints[current]);
                prev = current;
            }
            if (loop)
            {
                Gizmos.DrawLine(position + checkpoints[prev], position + checkpoints[0]);
            }
        }
    }
}
