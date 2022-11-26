using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour, Player.IPlayerCallbacks
{
    #region Fields
    [Header("General")]
    private GameManager manager;
    [SerializeField] private Player player;
    [SerializeField] private CharacterController controller;
    private bool isSpectating = false;
    private bool isInSpectatorBox = false;
    [Header("Transforms")]
    [SerializeField] private Transform handsTransform;
    [Header("Move")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float moveSmoothTime = 0;
    private Vector3 moveVelocity = Vector3.zero;
    private float moveX = 0;
    private float moveY = 0;
    private float moveXSmoothVelocity = 0;
    private float moveYSmoothVelocity = 0;
    private bool wasGrounded = true;
    [Header("Look")]
    [SerializeField] private float lookSpeed;
    [SerializeField] private float lookXLimit;
    private float rotationX = 0;
    [Header("Velocity Calculation")]
    [SerializeField] private int velocityAveragingQueueSize;
    private Vector3 lastPosition = Vector3.zero;
    public Vector3 Velocity { get; private set; } = Vector3.zero;
    private Queue<Vector3> velocities;
    [Header("SRB")]
    [SerializeField] private double srbSpeed;
    [SerializeField] private double srbBurnTime;
    private bool isSrbAvailable = true;
    private double srbStopTime;
    #endregion Fields

    #region Player Callbacks
    public void PlayerAwake()
    {
        manager = FindObjectOfType<GameManager>(true);

        velocities = new();
    }

    public void PlayerStart()
    {
        lastPosition = transform.position;
    }

    public void PlayerUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (transform.position.y < 0)
        {
            isSpectating = true;
        }

        UpdateVelocityCalculation();
        UpdateMovementVectors();
        UpdateMovement();
    }
    #endregion Player Callbacks

    private void UpdateVelocityCalculation()
    {
        Vector3 rawVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        velocities.Enqueue(rawVelocity);
        while (velocities.Count > velocityAveragingQueueSize)
        {
            velocities.Dequeue();
        }
        Velocity = Vector3.zero;
        foreach (Vector3 v in velocities)
        {
            Velocity += v;
        }
        Velocity /= velocities.Count;
    }
    private void UpdateMovementVectors()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float lastMoveX = moveX;
        float lastMoveY = moveY;
        moveX = (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical");
        moveY = (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal");
        moveX = Mathf.SmoothDamp(lastMoveX, moveX, ref moveXSmoothVelocity, moveSmoothTime);
        moveY = Mathf.SmoothDamp(lastMoveY, moveY, ref moveYSmoothVelocity, moveSmoothTime);
    }
    private void UpdateMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float moveVelocityY = moveVelocity.y;
        moveVelocity = (forward * moveX) + (right * moveY);

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            moveVelocity.y = jumpSpeed;
        else
            moveVelocity.y = moveVelocityY;

        if (!controller.isGrounded)
            moveVelocity.y -= gravity * Time.deltaTime;

        if (isSrbAvailable)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isSrbAvailable = false;
                srbStopTime = Time.timeAsDouble + srbBurnTime;
                manager.srbText.text = "";
            }
        }
        else if (srbStopTime > Time.timeAsDouble)
        {
            moveVelocity.y += (float)(gravity + srbSpeed) * Time.deltaTime;
        }

        Vector3 moveDelta = moveVelocity * Time.deltaTime;
        controller.Move(moveDelta);

        if (wasGrounded && !controller.isGrounded && moveVelocity.y < 0)
            moveVelocity.y = 0;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        Quaternion rotation = Quaternion.Euler(rotationX, 0, 0);
        Camera.main.transform.localRotation = rotation;
        handsTransform.transform.localRotation = rotation;

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        // Teleport to center if you fall off of the map
        if (transform.position.y < -25)
        {
            // TODO May need to do networkTransform.CmdTeleport instead, to avoid lerp?
            transform.position = new Vector3(0, 25, 0);
            moveVelocity.y = 0f;
        }

        if (isSpectating && !isInSpectatorBox)
        {
            transform.position = manager.spectatorBoxTransform.position;
            moveVelocity = Vector3.zero;
            isInSpectatorBox = true;
        }

        wasGrounded = controller.isGrounded;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!isLocalPlayer || isSpectating)
        {
            return;
        }

        if (hit.gameObject.CompareTag("Lava") || hit.gameObject.CompareTag("Base"))
        {
            isSpectating = true;
        }
    }
}
