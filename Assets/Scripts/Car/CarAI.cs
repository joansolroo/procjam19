using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : Particle
{
    [Header("links")]
    public CharacterController controller;
    public TrafficController traffic;
    public GameObject model;
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
            far = traffic.GetRandomWalk(current, next);
            transform.position = current.data;
            speed = speed + Random.Range(-speedDispertion, speedDispertion);
            lastNonZeroHorizontal = Vector3.forward;
            initialized = true;
            return;
        }
        Vector3 delta = traffic.GetOffset(next.data - current.data);
        if ((this.transform.position - (next.data + delta)).sqrMagnitude < (checkpointMinDistance * checkpointMinDistance))
        {
            var tmp = traffic.GetRandomWalk(next, far);
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

        bool visible = OcclusionCulling.IsVisibleAABB(controller.bounds) && OcclusionCulling.IsVisibleCollider(controller);
        model.SetActive(visible);
    }
    protected override void UpdateVisuals()
    {
        if (model.activeSelf)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lastNonZeroHorizontal, Vector3.up), 180 * Time.deltaTime);
        }
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
