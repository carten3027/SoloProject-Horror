using System.Collections;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("보유 장비 상태 (아이템을 주우면 활성화)")]
    public bool hasFlashlight = false;
    public bool hasLighter = false;
    public bool hasCamera = false;

    [Header("조명 및 카메라 연결")]
    public Light flashlight;      // 기존 Spot Light 연결
    public Light lighterLight;    // 라이터용 Point Light (비워두면 자동 생성)
    public Camera playerCam;      // Main Camera (비워두면 자동 연결)

    [Header("손전등 배터리 설정")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float drainSpeed = 1.0f;
    public int spareBatteries = 1;

    [Header("섬광 기절(Stun) 설정")]
    public KeyCode stunKey = KeyCode.V;
    public float stunBatteryCost = 50f;
    public float stunRange = 8f;

    private float normalIntensity = 3f;
    private float defaultFOV = 60f;
    private bool isFlashlightOn = false;
    private bool isLighterOn = false;
    private bool isCameraZoomed = false;

    void Start()
    {
        if (playerCam == null) playerCam = GetComponent<Camera>();
        if (playerCam != null) defaultFOV = playerCam.fieldOfView;

        if (flashlight != null)
        {
            normalIntensity = flashlight.intensity;
            flashlight.enabled = false;
        }

        // 라이터 조명이 따로 없으면 카메라 앞에 작은 주황색 불빛 자동 생성
        if (lighterLight == null)
        {
            GameObject lighterObj = new GameObject("Lighter_PointLight");
            lighterObj.transform.SetParent(transform);
            lighterObj.transform.localPosition = new Vector3(0.2f, -0.1f, 0.3f);
            lighterLight = lighterObj.AddComponent<Light>();
            lighterLight.type = LightType.Point;
            lighterLight.color = new Color(1f, 0.65f, 0.25f); // 따뜻한 불꽃색
            lighterLight.range = 3.5f;
            lighterLight.intensity = 1.8f;
            lighterLight.enabled = false;
        }
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // 1. 손전등 조작 ([F] 켜기/끄기, [R] 배터리 교체, [V] 섬광)
        if (hasFlashlight)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isFlashlightOn = !isFlashlightOn;
                if (isFlashlightOn) isLighterOn = false; // 손전등을 켜면 라이터는 끔
                UpdateLights();
            }

            if (Input.GetKeyDown(KeyCode.R)) ReloadBattery();

            if (isFlashlightOn && currentBattery > 0)
            {
                currentBattery -= drainSpeed * Time.deltaTime;
                if (currentBattery <= 0)
                {
                    currentBattery = 0;
                    UpdateLights();
                }
            }

            bool stunInput = Input.GetKeyDown(stunKey) || Input.GetMouseButtonDown(2);
            if (isFlashlightOn && stunInput && currentBattery >= stunBatteryCost)
            {
                FireStunFlash();
            }
        }

        // 2. 라이터 조작 ([L] 켜기/끄기 - 배터리 소모 없음, 좁은 시야)
        if (hasLighter && Input.GetKeyDown(KeyCode.L))
        {
            isLighterOn = !isLighterOn;
            if (isLighterOn) isFlashlightOn = false;
            UpdateLights();
        }

        // 3. 카메라 조작 ([C] 줌인 확대 / 원래대로)
        if (hasCamera && Input.GetKeyDown(KeyCode.C) && playerCam != null)
        {
            isCameraZoomed = !isCameraZoomed;
            playerCam.fieldOfView = isCameraZoomed ? 28f : defaultFOV;
            Debug.Log(isCameraZoomed ? "카메라 뷰파인더 확대(Zoom-In)" : "카메라 확대 해제");
        }
    }

    void UpdateLights()
    {
        if (flashlight != null)
            flashlight.enabled = isFlashlightOn && (currentBattery > 0);

        if (lighterLight != null)
            lighterLight.enabled = isLighterOn;
    }

    public void AcquireEquipment(FlashlightItem.EquipmentType type)
    {
        if (type == FlashlightItem.EquipmentType.Flashlight)
        {
            hasFlashlight = true;
            isFlashlightOn = true;
            isLighterOn = false;
            Debug.Log("손전등 획득! [F] 점등 / [V] 섬광 기절");
        }
        else if (type == FlashlightItem.EquipmentType.Lighter)
        {
            hasLighter = true;
            isLighterOn = true;
            isFlashlightOn = false;
            Debug.Log("라이터 획득! [L] 키로 배터리 소모 없이 주변을 밝힙니다.");
        }
        else if (type == FlashlightItem.EquipmentType.Camera)
        {
            hasCamera = true;
            Debug.Log("캠코더 카메라 획득! [C] 키로 멀리 있는 단서를 확대해 볼 수 있습니다.");
        }
        UpdateLights();
    }

    void ReloadBattery()
    {
        if (spareBatteries <= 0 || currentBattery >= 99f) return;
        spareBatteries--;
        currentBattery = maxBattery;
        UpdateLights();
    }

    void FireStunFlash()
    {
        currentBattery -= stunBatteryCost;
        StartCoroutine(FlashEffectRoutine());

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, stunRange))
        {
            hit.collider.SendMessageUpwards("OnStunned", SendMessageOptions.DontRequireReceiver);
        }
    }

    IEnumerator FlashEffectRoutine()
    {
        if (flashlight != null)
        {
            flashlight.intensity = normalIntensity * 6f;
            yield return new WaitForSeconds(0.25f);
            flashlight.intensity = normalIntensity;
            UpdateLights();
        }
    }
}