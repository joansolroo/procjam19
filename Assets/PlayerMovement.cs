using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    private Transform playerModel;

    [Header("Parameters")]
    public float forwardSpeed = 6;
    public float maxSpeedHorizontal = 10;
    public float maxSpeedVertical = 10;
    public float verticalSpeed = 10;
    public float rotationSpeed = 6;
    public float gravity = 1f;
    public float hleanLimit = 10;
    public float vleanLimit = 10;

    [Header("Public References")]
    public Transform aimTarget;
    public Transform cameraParent;


    void Start()
    {
        playerModel = transform.GetChild(0);
        controller = GetComponent<CharacterController>();
    }

    [SerializeField] Vector3 direction;
    Vector3 verticalDirection;
    Vector3 horizontalDirection;
    Vector3 lastNonZeroHorizontalDirection;
    Vector3 currentDirection;

    void Update()
    {
        float gas = Input.GetAxis("Pedals"); // Acceleration
        float steer = Input.GetAxis("Horizontal"); // Turn
        float vert = Input.GetAxis("Vertical"); // Up and down

        Move(gas, steer, vert);
        HorizontalLean(playerModel, steer, hleanLimit, .1f);
        VerticalLean(playerModel, vert, vleanLimit, .1f);
    }

    public void Move(float gas, float steer, float vert)
    {

        horizontalDirection = transform.forward;
        if ((horizontalDirection * gas).sqrMagnitude > 0.01f)
            lastNonZeroHorizontalDirection = (horizontalDirection * gas).normalized;

        direction = verticalDirection * gravity * maxSpeedVertical + horizontalDirection * gas * maxSpeedHorizontal;
        controller.Move(direction * Time.deltaTime);
        if (steer != 0)
        {
            this.transform.Rotate(Vector3.up, rotationSpeed * steer);
        }
        transform.localPosition += new Vector3(0, vert, 0) * verticalSpeed * Time.deltaTime;
        ClampPosition();
    }

    void ClampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
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

}
