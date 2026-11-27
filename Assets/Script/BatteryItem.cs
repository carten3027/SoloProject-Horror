using UnityEngine;

public class BatteryItem : MonoBehaviour, IInteractable
{
    public FlashlightController flashlightController;

    public string GetInteractPrompt()
    {
        return "[E] 손전등 배터리 줍기";
    }
    void Start()
    {
        if (flashlightController == null)
            flashlightController = FindAnyObjectByType<FlashlightController>();
    }
    public void OnInteract()
    {
        if (flashlightController != null)
        {
            flashlightController.spareBatteries++;
            Debug.Log($"배터리 획득! (보유 배터리: {flashlightController.spareBatteries}개)");
            gameObject.SetActive(false);
        }
    }
}