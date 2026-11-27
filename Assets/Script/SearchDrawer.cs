using UnityEngine;

public class SearchDrawer : MonoBehaviour, IInteractable
{
    [Header("잠금 및 필요 부품 설정 (작은 열쇠, 밸브, 크랭크 등)")]
    public bool isLocked = false;
    public string requiredKeyID = "SmallKey";
    public string lockedPrompt = "[E] 열기 (작은 열쇠 필요)";
    public string openPrompt = "[E] 서랍 열기";
    public string closePrompt = "[E] 서랍 닫기";

    [Header("이동 거리 및 속도")]
    public Vector3 slideOffset = new Vector3(0, 0, 0.5f);
    public float slideSpeed = 4f;

    private bool isOpen = false;
    private Vector3 closedPos;
    private Vector3 openPos;

    void Start()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + slideOffset;
    }

    void Update()
    {
        Vector3 targetPos = isOpen ? openPos : closedPos;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * slideSpeed);
    }

    public string GetInteractPrompt()
    {
        if (isLocked) return lockedPrompt;
        return isOpen ? closePrompt : openPrompt;
    }

    public void OnInteract()
    {
        if (isLocked)
        {
            if (KeyItem.collectedKeys.Contains(requiredKeyID))
            {
                isLocked = false;
                Debug.Log($"{requiredKeyID}를 사용하여 잠금을 해제했습니다!");
            }
            else
            {
                Debug.Log($"{requiredKeyID} 아이템이 필요합니다.");
                return;
            }
        }
        isOpen = !isOpen;
    }
}