using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Investigate, Chase, Stunned }

    [Header("현재 AI 상태")]
    public AIState currentState = AIState.Patrol;

    [Header("타겟 및 순찰 지점 연결")]
    public FirstPersonController playerController;
    public HidingSpot closetHidingSpot; // 플레이어가 숨는 옷장 (선택)
    public Transform[] patrolPoints;

    [Header("이동 속도 설정")]
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.5f;

    [Header("시야 및 청각 감지 설정")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public float hearingMultiplier = 1.5f; // 소음 수치 1당 감지 거리 배율
    public float catchDistance = 1.5f;     // 잡힘(게임오버) 판정 거리

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private Vector3 investigatePosition;
    private float stateTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        MoveToNextPatrolPoint();
    }

    void Update()
    {
        if (playerController == null) return;

        // 1. 기절 상태일 때는 타이머가 끝날 때까지 정지
        if (currentState == AIState.Stunned)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                Debug.Log("적이 기절에서 깨어났습니다!");
                currentState = AIState.Patrol;
                agent.isStopped = false;
                MoveToNextPatrolPoint();
            }
            return;
        }

        // 2. 매 프레임 시야와 소음 감지 실행
        bool canSee = CanSeePlayer();
        bool canHear = CanHearPlayer();

        if (canSee)
        {
            currentState = AIState.Chase;
        }
        else if (canHear && currentState != AIState.Chase)
        {
            // 시야에 없지만 소리가 들리면 소리가 난 위치로 수색 이동
            investigatePosition = playerController.transform.position;
            currentState = AIState.Investigate;
            stateTimer = 4.0f; // 4초 동안 해당 위치 주변 수색
        }

        // 3. 상태별 행동 처리
        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Investigate:
                UpdateInvestigate();
                break;
            case AIState.Chase:
                UpdateChase(canSee, canHear);
                break;
        }
    }

    void UpdatePatrol()
    {
        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextPatrolPoint();
        }
    }

    void UpdateInvestigate()
    {
        agent.speed = patrolSpeed * 1.2f;
        agent.SetDestination(investigatePosition);

        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                Debug.Log("수색 종료. 다시 순찰로 돌아갑니다.");
                currentState = AIState.Patrol;
                MoveToNextPatrolPoint();
            }
        }
    }

    void UpdateChase(bool canSee, bool canHear)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerController.transform.position);

        float distToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

        // 플레이어가 옷장에 숨어있고 숨을 참아 소음이 0이라면 추격 해제
        bool isSafelyHidden = (closetHidingSpot != null && closetHidingSpot.isPlayerHiding && playerController.currentNoiseLevel <= 0.1f);

        if (isSafelyHidden)
        {
            Debug.Log("플레이어가 숨을 참아 적이 위치를 놓쳤습니다!");
            currentState = AIState.Investigate;
            investigatePosition = transform.position;
            stateTimer = 3.0f;
            return;
        }

        // 잡힘 판정
        if (distToPlayer <= catchDistance)
        {
            Debug.Log("적에게 붙잡혔습니다! [GAME OVER]");
            Time.timeScale = 0f; // 게임 일시정지
        }

        // 시야와 소리를 모두 놓치면 마지막 위치만 수색 후 복귀
        if (!canSee && !canHear && distToPlayer > viewDistance)
        {
            investigatePosition = playerController.transform.position;
            currentState = AIState.Investigate;
            stateTimer = 3.0f;
        }
    }

    bool CanSeePlayer()
    {
        // 옷장에 숨어있고 소음을 내지 않으면 시야에서 제외
        if (closetHidingSpot != null && closetHidingSpot.isPlayerHiding && playerController.currentNoiseLevel <= 2f)
            return false;

        Vector3 dirToPlayer = playerController.transform.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;

        if (distToPlayer <= viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle <= viewAngle * 0.5f)
            {
                // 적과 플레이어 사이에 벽이나 장애물이 없는지 Raycast로 확인
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, out RaycastHit hit, viewDistance))
                {
                    if (hit.collider.GetComponent<FirstPersonController>() != null)
                        return true;
                }
            }
        }
        return false;
    }

    bool CanHearPlayer()
    {
        float noise = playerController.currentNoiseLevel;
        if (noise <= 0f) return false;

        float distToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
        float maxHearDistance = noise * hearingMultiplier;

        if (distToPlayer <= maxHearDistance)
        {
            Debug.Log($"적이 소음을 감지했습니다! (소음 수치: {noise})");
            return true;
        }
        return false;
    }

    void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // 4단계 FlashlightController의 SendMessage("OnStunned")로 호출되는 함수
    public void OnStunned()
    {
        currentState = AIState.Stunned;
        stateTimer = 4.0f; // 4초간 기절
        agent.isStopped = true;
        Debug.Log("적이 손전등 섬광에 맞아 4초간 기절했습니다!");
    }
    // 투척물이 바닥에 떨어졌을 때 해당 위치로 적을 유인하는 함수
    public void HearExternalNoise(Vector3 noisePosition, float noiseRadius)
    {
        if (currentState == AIState.Stunned || currentState == AIState.Chase) return;

        float dist = Vector3.Distance(transform.position, noisePosition);
        if (dist <= noiseRadius)
        {
            investigatePosition = noisePosition;
            currentState = AIState.Investigate;
            stateTimer = 5.0f; // 5초간 물체가 떨어진 지점 수색
            Debug.Log("실험체가 투척물 파열음을 듣고 해당 위치로 이동합니다!");
        }
    }
}