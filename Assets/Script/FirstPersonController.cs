using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("이동 및 달리기 설정")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 5.5f;
    public float gravity = -9.81f;

    [Header("스태미나 및 탈진(쿨타임) 설정")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaDrainSpeed = 25f;  // 달릴 때 초당 소모량
    public float staminaRegenSpeed = 15f;  // 회복할 때 초당 충전량
    public float normalRegenDelay = 1.5f;  // 달리기 멈춘 후 회복 시작까지 대기 시간(초)
    public float exhaustedRegenDelay = 3.0f; // 0까지 다 썼을 때 회복 시작 대기 시간(초)
    public float minStaminaToRun = 30f;    // 탈진 후 다시 달리기 위해 필요한 최소 스태미나
    public bool isExhausted = false;       // 현재 탈진 상태 여부

    [Header("소음 상태")]
    public float currentNoiseLevel = 0f;

    [Header("카메라 시선 설정")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 80.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float regenWaitTimer = 0f; // 스태미나 회복 대기 타이머

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f);

        // 탈진 상태에서 스태미나가 기준치(30%) 이상 차오르면 탈진 해제
        if (isExhausted && currentStamina >= minStaminaToRun)
        {
            isExhausted = false;
        }

        // Shift를 누르고, 움직이는 중이며, 탈진 상태가 아닐 때만 달리기 작동
        bool canRun = !isExhausted && currentStamina > 0f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && canRun;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isRunning)
        {
            currentStamina -= staminaDrainSpeed * Time.deltaTime;
            currentNoiseLevel = 10f; // 달리기 소음
            regenWaitTimer = normalRegenDelay; // 달리는 동안 회복 대기 타이머 유지

            // 스태미나를 끝까지 다 썼다면 탈진 상태 돌입 및 3초 대기 적용
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
                regenWaitTimer = exhaustedRegenDelay;
                Debug.Log("스태미나 방전(탈진)! 잠시 숨을 골라야 다시 달릴 수 있습니다.");
            }
        }
        else
        {
            currentNoiseLevel = isMoving ? 3f : 0f;

            // 대기 타이머가 먼저 줄어들고, 0초가 되면 스태미나 회복 시작
            if (regenWaitTimer > 0f)
            {
                regenWaitTimer -= Time.deltaTime;
            }
            else if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenSpeed * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // 이동 및 중력 적용
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float moveY = moveDirection.y;

        moveDirection = (forward * vertical + right * horizontal).normalized * currentSpeed;

        if (characterController.isGrounded)
            moveDirection.y = -0.5f;
        else
            moveDirection.y = moveY + (gravity * Time.deltaTime);

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float lookX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float lookY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 노트북 방향키 회전 지원
        float arrowSpeed = 120f * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) lookX += arrowSpeed;
        if (Input.GetKey(KeyCode.LeftArrow)) lookX -= arrowSpeed;
        if (Input.GetKey(KeyCode.UpArrow)) lookY += arrowSpeed;
        if (Input.GetKey(KeyCode.DownArrow)) lookY -= arrowSpeed;

        rotationX += -lookY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, lookX, 0);
    }
}