using UnityEngine;

public class SearchDrawer : MonoBehaviour, IInteractable
{
    [Header("서랍이 앞으로 나오는 거리 및 속도")]
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
        return isOpen ? "[E] 서랍 닫기" : "[E] 서랍 열기";
    }

    public void OnInteract()
    {
        isOpen = !isOpen;
    }
}