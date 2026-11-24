using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("연결할 조명과 현재 상태")]
    public Light targetLight;
    public bool isOn = false;

    void Start()
    {
        if (targetLight != null)
            targetLight.enabled = isOn;
    }

    public string GetInteractPrompt()
    {
        return isOn ? "[E] 불 끄기" : "[E] 불 켜기";
    }

    public void OnInteract()
    {
        isOn = !isOn; // 켜짐/꺼짐 상태 반전
        if (targetLight != null)
            targetLight.enabled = isOn;

        Debug.Log(isOn ? "조명이 켜졌습니다." : "조명이 꺼졌습니다.");
    }
}