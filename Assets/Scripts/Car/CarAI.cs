using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : Particle
{
    [Header("links")]
    public CharacterController controller;
    public TrafficController traffic;
    /*[SerializeField] Car car;
    public City city;
    
    [SerializeField] Collider movementPrediction;*/

    [Header("AI")]
    GraphSparse<Vector3>.Node current;
    GraphSparse<Vector3>.Node next;
    GraphSparse<Vector3>.Node far;
    public float checkpointMinDistance = 1f;
    public float speed = 6f;
    public float correctionSpeed = 12f;
    public float speedDispertion = 1f;

    [Header("Filters")]
    private Vector3 lastNonZeroHorizontal;
    public float aimingFilter = 0.1f;
    public Vector3 smoothDirection = Vector3.zero;

    /*
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
    [SerializeField] bool angry = false;*/


    public GameObject GetGameObject()
    {
        return gameObject;
    }

    bool initialized = false;
    protected override void DoCreate()
    {
        initialized = false;
        /*current = traffic.GetStartingPoint();
        next = traffic.GetRandomWalk(current);
        far = traffic.GetRandomWalk(current,next);
        transform.position = current.data;
        speed = speed + Random.Range(-speedDispertion, speedDispertion);
        lastNonZeroHorizontal = new Vector3(0, 0, 1);*/
    }

    protected override void DoDestroy()
    {
        current = null;
        next = null;
        far = null;
        this.gameObject.SetActive(false);
        pool.Release(this);
    }


    protected override void DoTick()
    {
        if (!initialized)
        {
            current = traffic.GetStartingPoint();
        next = traffic.GetRandomWalk(current);
        far = traffic.GetRandomWalk(current,next);
        transform.position = current.data;
        speed = speed + Random.Range(-speedDispertion, speedDispertion);
        lastNonZeroHorizontal = new Vector3(0, 0, 1);
            initialized = true;
            return;
        }
        Vector3 delta = traffic.GetOffset(next.data - current.data);
        if ((this.transform.position - (next.data + delta)).sqrMagnitude < (checkpointMinDistance * checkpointMinDistance))
        {
            var tmp  = traffic.GetRandomWalk(next, far);
            current = next;
            next = far;
            far = tmp;
            delta = traffic.GetOffset(next.data - current.data);
        }
        
        Vector3 perfect = Vector3.Project(transform.position - (current.data + delta), (next.data + delta) - (current.data + delta));
        Vector3 correction = perfect + (current.data + delta) - transform.position;
        if (correction.sqrMagnitude > 0.5f)
            correction = correction.normalized * correctionSpeed * Time.deltaTime;
        else correction = Vector3.zero;
        Vector3 movement = (next.data + delta - transform.position).normalized * speed * Time.deltaTime;

        controller.Move(correction + movement);

        Vector3 horizontal = next.data - current.data;
        horizontal.y = 0;
        if (horizontal.sqrMagnitude != 0)
            lastNonZeroHorizontal = horizontal.normalized;
        //smoothDirection = (1 - aimingFilter) * smoothDirection + aimingFilter * lastNonZeroHorizontal;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lastNonZeroHorizontal, Vector3.up), 180*Time.deltaTime);




        //transform.LookAt(transform.position + smoothDirection);

        /*if ((this.transform.position - checkpoint).sqrMagnitude < (checkpointMinDistance * checkpointMinDistance))
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
        }*/
    }

    private void OnDrawGizmos()
    {
        if (current != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(this.transform.position, next.data);
            Gizmos.DrawLine(this.transform.position, current.data);
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(current.data, next.data);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(next.data, far.data);
        }
    }
}
