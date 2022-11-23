using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    // Private Runtime Set Unity References
    private GameManager manager;

    // Private Set Unity References
    [SerializeField] private CharacterController controller;
    [Space]
    [SerializeField] private List<GameObject> invisibleToSelf;
    [Header("Transforms")]
    [SerializeField] private Transform handsTransform;
    [SerializeField] private Transform gripTransform;
    [SerializeField] private Transform muzzleTransform;

    // Private Set Unity Variables
    [Header("Movement")]
    [SerializeField] public float walkingSpeed;
    [SerializeField] public float runningSpeed;
    [SerializeField] public float jumpSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float lookXLimit;
    [SerializeField] private float momentumLerp;

    [Header("Health")]
    [SerializeField] private int startingHealth;

    // Private Variables
    private float rotationX = 0;
    private Vector3 moveVelocity = Vector3.zero;
    private float lastSpeedX = 0;
    private float lastSpeedY = 0;

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            // Position Own Camera
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 1.6f, 0);

            // Make Own GameObjects invisible
            invisibleToSelf.ForEach(go => go.SetActive(false));

            SetCursorVisibility(false);
        }
    }

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorVisibility(!Cursor.visible);
        }

        Move();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 a = muzzleTransform.position;
            if (Physics.Raycast(muzzleTransform.position, Camera.main.transform.forward, out var hit, 1024f))
            {
                if (hit.transform.TryGetComponent(out PlayerShoot player))
                {
                    manager.SpawnProjectileTrail(a, hit.point, true, hit.normal);
                    CmdHitPlayer(player.netId, a, hit.point - player.transform.position, hit.normal);
                }
                else
                {
                    manager.SpawnProjectileTrail(a, hit.point, true, hit.normal);
                    CmdHit(a, hit.point, true, hit.normal);
                }
            }
            else
            {
                Vector3 b = muzzleTransform.position + Camera.main.transform.forward * 1024f;
                manager.SpawnProjectileTrail(muzzleTransform.position, muzzleTransform.position + Camera.main.transform.forward * 1024f, false, Vector3.zero);
                CmdHit(a, b, false, Vector3.zero);
            }
        }
    }

    [Command]
    public void CmdHitPlayer(uint netId, Vector3 a, Vector3 bLocalPositionOnPlayer, Vector3 hitNormal)
    {
        RpcHitPlayer(netId, a, bLocalPositionOnPlayer, hitNormal);
    }

    [ClientRpc]
    public void RpcHitPlayer(uint netId, Vector3 a, Vector3 bLocalPositionOnPlayer, Vector3 hitNormal)
    {
        // TODO FindObjects bad???
        foreach (var player in FindObjectsOfType<PlayerShoot>())
        {
            if (player.netId == netId)
            {
                Vector3 b = player.transform.position + bLocalPositionOnPlayer;
                manager.SpawnProjectileTrail(a, b, true, hitNormal);
                break;
            }
        }
    }

    [Command]
    public void CmdHit(Vector3 a, Vector3 b, bool hitObject, Vector3 hitNormal)
    {
        RpcHit(a, b, hitObject, hitNormal);
    }

    [ClientRpc]
    public void RpcHit(Vector3 a, Vector3 b, bool hitObject, Vector3 hitNormal)
    {
        manager.SpawnProjectileTrail(a, b, hitObject, hitNormal);
    }

    private void Move()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speedX = (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical");
        float speedY = (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal");
        if (speedX < lastSpeedX)
            speedX = Mathf.Lerp(speedX, lastSpeedX, momentumLerp);
        if (speedY < lastSpeedY)
            speedY = Mathf.Lerp(speedY, lastSpeedY, momentumLerp);
        lastSpeedX = speedX;
        lastSpeedY = speedY;
        float moveVelocityY = moveVelocity.y;
        moveVelocity = (forward * speedX) + (right * speedY);

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            moveVelocity.y = jumpSpeed;
        else
            moveVelocity.y = moveVelocityY;

        if (!controller.isGrounded)
            moveVelocity.y -= gravity * Time.deltaTime;

        Vector3 moveDelta = moveVelocity * Time.deltaTime;
        controller.Move(moveDelta);

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        var rotation = Quaternion.Euler(rotationX, 0, 0);
        Camera.main.transform.localRotation = rotation;
        handsTransform.localRotation = rotation;

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        if (transform.position.y < -8)
        {
            transform.position = new Vector3(0, 8, 0);
            moveVelocity.y = 0f;
        }
    }

    private void SetCursorVisibility(bool isVisible)
    {
        if (isVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
