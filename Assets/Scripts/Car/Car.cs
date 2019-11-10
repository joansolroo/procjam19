using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] CharacterController controller;
    public Transform model;
    [SerializeField] Transform cameraParent;

    [Header("Parameters")]
    public float forwardSpeed = 6;
    public float maxSpeedHorizontal = 10;
    public float maxSpeedVertical = 10;
    public float verticalSpeed = 10;
    public float rotationSpeed = 6;
    public float gravity = 1f;
    public float hleanLimit = 10;
    public float vleanLimit = 10;
    public float inertia = 0.1f;

    [Header("Status")]
    [SerializeField] Vector3 direction;
    Vector3 verticalDirection;
    Vector3 horizontalDirection;
    Vector3 lastNonZeroHorizontalDirection;
    Vector3 currentDirection;
    [SerializeField] public Vector3 velocity;
    float previousGas;
    void Start()
    {
        if(!controller) controller = GetComponent<CharacterController>();
    }

    public void Move(float gas, float steer, float vert)
    {
        float currentGas = Mathf.Lerp(gas, previousGas, inertia);
        previousGas = currentGas;

        horizontalDirection = transform.forward;
        if ((horizontalDirection * currentGas).sqrMagnitude > 0.01f)
            lastNonZeroHorizontalDirection = (horizontalDirection * currentGas).normalized;

        direction = verticalDirection * gravity * maxSpeedVertical + horizontalDirection * currentGas * maxSpeedHorizontal;

        Vector3 prevPosition = this.transform.position;
        Vector3 expectedMovement = direction * Time.deltaTime + new Vector3(0, vert, 0) * verticalSpeed * Time.deltaTime;
        controller.Move(expectedMovement);
        if (steer != 0)
        {
            this.transform.Rotate(Vector3.up, rotationSpeed * steer);
        }
        this.velocity = this.transform.position - prevPosition;
        Vector3 normalizedVelocity=velocity;
        if(expectedMovement.x!=0) normalizedVelocity.x /= expectedMovement.x;
        if (expectedMovement.y != 0) normalizedVelocity.y /= expectedMovement.y;
        if (expectedMovement.z != 0) normalizedVelocity.z /= expectedMovement.z;
        HorizontalLean(model, steer, hleanLimit, .1f);
        VerticalLean(model, vert, vleanLimit* normalizedVelocity.y, .1f);
    }
    
    void HorizontalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngels = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngels.x, targetEulerAngels.y, Mathf.LerpAngle(targetEulerAngels.z, -axis * leanLimit, lerpTime));
    }

    void VerticalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngels = target.localEulerAngles;
        target.localEulerAngles = new Vector3(Mathf.LerpAngle(targetEulerAngels.x, -axis * leanLimit, lerpTime), targetEulerAngels.y, targetEulerAngels.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + currentDirection);
    }
}
