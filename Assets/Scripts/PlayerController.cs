
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// 2022.12.05 / LJ
/// 플레이어 조작 관련 스크립트
/// </summary>
public class PlayerController : NetworkingObject
{
    private PlayerStateManager playerStateManager;

    [Header("Player Movement Stat")]
    [Range(0f, 100f)] [SerializeField] private float speed;
    CharacterController controller;
    [SerializeField] private bool jump; // 점프 중이라면 true
    [SerializeField] private LayerMask ground;
    [SerializeField] [Range(0f, 10f)] private float jumpHeight;
    [SerializeField] [Range(0f, 10f)] private float jumpTimeout;
    private float jumpTimer;

    private Rigidbody rb;

    [Header("Player Movement Stat")]
    [Range(0f, 100f)]
    [SerializeField]
    private float speed;


    private float movementX;
    private float movementY;

    [Header("Player Rotation")]
    [SerializeField] private Camera cam;
    [SerializeField] private float targetRotation = 0f;
    [SerializeField] private float rotationVelocity;
    private float rotationTime = 0.12f;
    

    [Header("Player Gravity")]
    [SerializeField] [Range(-20f, 20f)] private float gravity = -9.81f;
    [SerializeField] private float verticalVelocity;


    [Header("Player Animation")]
    [SerializeField] private Animator anim;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerStateManager = GetComponent<PlayerStateManager>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) onJump();
        if (IsCheckGrounded()) jump = false;
        else jump = true;


        if (isMine)
        {
            InputFunc();
        }
        if (!isMine)
        {
            SyncPos();
        }
    }

    private void FixedUpdate()
    {
        if(isMine)
            Movement();
    }

    void InputFunc()
    {

        int XInput = (movementX > 0) ? 1 : (movementX <0) ? 2 : 0;
        int YInput = (movementY > 0) ? 1 : (movementY <0) ? 2 : 0;
        int x = XInput << 27;
        int y = YInput << 23;

        inputFlag = 0;
        inputFlag = inputFlag | x;
        inputFlag = inputFlag | y;
        bool isDiff = prev_inputFlag != inputFlag;
        if(isDiff)
            Debug.Log("Difficult:  " + isDiff);

        prev_inputFlag = inputFlag;
        if (isDiff)
        {
            C_Move move = new C_Move();
            move.Transform = null;
            move.InputFlag = inputFlag;

            NetworkManager.Instance.Send(move);
        }
    }

    /// <summary>
    /// 2022.12.07 / LJ
    /// 플레이어 이동 감지
    /// </summary>
    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    /// <summary>
    /// 2022.12.07 / LJ??
    /// 플레이어 이동 구현
    /// </summary>
    void Movement()
    {

        Vector3 targetDirection = Vector3.zero;

        if (movement != Vector3.zero)
        {
            targetRotation = Mathf.Atan2(movementX, movementY) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        if ((!(movementX == 0 && movementY == 0)))
        {
            // StateManager
            playerStateManager.State = PlayerState.Move;

            // Animation
            anim.SetBool("Move", true);

            targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            //rb.velocity = movement * speed * Time.deltaTime;
        }
        else
        {
            //StateManager
            if (playerStateManager.State != PlayerState.AFK)
            {
                playerStateManager.State = PlayerState.Idle;
                // Animation
                anim.SetBool("Move", false);
            }
        }

        if (jump) // 중력
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
    }

    /// <summary>
    /// 2022.12.21 / LJ
    /// 바닥 검사
    /// </summary>
    private bool IsCheckGrounded()
    {
        if (controller.isGrounded)
        {
            return true;
        }
        var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        float maxDistance = 0.3f;
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.yellow);
        return Physics.Raycast(ray, maxDistance, ground);
    }

    /// <summary>
    /// 2022.12.21 / LJ
    /// Space키를 눌렀을 때 실행
    /// </summary>
    void onJump()
    {
        if (IsCheckGrounded() && !jump) // 점프가 가능 하다면
        {
            verticalVelocity = 0f;
            JumpAndGravity();
        }
        else return;
    }

    /// <summary>
    /// 2022.12.21 / LJ
    /// 점프 및 중력 관리
    /// </summary>
    void JumpAndGravity()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumpTimer = 0f;
        JumpTimerOut();
        playerStateManager.State = PlayerState.Move;
        // Animation
        anim.SetTrigger("Jump");
    }

    /// <summary>
    ///  2022.12.21 / LJ
    ///  점프 시간 관리
    /// </summary>
    void JumpTimerOut()
    {
        jumpTimer += Time.deltaTime;
        if (jumpTimer < jumpTimeout)
        {
            Invoke("JumpTimerOut", Time.deltaTime);
            return;
        }
        jump = true;
        return;
    }

    void SyncPos()
    {

        int x = ((inputFlag >> 27) == 1) ? 1: ((inputFlag >> 27) == 2) ? -1 : 0;
        int y = ((inputFlag >> 23 & 0b1111) == 1) ? 1: ((inputFlag >> 23 & 0b1111) == 2) ? -1 : 0;
        rb.velocity = new Vector3(x,0,y).normalized * speed * Time.deltaTime;
        Debug.Log("Y:  " + (inputFlag >> 23 & 0b1111));
        Debug.Log($"{x} {y}");
    }
}


