using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    [Header("발전기 가동 설정")]
    public float currentProgress = 0f;
    public float chargeSpeed = 25f; // 초당 충전량 (4초면 100%)
    public float generatorNoise = 15f; // 가동 중 발생하는 큰 소음

    [Header("전력 공급 대상 연결")]
    public Light emergencyLight;
    public SlidingDoor poweredDoor;
    public FirstPersonController playerController;

    private bool isRunning = false;
    private bool isCompleted = false;

    void Update()
    {
        if (isRunning && !isCompleted)
        {
            currentProgress += chargeSpeed * Time.deltaTime;

            // 발전기가 돌아가는 동안 플레이어의 소음 수치를 높여 적의 주의를 끔
            if (playerController != null)
                playerController.currentNoiseLevel = generatorNoise;

            Debug.Log($"발전기 가동 중... {Mathf.FloorToInt(currentProgress)}%");

            if (currentProgress >= 100f)
            {
                CompleteGenerator();
            }
        }
    }

    public string GetInteractPrompt()
    {
        if (isCompleted) return "발전기가 정상 가동 중입니다.";
        return isRunning ? "[E] 발전기 가동 중지" : "[E] 발전기 가동 시작";
    }

    public void OnInteract()
    {
        if (isCompleted) return;
        isRunning = !isRunning;
    }

    void CompleteGenerator()
    {
        currentProgress = 100f;
        isCompleted = true;
        isRunning = false;

        if (emergencyLight != null)
            emergencyLight.enabled = true;

        if (poweredDoor != null)
            poweredDoor.SupplyPower();

        Debug.Log("발전기 가동 완료! 모든 전력이 복구되었습니다.");
    }
}