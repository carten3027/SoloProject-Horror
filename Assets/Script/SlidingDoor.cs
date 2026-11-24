using UnityEngine;

public class SlidingDoor : MonoBehaviour, IInteractable
{
    [Header("보안 카드키 및 발전기 전력 조건")]
    public bool requiresCardKey = true;
    public string requiredKeyID = "LabCardKey_Lv1";
    public bool requiresGeneratorPower = false;
    public bool hasPower = false;

    [Header("미닫이 이동 거리 (옆문: X=1.5 / 상하 셔터: Y=2.2)")]
    public Vector3 slideDirectionOffset = new Vector3(1.5f, 0f, 0f);
    public float slideSpeed = 3.5f;

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + transform.TransformDirection(slideDirectionOffset);
    }

    void Update()
    {
        Vector3 targetPos = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * slideSpeed);
    }

    public string GetInteractPrompt()
    {
        if (requiresGeneratorPower && !hasPower)
            return "보안 게이트 전력 차단됨 (비상 발전기 가동 필요)";

        if (requiresCardKey)
            return $"[E] 보안 단말기 태그 ({requiredKeyID} 필요)";

        return isOpen ? "[E] 보안문 닫기" : "[E] 보안문 열기";
    }

    public void OnInteract()
    {
        if (requiresGeneratorPower && !hasPower)
        {
            Debug.Log("전력이 공급되지 않아 미닫이문이 열리지 않습니다.");
            return;
        }

        if (requiresCardKey)
        {
            if (KeyItem.collectedKeys.Contains(requiredKeyID))
            {
                requiresCardKey = false;
                Debug.Log("보안 카드키 승인 완료! 잠금이 해제되었습니다.");
            }
            else
            {
                Debug.Log($"접근 거부: {requiredKeyID} 카드키가 필요합니다.");
                return;
            }
        }

        isOpen = !isOpen;
    }

    public void SupplyPower()
    {
        hasPower = true;
        Debug.Log("미닫이 보안 게이트에 비상 전력이 공급되었습니다!");
    }
}