using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using System.Diagnostics;

public class PlayerController : NetworkBehaviour
{
    //Unity new input system
    private PlayerInput playerInput;

    //获取组件
    private Rigidbody rb;
    private BoxCollider boxCollider;
    //获取Transform
    public Transform playerTransform;
    public Transform cameraTransform;

    private bool isGround;

    //GetInput获取输入
    private Vector2 inputMouse;
    private Vector2 inputMovement;
    private bool inputJump;
    private bool inputCrouch;
    private bool inputSprint;
    //用于CameraController
    private float xRotation, yRotation;

    //用于计算移动方向
    private Vector3 moveDirection;
    //用于边缘检测
    private bool edgeXmax, edgeXmin, edgeZmax, edgeZmin, edgeXmaxZmax, edgeXmaxZmin, edgeXminZmin, edgeXminZmax;

    //获取玩家数据Player Data
    public MobsScriptableObject playerData;

    //灵敏度（临时)
    public float senX, senY;
    //射线调试(临时)
    public bool isDebug;

    private void Awake()
    {
        playerInput = new PlayerInput();

        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }
    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDisable()
    {
        playerInput.Disable();
    }
    private void Update()
    {
        if (!isLocalPlayer) { return; }
        GetInput();
        CameraController();
        GroundAndEdgeCheck();
        ColliderControl();
    }
    private void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }
        moveController();
    }

    private void GetInput()
    {
        inputMouse = playerInput.Gameplay_Infantry.Mouse.ReadValue<Vector2>();
        inputMovement = playerInput.Gameplay_Infantry.Movement.ReadValue<Vector2>();
        inputJump = playerInput.Gameplay_Infantry.Jump.IsPressed();
        inputCrouch = playerInput.Gameplay_Infantry.Crouch.IsPressed();
        inputSprint = playerInput.Gameplay_Infantry.Sprint.IsPressed();
    }

    private void CameraController()
    {
        yRotation = Mathf.Repeat(yRotation + inputMouse.x * Time.deltaTime * senX, 360);
        xRotation -= inputMouse.y * Time.deltaTime * senY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //playerTransform.Rotate(Vector3.up * mouseInputX);
        //cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerTransform.rotation = Quaternion.Euler(0, yRotation, 0);

        //Position the camera in the correct position
        Camera.main.transform.position = cameraTransform.position;
        Camera.main.transform.rotation = cameraTransform.rotation;
    }

    private void moveController()
    {
        //移动的向量
        moveDirection = playerTransform.forward * inputMovement.y + playerTransform.right * inputMovement.x;

        moveDirection = Edge(moveDirection);

        if (isGround)
            //地面移动的力
            rb.AddForce(moveDirection.normalized * playerData.groundMoveAcceleration, ForceMode.Force);
        else
            //空中移动的力
            rb.AddForce(moveDirection.normalized * playerData.airMoveAcceleration, ForceMode.Force);

        if (inputJump && isGround)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * playerData.jumpAcceleration, ForceMode.Impulse);
        }
        SpeedControl();
    }

    private void SpeedControl()
    {
        //地面阻力和空中阻力
        if (isGround)
        {
            Vector3 velocity = rb.velocity;
            Vector3 drag = -playerData.groundDrag * velocity;
            drag.y = 0;
            rb.AddForce(drag);
        }
        else
        {
            Vector3 velocity = rb.velocity;
            Vector3 drag = -playerData.airDrag * velocity;
            drag.y = 0;
            rb.AddForce(drag);
        }

        //水平面移动速度
        Vector3 flatSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (!inputCrouch && (flatSpeed.magnitude > playerData.moveSpeed))
        {
            Vector3 limitedVel = flatSpeed.normalized * playerData.moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
        if (inputCrouch && (flatSpeed.magnitude > playerData.crouchSpeed))
        {
            Vector3 limitedVel = flatSpeed.normalized * playerData.crouchSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

        //玩家增加重力(为了手感)
        rb.AddForce(new Vector3(0, -10, 0), ForceMode.Acceleration);
    }

    private void ColliderControl()
    {
        if (inputCrouch)
        {
            boxCollider.center = new Vector3(0, playerData.crouchHeight / 2, 0);
            boxCollider.size = new Vector3(playerData.width, playerData.crouchHeight, playerData.width);
            cameraTransform.localPosition = new Vector3(0, playerData.crouchHeight - (playerData.standHeight - playerData.cameraHeight), 0);
        }
        else
        {
            boxCollider.center = new Vector3(0, playerData.standHeight / 2, 0);
            boxCollider.size = new Vector3(playerData.width, playerData.standHeight, playerData.width);
            cameraTransform.localPosition = new Vector3(0, playerData.cameraHeight, 0);
        }
    }


    private void GroundAndEdgeCheck()
    {
        bool rayXmaxZmax = Raycast(new Vector3(playerData.width / 2 - 0.01f, playerData.standHeight / 10, playerData.width / 2 - 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool rayXmaxZmin = Raycast(new Vector3(playerData.width / 2 - 0.01f, playerData.standHeight / 10, -playerData.width / 2 + 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool rayXminZmax = Raycast(new Vector3(-playerData.width / 2 + 0.01f, playerData.standHeight / 10, playerData.width / 2 - 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool rayXminZmin = Raycast(new Vector3(-playerData.width / 2 + 0.01f, playerData.standHeight / 10, -playerData.width / 2 + 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        if (rayXmaxZmax || rayXmaxZmin || rayXminZmax || rayXminZmin)
        {
            isGround = true;
        }
        else
        {
            isGround = false;
        }

        bool crouchRayXmaxZmaxA = Raycast(new Vector3(playerData.width / 2 - 0.11f, playerData.standHeight / 10, playerData.width / 2 - 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool crouchRayXmaxZmaxB = Raycast(new Vector3(playerData.width / 2 - 0.01f, playerData.standHeight / 10, playerData.width / 2 - 0.11f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);

        bool crouchRayXmaxZminA = Raycast(new Vector3(playerData.width / 2 - 0.11f, playerData.standHeight / 10, -playerData.width / 2 + 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool crouchRayXmaxZminB = Raycast(new Vector3(playerData.width / 2 - 0.01f, playerData.standHeight / 10, -playerData.width / 2 + 0.11f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);

        bool crouchRayXminZmaxA = Raycast(new Vector3(-playerData.width / 2 + 0.11f, playerData.standHeight / 10, playerData.width / 2 - 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool crouchRayXminZmaxB = Raycast(new Vector3(-playerData.width / 2 + 0.01f, playerData.standHeight / 10, playerData.width / 2 - 0.11f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);

        bool crouchRayXminZminA = Raycast(new Vector3(-playerData.width / 2 + 0.11f, playerData.standHeight / 10, -playerData.width / 2 + 0.01f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);
        bool crouchRayXminZminB = Raycast(new Vector3(-playerData.width / 2 + 0.01f, playerData.standHeight / 10, -playerData.width / 2 + 0.11f), Vector3.down, playerData.standHeight / 10 + 0.1f, playerData.ground, isDebug);

        if ((inputCrouch && rayXmaxZmax && !crouchRayXmaxZmaxA) || (rayXmaxZmin && !crouchRayXmaxZminA))
            edgeXmax = true;
        else
            edgeXmax = false;
        if ((inputCrouch && rayXminZmax && !crouchRayXminZmaxA) || (rayXminZmin && !crouchRayXminZminA))
            edgeXmin = true;
        else
            edgeXmin = false;
        if ((inputCrouch && rayXmaxZmax && !crouchRayXmaxZmaxB) || (rayXminZmax && !crouchRayXminZmaxB))
            edgeZmax = true;
        else
            edgeZmax = false;
        if ((inputCrouch && rayXmaxZmin && !crouchRayXmaxZminB) || (rayXminZmin && !crouchRayXminZminB))
            edgeZmin = true;
        else
            edgeZmin = false;

        if (inputCrouch && rayXmaxZmax && !crouchRayXmaxZmaxA && !crouchRayXmaxZmaxB)
            edgeXmaxZmax = true;
        else
            edgeXmaxZmax = false;

        if (inputCrouch && rayXmaxZmin && !crouchRayXmaxZminA && !crouchRayXmaxZminB)
            edgeXmaxZmin = true;
        else
            edgeXmaxZmin = false;

        if (inputCrouch && rayXminZmax && !crouchRayXminZmaxA && !crouchRayXminZmaxB)
            edgeXminZmax = true;
        else
            edgeXminZmax = false;

        if (inputCrouch && rayXminZmin && !crouchRayXminZminA && !crouchRayXminZminB)
            edgeXminZmin = true;
        else
            edgeXminZmin = false;
    }

    //边缘判定
    private Vector3 Edge(Vector3 moveDirection)
    {
        if (edgeXmax)
            moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, 0, float.MaxValue), moveDirection.y, moveDirection.z);

        if (edgeXmin)
            moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, float.MinValue, 0), moveDirection.y, moveDirection.z);

        if (edgeZmax)
            moveDirection = new Vector3(moveDirection.x, moveDirection.y, Mathf.Clamp(moveDirection.z, 0, float.MaxValue));

        if (edgeZmin)
            moveDirection = new Vector3(moveDirection.x, moveDirection.y, Mathf.Clamp(moveDirection.z, float.MinValue, 0));


        if (edgeXmaxZmax)
            moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, 0, float.MaxValue), moveDirection.y, Mathf.Clamp(moveDirection.z, 0, float.MaxValue));

        if (edgeXmaxZmin)
            moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, 0, float.MaxValue), moveDirection.y, Mathf.Clamp(moveDirection.z, float.MinValue, 0));

        if (edgeXminZmax)
            moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, float.MinValue, 0), moveDirection.y, Mathf.Clamp(moveDirection.z, 0, float.MaxValue));

        if (edgeXminZmin)
            moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, float.MinValue, 0), moveDirection.y, Mathf.Clamp(moveDirection.z, float.MinValue, 0));

        return moveDirection;
    }

    //射线
    private bool Raycast(Vector3 offset, Vector3 rayDiraction, float length, LayerMask layer, bool isDebug)
    {
        Vector3 pos = transform.position;
        bool hit = Physics.Raycast(pos + offset, rayDiraction, length, layer);
        //射线是否显示，用于调试
        if (isDebug == true)
        {
            Color color = hit ? Color.red : Color.green;
            UnityEngine.Debug.DrawRay(pos + offset, rayDiraction * length, color);
        }
        return hit;
    }
}