using UnityEngine;

public class FlashlightItem : MonoBehaviour, IInteractable
{
    public enum EquipmentType { Flashlight, Lighter, Camera }

    [Header("아이템 종류 선택")]
    public EquipmentType equipmentType = EquipmentType.Flashlight;
    public string itemName = "보안용 손전등";

    private FlashlightController controller;

    void Start()
    {
        controller = FindAnyObjectByType<FlashlightController>();
    }

    public string GetInteractPrompt()
    {
        return $"[E] {itemName} 줍기";
    }

    public void OnInteract()
    {
        if (controller != null)
        {
            controller.AcquireEquipment(equipmentType);
            gameObject.SetActive(false);
        }
    }
}