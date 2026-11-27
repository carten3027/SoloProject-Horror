using UnityEngine;

public class HorrorHUD : MonoBehaviour
{
    [Header("상태를 표시할 컴포넌트 연결")]
    public FirstPersonController playerController;
    public PlayerInteractor playerInteractor;
    public FlashlightController flashlightController;
    public NoiseThrower playerThrower; // Main Camera에 붙인 투척 컴포넌트
    public HidingSpot closetHidingSpot;

    private bool showControls = true; // Tab 키로 조작법 표시 켜기/끄기
    private GUIStyle centerPromptStyle;
    private GUIStyle statusTextStyle;
    private GUIStyle warningTextStyle;
    private GUIStyle guideTextStyle;
    private GUIStyle gameOverStyle;

    void Update()
    {
        // Tab 키를 누르면 우측 상단 조작법 창을 숨기거나 다시 띄움
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showControls = !showControls;
        }
    }

    void InitStyles()
    {
        if (centerPromptStyle != null) return;

        centerPromptStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        centerPromptStyle.normal.textColor = Color.white;

        statusTextStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
        statusTextStyle.normal.textColor = Color.white;

        warningTextStyle = new GUIStyle(statusTextStyle);
        warningTextStyle.normal.textColor = new Color(1f, 0.35f, 0.35f); // 붉은 경고색

        guideTextStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Normal
        };
        guideTextStyle.normal.textColor = new Color(0.85f, 0.9f, 1f);

        gameOverStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 48,
            fontStyle = FontStyle.Bold
        };
        gameOverStyle.normal.textColor = Color.red;
    }

    void OnGUI()
    {
        InitStyles();

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        // 1. 화면 정중앙 조준점 (+)
        GUI.Label(new Rect(centerX - 15, centerY - 15, 30, 30), "+", centerPromptStyle);

        // 2. 상호작용 안내 문구 표시
        if (playerInteractor != null && !string.IsNullOrEmpty(playerInteractor.currentPrompt))
        {
            GUI.Label(new Rect(centerX - 250, centerY + 35, 500, 40), playerInteractor.currentPrompt, centerPromptStyle);
        }

        // 3. 좌측 하단 플레이어 상태 및 인벤토리 창
        float boxHeight = 165f;
        float startY = Screen.height - boxHeight - 15f;
        GUI.Box(new Rect(15, startY, 310, boxHeight), "");

        float lineY = startY + 10f;

        if (playerController != null)
        {
            string staminaText = $"스태미나 : {Mathf.FloorToInt(playerController.currentStamina)}%";
            if (playerController.isExhausted)
            {
                staminaText += " [탈진 - 회복 대기]";
                GUI.Label(new Rect(25, lineY, 290, 25), staminaText, warningTextStyle);
            }
            else
            {
                GUI.Label(new Rect(25, lineY, 290, 25), staminaText, statusTextStyle);
            }
            lineY += 25f;

            GUI.Label(new Rect(25, lineY, 290, 25), $"현재 소음 수치 : {playerController.currentNoiseLevel:F0}", statusTextStyle);
            lineY += 25f;
        }

        if (flashlightController != null)
        {
            if (!flashlightController.hasFlashlight)
            {
                GUI.Label(new Rect(25, lineY, 290, 25), $"손전등 : 미보유 (여분 배터리: {flashlightController.spareBatteries}개)", warningTextStyle);
            }
            else
            {
                string flashReady = (flashlightController.currentBattery >= flashlightController.stunBatteryCost) ? " (섬광 가능!)" : "";
                GUI.Label(new Rect(25, lineY, 290, 25),
                    $"배터리 : {Mathf.FloorToInt(flashlightController.currentBattery)}%{flashReady} | 여분 : {flashlightController.spareBatteries}개",
                    statusTextStyle);
            }
            lineY += 25f;
        }

        if (playerThrower != null)
        {
            GUI.Label(new Rect(25, lineY, 290, 25), $"유인용 플라스크 : {playerThrower.throwableCount}개", statusTextStyle);
            lineY += 25f;
        }

        // 획득한 카드키 표시
        string keysText = KeyItem.collectedKeys.Count > 0
            ? string.Join(", ", KeyItem.collectedKeys)
            : "없음";
        GUI.Label(new Rect(25, lineY, 290, 25), $"보유 카드키 : {keysText}", statusTextStyle);
        lineY += 25f;

        if (closetHidingSpot != null && closetHidingSpot.isPlayerHiding)
        {
            GUI.Label(new Rect(25, lineY, 290, 25), $"남은 호흡 : {closetHidingSpot.currentBreath:F1}초", warningTextStyle);
        }

        // 4. 우측 상단 조작법 가이드 ([Tab] 키로 토글)
        if (showControls)
        {
            float guideWidth = 310f;
            float guideX = Screen.width - guideWidth - 15f;
            GUI.Box(new Rect(guideX, 15, guideWidth, 205), "");

            GUI.Label(new Rect(guideX + 12, 22, 290, 25), "■ 연구동 생존 조작법 ([Tab] 숨기기)", statusTextStyle);
            GUI.Label(new Rect(guideX + 12, 50, 290, 22), "• 이동 / 시선 : [WASD] / [마우스·방향키]", guideTextStyle);
            GUI.Label(new Rect(guideX + 12, 72, 290, 22), "• 달리기 : [Left Shift] (소음 증가)", guideTextStyle);
            GUI.Label(new Rect(guideX + 12, 94, 290, 22), "• 조사 / 작동 : [E] (문·서랍·발전기·은신)", guideTextStyle);
            GUI.Label(new Rect(guideX + 12, 116, 290, 22), "• 손전등 점등 / 배터리 교체 : [F] / [R]", guideTextStyle);
            GUI.Label(new Rect(guideX + 12, 138, 290, 22), "• 실험체 섬광 기절 : [좌클릭] (배터리 80%↑)", guideTextStyle);
            GUI.Label(new Rect(guideX + 12, 160, 290, 22), "• 소음 유인물 투척 : [G]", guideTextStyle);
            GUI.Label(new Rect(guideX + 12, 182, 290, 22), "• 은신 중 숨 참기 : [Space 홀드]", guideTextStyle);
        }

        // 5. 게임 오버 화면
        if (Time.timeScale == 0f)
        {
            GUI.Label(new Rect(centerX - 300, centerY - 100, 600, 80), "GAME OVER", gameOverStyle);
        }
    }
}