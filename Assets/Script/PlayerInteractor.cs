using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("상호작용 거리 및 키 설정")]
    public float interactDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    [HideInInspector]
    public string currentPrompt = ""; // 화면 중앙에 표시할 상호작용 문구

    void Update()
    {
        currentPrompt = "";
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // 기존: IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentPrompt = interactable.GetInteractPrompt();

                if (Input.GetKeyDown(interactKey))
                {
                    interactable.OnInteract();
                }
            }
        }
    }
}