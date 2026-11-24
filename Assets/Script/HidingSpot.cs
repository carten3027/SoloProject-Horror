using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [Header("은신 위치 및 플레이어 연결")]
    public Transform hideViewPoint; // 숨었을 때 플레이어가 위치할 지점
    public FirstPersonController playerController;

    [Header("숨참기 설정")]
    public float maxBreathTime = 4.0f; // 최대 숨참기 가능 시간(초)
    public float currentBreath = 4.0f;
    public bool isPlayerHiding = false;
    public bool isHoldingBreath = false;

    private CharacterController charController;
    private Vector3 savedExitPosition;
    private float gaspPenaltyTimer = 0f;

    void Start()
    {
        if (playerController != null)
            charController = playerController.GetComponent<CharacterController>();

        currentBreath = maxBreathTime;
    }

    void Update()
    {
        if (!isPlayerHiding || playerController == null) return;

        // 숨는 직후 같은 프레임에 바로 나가는 것을 방지하기 위해 E키 입력 체크
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitHiding();
            return;
        }

        // 옷장에 숨어있는 동안 플레이어 스태미나 자동 회복
        if (playerController.currentStamina < playerController.maxStamina)
        {
            playerController.currentStamina += 12f * Time.deltaTime;
            playerController.currentStamina = Mathf.Clamp(playerController.currentStamina, 0f, playerController.maxStamina);
        }

        // 숨참기 실패 후 헐떡이는 페널티 시간 처리
        if (gaspPenaltyTimer > 0)
        {
            gaspPenaltyTimer -= Time.deltaTime;
            playerController.currentNoiseLevel = 15f; // 숨이 막혀 큰 소음 발생
            currentBreath += Time.deltaTime * 0.8f;
            return;
        }

        // 스페이스바를 누르고 있으면 숨참기 작동
        if (Input.GetKey(KeyCode.Space))
        {
            isHoldingBreath = true;
            currentBreath -= Time.deltaTime;
            playerController.currentNoiseLevel = 0f; // 숨을 참으면 완벽히 무음 상태

            Debug.Log($"숨 참는 중... 남은 호흡: {currentBreath:F1}초");

            if (currentBreath <= 0)
            {
                currentBreath = 0;
                isHoldingBreath = false;
                gaspPenaltyTimer = 1.5f; // 1.5초 동안 헐떡이며 큰 소음 발생
                Debug.Log("호흡 한계 도달! 큰 숨소리가 났습니다!");
            }
        }
        else
        {
            // 평소 숨어있을 때는 미세한 숨소리(소음 2)가 발생하며 호흡 회복
            isHoldingBreath = false;
            playerController.currentNoiseLevel = 2f;
            if (currentBreath < maxBreathTime)
                currentBreath += Time.deltaTime * 1.2f;
        }
    }

    public string GetInteractPrompt()
    {
        return isPlayerHiding ? "[E] 밖으로 나가기 / [Space 홀드] 숨 참기" : "[E] 옷장에 숨기";
    }

    public void OnInteract()
    {
        if (!isPlayerHiding)
            EnterHiding();
    }

    void EnterHiding()
    {
        if (playerController == null || hideViewPoint == null) return;

        // 원래 서 있던 위치 저장 후 이동 제어 끄기
        savedExitPosition = playerController.transform.position;
        if (charController != null) charController.enabled = false;
        playerController.enabled = false;

        // 옷장 시점으로 플레이어 이동 및 방향 정렬
        playerController.transform.position = hideViewPoint.position;
        playerController.transform.rotation = hideViewPoint.rotation;

        isPlayerHiding = true;
        Debug.Log("옷장에 숨었습니다. 적이 가까워지면 [Space]를 눌러 숨을 참으세요! ([E] 나가기)");
    }

    void ExitHiding()
    {
        isPlayerHiding = false;
        isHoldingBreath = false;

        // 원래 위치로 복귀 후 이동 제어 다시 켜기
        playerController.transform.position = savedExitPosition;
        playerController.enabled = true;
        if (charController != null) charController.enabled = true;

        Debug.Log("옷장에서 나왔습니다.");
    }
}