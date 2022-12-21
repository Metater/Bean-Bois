using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerMovement : PlayerComponent
{
    [Header("General")]
    [SerializeField] private CharacterController controller;

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
    private bool controllerWasGrounded = true;

    [Header("Look")]
    [SerializeField] private float lookSpeed;
    [SerializeField] private float lookXLimit;
    private float rotationX = 0;

    [Header("SRB")]
    [SerializeField] private double srbSpeed;
    [SerializeField] private double srbBurnTime;
    private bool isSrbAvailable = true;
    private double srbStopTime;

    #region Player Callbacks
    public override void PlayerAwake()
    {
        void OnDeltaSpectating()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            isSrbAvailable = true;
            refs.srbAvailableText.gameObject.SetActive(true);
        }

        player.OnStartedSpectating += OnDeltaSpectating;
        player.OnStoppedSpectating += OnDeltaSpectating;
    }
    public override void PlayerUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (transform.position.y < 0)
        {
            player.CmdStartSpectating();
        }

        UpdateMovementVectors();
        UpdateMovement();
    }
    #endregion Player Callbacks

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

        // Stop movement when menu is open
        if (manager.IsCursorVisable)
        {
            moveX = 0;
            moveY = 0;
        }

        float moveVelocityY = moveVelocity.y;
        moveVelocity = (forward * moveX) + (right * moveY);

        // Allow jumps only when menu is closed and is grounded
        if (!manager.IsCursorVisable && controller.isGrounded && Input.GetButtonDown("Jump"))
            moveVelocity.y = jumpSpeed;
        else // Keep old y velocity
            moveVelocity.y = moveVelocityY;

        // Apply gravity when not on ground
        if (!controller.isGrounded)
            moveVelocity.y -= gravity * Time.deltaTime;

        UpdateSrb();

        // Apply move velocity to controller
        Vector3 moveDelta = moveVelocity * Time.deltaTime;
        controller.Move(moveDelta);

        // If was grounded last update and isnt grounded now and falling down, cancel velocity
        if (controllerWasGrounded && !controller.isGrounded && moveVelocity.y < 0)
            moveVelocity.y = 0;

        // Only allow looking when menu is closed
        if (!manager.IsCursorVisable)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            Quaternion rotation = Quaternion.Euler(rotationX, 0, 0);
            Camera.main.transform.localRotation = rotation;
            handsTransform.transform.localRotation = rotation;

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        controllerWasGrounded = controller.isGrounded;
    }
    private void UpdateSrb()
    {
        if (player.isSpectating || manager.IsCursorVisable)
        {
            return;
        }

        if (isSrbAvailable)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isSrbAvailable = false;
                srbStopTime = Time.timeAsDouble + srbBurnTime;
                refs.srbAvailableText.gameObject.SetActive(false);
            }
        }
        else if (srbStopTime > Time.timeAsDouble) // Srb still firing?
        {
            moveVelocity.y += (float)(gravity + srbSpeed) * Time.deltaTime;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!isLocalPlayer || player.isSpectating)
        {
            return;
        }

        if (hit.gameObject.CompareTag("Lava") ||
            hit.gameObject.CompareTag("Base") ||
            hit.gameObject.CompareTag("SpectatorBox"))
        {
            player.CmdStartSpectating();
        }
    }

    public Vector3 GetCurrentVelocity()
    {
        Vector3 currentVelocity = moveVelocity;
        if (controller.isGrounded)
        {
            currentVelocity.y = 0;
        }
        return currentVelocity;
    }

    #region Teleport
    public void GeneralTeleport(Vector3 position)
    {
        if (isLocalPlayer)
        {
            Teleport(position);
        }
        else if (isServer)
        {
            RpcTeleport(position);
        }
    }
    [ClientRpc]
    private void RpcTeleport(Vector3 position)
    {
        Teleport(position);
    }
    private void Teleport(Vector3 position)
    {
        moveVelocity = Vector3.zero;
        controller.enabled = false;
        controller.transform.position = position;
        controller.enabled = true;
    }
    #endregion Teleport
}
