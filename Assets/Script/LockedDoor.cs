using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [Header("경첩(회전축) 연결 - 비워두면 부모 오브젝트를 자동 사용")]
    public Transform doorHinge;

    [Header("잠금 조건 설정")]
    public bool isLockedByKey = true;
    public string requiredKeyID = "GeneratorRoomKey";
    public bool requiresGeneratorPower = false;
    public bool hasPower = false;

    [Header("문 회전 설정")]
    public float openAngle = 90f;
    public float openSpeed = 3f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        // 인스펙터에 경첩을 안 넣었더라도 부모 오브젝트가 있으면 부모를 회전축으로 사용
        if (doorHinge == null)
            doorHinge = (transform.parent != null) ? transform.parent : transform;

        closedRotation = doorHinge.rotation;
        openRotation = Quaternion.Euler(doorHinge.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorHinge.rotation = Quaternion.Slerp(doorHinge.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    public string GetInteractPrompt()
    {
        if (requiresGeneratorPower && !hasPower)
            return "전력이 끊겨 있습니다. (발전기 가동 필요)";

        if (isLockedByKey)
            return $"[E] 자물쇠 열기 ({requiredKeyID} 필요)";

        return isOpen ? "[E] 문 닫기" : "[E] 문 열기";
    }

    public void OnInteract()
    {
        if (requiresGeneratorPower && !hasPower)
        {
            Debug.Log("전원이 들어오지 않아 문이 작동하지 않습니다.");
            return;
        }

        if (isLockedByKey)
        {
            if (KeyItem.collectedKeys.Contains(requiredKeyID))
            {
                isLockedByKey = false;
                Debug.Log("자물쇠를 열었습니다!");
            }
            else
            {
                Debug.Log("문과 맞는 열쇠가 없습니다.");
                return;
            }
        }

        isOpen = !isOpen;
    }

    public void SupplyPower()
    {
        hasPower = true;
        Debug.Log("문에 전력이 공급되었습니다!");
    }
}