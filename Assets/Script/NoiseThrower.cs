using UnityEngine;

public class NoiseThrower : MonoBehaviour, IInteractable
{
    [Header("체크하면 맵에 놓인 '줍는 아이템', 해제하면 카메라의 '투척 기능'")]
    public bool isPickupItem = false;

    [Header("투척 보유량 및 설정 (카메라에 붙일 때 설정)")]
    public int throwableCount = 2; // 시작 시 기본 2개 보유
    public float throwForce = 12f;
    public float noiseRadius = 20f;
    public EnemyAI targetEnemy;

    [Header("아이템일 때 연결할 카메라의 NoiseThrower")]
    public NoiseThrower playerThrower;

    void Update()
    {
        if (isPickupItem) return;

        // G 키를 눌러 소음 유인물 투척
        if (Input.GetKeyDown(KeyCode.G) && throwableCount > 0)
        {
            throwableCount--;
            ThrowObject();
        }
    }

    void ThrowObject()
    {
        // 작은 구체(플라스크 대용)를 즉시 생성하여 전방으로 투척
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.localScale = Vector3.one * 0.25f;
        projectile.transform.position = transform.position + transform.forward * 0.8f;

        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        ThrownNoiseMaker marker = projectile.AddComponent<ThrownNoiseMaker>();
        marker.targetEnemy = targetEnemy;
        marker.noiseRadius = noiseRadius;

        Debug.Log($"유인물 투척! (남은 개수: {throwableCount}개)");
    }

    public string GetInteractPrompt()
    {
        return isPickupItem ? "[E] 유인용 빈 플라스크 줍기" : "";
    }

    public void OnInteract()
    {
        if (isPickupItem && playerThrower != null)
        {
            playerThrower.throwableCount++;
            Debug.Log($"빈 플라스크 획득! (보유: {playerThrower.throwableCount}개)");
            gameObject.SetActive(false);
        }
    }
}

// 던져진 물체가 바닥이나 벽에 부딪힐 때 소음을 발생시키는 보조 클래스
public class ThrownNoiseMaker : MonoBehaviour
{
    public EnemyAI targetEnemy;
    public float noiseRadius = 20f;
    private bool hasTriggered = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;
        hasTriggered = true;

        if (targetEnemy != null)
        {
            targetEnemy.HearExternalNoise(transform.position, noiseRadius);
        }

        Destroy(gameObject, 1.5f); // 1.5초 뒤 파편 제거
    }
    void Start()
    {
        // 맵에 놓인 파밍용 플라스크일 때 카메라의 투척기를 자동으로 찾아 연결
        if (isPickupItem && playerThrower == null)
        {
            foreach (NoiseThrower nt in FindObjectsByType<NoiseThrower>(FindObjectsSortMode.None))
            {
                if (!nt.isPickupItem)
                {
                    playerThrower = nt;
                    break;
                }
            }
        }
        if (!isPickupItem && targetEnemy == null)
        {
            targetEnemy = FindAnyObjectByType<EnemyAI>();
        }
    }
}