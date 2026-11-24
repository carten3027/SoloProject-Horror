using System.Collections.Generic;
using UnityEngine;

public class KeyItem : MonoBehaviour, IInteractable
{
    // 플레이어가 획득한 열쇠 목록을 공용으로 관리
    public static HashSet<string> collectedKeys = new HashSet<string>();

    [Header("열쇠 고유 이름 (문과 이름을 맞춰주세요)")]
    public string keyID = "GeneratorRoomKey";
    public string keyDisplayName = "발전기실 열쇠";

    public string GetInteractPrompt()
    {
        return $"[E] {keyDisplayName} 줍기";
    }

    public void OnInteract()
    {
        collectedKeys.Add(keyID);
        Debug.Log($"{keyDisplayName}를 획득했습니다!");
        gameObject.SetActive(false); // 주운 열쇠는 화면에서 숨김
    }
}