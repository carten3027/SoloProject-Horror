using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    [Header("발전기 수리 부품 조건 (기어 & 케이블)")]
    public bool requiresRepair = true;
    public string requiredPart1 = "Gear";
    public string requiredPart2 = "Cable";

    [Header("발전기 가동 설정")]
    public float currentProgress = 0f;
    public float chargeSpeed = 25f;
    public float generatorNoise = 15f;

    [Header("전력 공급 대상 연결")]
    public Light emergencyLight;
    public SlidingDoor poweredDoor;
    public FirstPersonController playerController;

    private bool isRunning = false;
    private bool isCompleted = false;

    void Start()
    {
        if (playerController == null)
            playerController = FindAnyObjectByType<FirstPersonController>();

        if (poweredDoor != null)
            poweredDoor.requiresGeneratorPower = true;
    }

    void Update()
    {
        if (isRunning && !isCompleted)
        {
            currentProgress += chargeSpeed * Time.deltaTime;

            if (playerController != null)
                playerController.currentNoiseLevel = generatorNoise;

            if (currentProgress >= 100f)
                CompleteGenerator();
        }
    }

    public string GetInteractPrompt()
    {
        if (isCompleted) return "발전기가 정상 가동 중입니다.";
        if (requiresRepair) return $"[E] 발전기 수리하기 ({requiredPart1}, {requiredPart2} 필요)";

        return isRunning
            ? $"[E] 발전기 가동 중지 ({Mathf.FloorToInt(currentProgress)}%)"
            : $"[E] 발전기 가동 시작 ({Mathf.FloorToInt(currentProgress)}%)";
    }

    public void OnInteract()
    {
        if (isCompleted) return;

        // 수리가 필요한 상태라면 기어와 케이블 보유 여부 확인
        if (requiresRepair)
        {
            bool hasPart1 = string.IsNullOrEmpty(requiredPart1) || KeyItem.collectedKeys.Contains(requiredPart1);
            bool hasPart2 = string.IsNullOrEmpty(requiredPart2) || KeyItem.collectedKeys.Contains(requiredPart2);

            if (hasPart1 && hasPart2)
            {
                requiresRepair = false;
                Debug.Log("기어와 케이블을 장착해 발전기 수리를 완료했습니다! 이제 가동할 수 있습니다.");
            }
            else
            {
                Debug.Log($"부품 부족: {requiredPart1} 및 {requiredPart2}을(를) 찾아오세요.");
            }
            return;
        }

        isRunning = !isRunning;
    }

    void CompleteGenerator()
    {
        currentProgress = 100f;
        isCompleted = true;
        isRunning = false;

        if (emergencyLight != null) emergencyLight.enabled = true;
        if (poweredDoor != null) poweredDoor.SupplyPower();

        Debug.Log("발전기 가동 완료! 보안 게이트에 전력이 복구되었습니다.");
    }
}