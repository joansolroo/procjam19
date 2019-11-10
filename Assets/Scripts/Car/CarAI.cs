using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : Particle
{
    [Header("links")]
    [SerializeField] Car car;
    public City city;
    public TrafficController traffic;
    [SerializeField] Collider movementPrediction;

    [Header("AI")]
    [SerializeField] float checkpointMinDistance = 1f;
    [SerializeField] bool fancySteer = false;
    [SerializeField] bool randomWalk = true;
    GraphSparse<Vector3>.Node current;
    GraphSparse<Vector3>.Node next;

    [SerializeField] bool predictive = false;
    [SerializeField] float patience = 10;

    [SerializeField] Vector3[] checkpoints;
    [SerializeField] bool loop = true;
    public int currentCheckpoint = 0;
    Vector3 initPosition;
    Vector3 checkpoint;
    Vector3 checkpointRoadBeginning;
    Vector3 checkpointRoadEnd;

    [Header("Status")]
    [SerializeField] float gas = 0;
    [SerializeField] float steer = 0;
    [SerializeField] float steerUnclamped = 0;
    [SerializeField] float vertical = 0;
    public Vector3 forwardPlane;
    public Vector3 targetPlane;
    public Vector3 positionOnPath;
    [SerializeField] float currentPatience;
    [SerializeField] bool angry = false;


    public GameObject GetGameObject()
    {
        return gameObject;
    }

    bool targetisCurrentRoad = true;
    protected override void DoCreate()
    {
        currentPatience = patience;
        if (randomWalk)
        {
            current = traffic.GetStartingPoint();
            next = traffic.GetRandomWalk(current);
            checkpoint = traffic.GetRoadPoint(current, next.data - current.data);
            checkpointRoadBeginning = traffic.GetRoadPoint(current, next.data - current.data);
            checkpointRoadEnd = traffic.GetRoadPoint(next, next.data - current.data);
            this.transform.position = checkpoint;
            targetisCurrentRoad = true;
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
        this.gameObject.SetActive(false);
        pool.Release(this);
    }


    protected override void DoTick()
    {
        if ((this.transform.position - checkpoint).sqrMagnitude < (checkpointMinDistance * checkpointMinDistance))
        {
            if (randomWalk)
            {
                if (targetisCurrentRoad)
                {
                    checkpoint = traffic.GetRoadPoint(next, next.data - current.data);
                    targetisCurrentRoad = false;
                }
                else
                {
                    var newNext = traffic.GetRandomWalk(current, next);
                    current = next;
                    next = newNext;
                    checkpoint = traffic.GetRoadPoint(current, next.data - current.data);
                    targetisCurrentRoad = true;
                }
                checkpointRoadBeginning = traffic.GetRoadPoint(current, next.data - current.data);
                checkpointRoadEnd = traffic.GetRoadPoint(next, next.data - current.data);
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
            positionOnPath = Vector3.Project(this.transform.position - checkpointRoadBeginning, checkpointRoadEnd - checkpointRoadBeginning) + checkpointRoadBeginning;
            Vector3 errorCorrection = (positionOnPath - this.transform.position);
            Vector3 targetPosition;
            if (errorCorrection.sqrMagnitude > checkpointMinDistance)
            {
                targetPosition = checkpoint + errorCorrection * 10;
            }
            else
            {
                targetPosition = checkpoint;
            }

            gas = Mathf.MoveTowards(gas, 1, Time.deltaTime);

            if (fancySteer)
            {
                forwardPlane = new Vector3(this.transform.forward.x, 0, this.transform.forward.z);
                targetPlane = new Vector3(targetPosition.x - this.transform.position.x, 0, targetPosition.z - this.transform.position.z).normalized;
                steerUnclamped = Mathf.Atan2(forwardPlane.z, forwardPlane.x) - Mathf.Atan2(targetPlane.z, targetPlane.x);
                steerUnclamped = (steerUnclamped / Mathf.PI + 1) % (2.0f) - 1f;
                steer = Mathf.Clamp(steerUnclamped, -1, 1);
            }
            else
            {
                var targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
                // Smoothly rotate towards the target point.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, car.rotationSpeed * Time.deltaTime);
                steer = 0;
            }
            vertical = Mathf.Clamp(targetPosition.y - this.transform.position.y, -1, 1);

            if (predictive)
            {
                bool hit = Physics.OverlapSphere(movementPrediction.transform.position, 1 / 4).Length > 0;
                if (!hit || angry)
                {
                    currentPatience += Time.deltaTime;
                    if (angry && currentPatience > patience / 2)
                    {
                        angry = false;
                    }
                }
                else
                {
                    currentPatience -= Time.deltaTime;
                    gas = 0;
                    if (currentPatience <= 0)
                    {
                        angry = true;
                    }
                }
                Mathf.Clamp(currentPatience, 0, patience);
            }
            car.Move(gas, steer, vertical);
        }
    }

    private void OnDrawGizmos()
    {

        int prev = 0;
        Vector3 position = Application.isPlaying ? initPosition : this.transform.position;
        if (randomWalk)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(this.transform.position, checkpoint);
            Gizmos.DrawLine(this.transform.position, positionOnPath);
            Gizmos.DrawLine(checkpointRoadBeginning, checkpointRoadEnd);
        }
        else
        {
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
