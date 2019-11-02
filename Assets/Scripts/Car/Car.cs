using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    public Transform model;

    [Header("Car attributes")]
    public float maxSpeedHorizontal = 10;
    public float maxSpeedVertical = 10;
    public float rotationSpeed = 6;
    public float rotationSmooth = 5;
    public Transform floorTest;
    public float gravity = 1f;

    public LayerMask floorMask;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    [SerializeField] Vector3 direction;
    Vector3 verticalDirection;
    Vector3 horizontalDirection;
    Vector3 currentDirection;
    
    private float currentHeight;

    public float CurrentHeight { get => currentHeight; set => currentHeight = value; }

    private void FixedUpdate()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(floorTest.position, floorTest.TransformDirection(Vector3.down), out hit, Mathf.Infinity, floorMask))
        {
            Debug.DrawRay(floorTest.position, floorTest.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            CurrentHeight = hit.distance;
        }
    }
    public void Move(float speed, float steer, float deltaHeight)
    {
        //ensure that the target height is positive
        float targetHeight = currentHeight + deltaHeight;
        targetHeight = Mathf.Max(targetHeight, 0);
        float dy = targetHeight - currentHeight;

        if (Mathf.Abs(dy) > 1) dy = Mathf.Sign(dy);
        
        verticalDirection = new Vector3(0, dy, 0);
        horizontalDirection = transform.forward;

        direction = verticalDirection * gravity * maxSpeedVertical + horizontalDirection* speed * maxSpeedHorizontal;
        controller.Move(direction * Time.deltaTime);
        if (steer != 0)
        {
            this.transform.Rotate(Vector3.up, rotationSpeed * steer);
        }
    }
    public void MoveTowards(Vector3 targetPosition)
    {
       
        direction = targetPosition - this.transform.position;
        float msqr = direction.sqrMagnitude;
        if(msqr>1)
        {
            direction /= msqr;
        }

        verticalDirection = new Vector3(0,direction.y,0)*maxSpeedVertical;
        horizontalDirection = direction*maxSpeedHorizontal;
        horizontalDirection.y = 0;
        
        controller.Move((horizontalDirection+verticalDirection) * Time.deltaTime);
        Debug.DrawLine(this.transform.position, targetPosition);
    }

    private void LateUpdate()
    {
        // visuals
        Vector3 targetDirection;
        /*if(direction.sqrMagnitude < 0.01f)
        {
            targetDirection = this.transform.forward;
        }
        else */if (horizontalDirection.sqrMagnitude == 0)
        {
            targetDirection = verticalDirection + this.transform.forward;
        }else
        {
            targetDirection = this.direction;
        }

        currentDirection = Vector3.MoveTowards(currentDirection, targetDirection, Time.deltaTime * rotationSmooth);
        model.LookAt(model.position + currentDirection);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + currentDirection);
    }
}
