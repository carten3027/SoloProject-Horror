using System.Collections;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("손전등 연결")]
    public Light flashlight;

    [Header("배터리 및 파밍 설정")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float drainSpeed = 3.5f;   // 초당 배터리 소모량
    public int spareBatteries = 1;    // 보유 중인 여분 배터리 개수

    [Header("섬광 기절(Stun) 설정")]
    public float stunRange = 8f;
    private float normalIntensity = 3f;
    private bool isOn = true;

    void Start()
    {
        if (flashlight != null)
        {
            normalIntensity = flashlight.intensity;
            flashlight.enabled = isOn;
        }
    }

    void Update()
    {
        // 1. F 키로 손전등 켜기 / 끄기
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;
            if (flashlight != null)
                flashlight.enabled = isOn && (currentBattery > 0);
        }

        // 2. R 키로 여분 배터리 교체
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadBattery();
        }

        // 3. 켜져 있을 때 배터리 소모
        if (isOn && currentBattery > 0)
        {
            currentBattery -= drainSpeed * Time.deltaTime;
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                if (flashlight != null) flashlight.enabled = false;
                Debug.Log("배터리가 방전되었습니다! [R] 키로 여분 배터리를 교체하세요.");
            }
        }

        // 4. 배터리가 80% 이상일 때 마우스 좌클릭으로 실험체 섬광 기절 발사
        if (Input.GetMouseButtonDown(0) && currentBattery >= 80f)
        {
            FireStunFlash();
        }
    }

    void ReloadBattery()
    {
        if (spareBatteries <= 0)
        {
            Debug.Log("여분 배터리가 없습니다! 연구실 서랍을 탐색하세요.");
            return;
        }
        if (currentBattery >= 99f) return;

        spareBatteries--;
        currentBattery = maxBattery;
        if (flashlight != null && isOn) flashlight.enabled = true;
        Debug.Log($"배터리 교체 완료! (남은 배터리: {spareBatteries}개)");
    }

    void FireStunFlash()
    {
        currentBattery = 0f; // 섬광 발사 시 현재 배터리 전량 소모
        StartCoroutine(FlashEffectRoutine());

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, stunRange))
        {
            hit.collider.SendMessage("OnStunned", SendMessageOptions.DontRequireReceiver);
            Debug.Log($"고출력 섬광 발사! {hit.collider.name} 마비!");
        }
    }

    IEnumerator FlashEffectRoutine()
    {
        if (flashlight != null)
        {
            flashlight.enabled = true;
            flashlight.intensity = normalIntensity * 6f;
            yield return new WaitForSeconds(0.25f);
            flashlight.intensity = normalIntensity;
            flashlight.enabled = false;
        }
    }
}