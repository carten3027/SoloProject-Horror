using UnityEngine;

public class LabComputer : MonoBehaviour, IInteractable
{
    [Header("플레이어 및 원격 제어할 미닫이문 연결")]
    public FirstPersonController playerController;
    public SlidingDoor remoteDoor; // 컴퓨터로 원격 개방할 미닫이 보안문

    [Header("실시간 CCTV 카메라 목록 연결")]
    public Camera[] cctvCameras;
    public string[] cctvLocationNames = { "CAM 01 - 제1연구실 복도", "CAM 02 - 지하 발전기실" };

    [Header("컴퓨터에 저장된 연구 일지 내용")]
    public string logTitle = "[기밀] 실험체 09호 격리 보고서";
    [TextArea(4, 8)]
    public string logContent =
        "1. 실험체 09호는 암실 격리로 인해 시각을 상실했으나 청각이 기형적으로 발달함.\n" +
        "2. 달리는 발소리나 발전기 가동음이 발생하면 즉시 해당 위치로 접근함.\n" +
        "3. [대응책] 손전등 배터리가 80% 이상일 때 고출력 섬광(좌클릭)을 정면으로 비추면 시신경 과부하로 4초간 마비됨.\n" +
        "4. 추격당할 경우 철제 캐비닛에 숨어 숨을 참으면(Space) 실험체의 추적을 따돌릴 수 있음.";

    private bool isUsingComputer = false;
    private int currentTab = 0; // 0: 연구 일지, 1: 실시간 CCTV, 2: 원격 보안문 제어
    private int currentCamIndex = 0;
    private bool isDoorUnlockedRemotely = false;
    private int openedFrame = -1;

    private RenderTexture cctvRenderTexture;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;

    void Start()
    {
        // 코드로 CCTV 화면용 렌더 텍스처 자동 생성 (해상도 640x360)
        cctvRenderTexture = new RenderTexture(640, 360, 16);

        if (cctvCameras != null)
        {
            for (int i = 0; i < cctvCameras.Length; i++)
            {
                if (cctvCameras[i] != null)
                {
                    // 혹시 남아있는 AudioListener가 있다면 자동 제거
                    AudioListener listener = cctvCameras[i].GetComponent<AudioListener>();
                    if (listener != null) Destroy(listener);

                    cctvCameras[i].targetTexture = cctvRenderTexture;
                    cctvCameras[i].enabled = false; // 컴퓨터를 켤 때만 작동시켜 성능 절약
                }
            }
        }
    }

    void Update()
    {
        if (!isUsingComputer) return;

        // 컴퓨터를 켠 첫 프레임에 바로 닫히는 현상 방지
        if (Time.frameCount == openedFrame) return;

        // 1. Q 키 또는 E 키로 컴퓨터 종료
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
        {
            CloseComputer();
            return;
        }

        // 2. 숫자키 1, 2, 3으로 메뉴 탭 빠른 전환
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTab(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTab(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTab(2);

        // 3. CCTV 탭일 때 A/D 또는 좌/우 방향키로 카메라 채널 변경
        if (currentTab == 1 && cctvCameras != null && cctvCameras.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                ChangeCCTVCamera(1);
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeCCTVCamera(-1);
        }

        // 4. 원격 제어 탭일 때 Space 또는 Enter로 보안문 잠금 해제
        if (currentTab == 2 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            UnlockRemoteDoor();
        }
    }

    public string GetInteractPrompt()
    {
        return isUsingComputer ? "" : "[E] 통제실 보안 컴퓨터 접속";
    }

    public void OnInteract()
    {
        if (!isUsingComputer)
            OpenComputer();
    }

    void OpenComputer()
    {
        isUsingComputer = true;
        openedFrame = Time.frameCount;

        // 컴퓨터 사용 중 플레이어 이동 정지 및 마우스 커서 표시
        if (playerController != null)
        {
            playerController.currentNoiseLevel = 0f;
            playerController.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SwitchTab(currentTab);
    }

    void CloseComputer()
    {
        isUsingComputer = false;
        DisableAllCCTVs();

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SwitchTab(int tabIndex)
    {
        currentTab = tabIndex;
        if (currentTab == 1)
            UpdateActiveCCTV();
        else
            DisableAllCCTVs();
    }

    void ChangeCCTVCamera(int direction)
    {
        if (cctvCameras == null || cctvCameras.Length == 0) return;

        currentCamIndex = (currentCamIndex + direction + cctvCameras.Length) % cctvCameras.Length;
        UpdateActiveCCTV();
    }

    void UpdateActiveCCTV()
    {
        DisableAllCCTVs();
        if (cctvCameras != null && cctvCameras.Length > 0 && cctvCameras[currentCamIndex] != null)
        {
            cctvCameras[currentCamIndex].enabled = true;
        }
    }

    void DisableAllCCTVs()
    {
        if (cctvCameras == null) return;
        foreach (Camera cam in cctvCameras)
        {
            if (cam != null) cam.enabled = false;
        }
    }

    void UnlockRemoteDoor()
    {
        if (remoteDoor == null) return;

        remoteDoor.requiresCardKey = false;
        remoteDoor.SupplyPower();
        if (!isDoorUnlockedRemotely)
        {
            remoteDoor.OnInteract(); // 즉시 문 열기
            isDoorUnlockedRemotely = true;
        }
    }

    void InitGUIStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = new Color(0.3f, 1f, 0.5f); // 연구소 터미널 초록색

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            wordWrap = true
        };
        bodyStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
    }

    void OnGUI()
    {
        if (!isUsingComputer) return;
        InitGUIStyles();

        float winW = 720f;
        float winH = 520f;
        float winX = (Screen.width - winW) * 0.5f;
        float winY = (Screen.height - winH) * 0.5f;

        // 모니터 배경 박스
        GUI.Box(new Rect(winX, winY, winW, winH), "");
        GUI.Box(new Rect(winX, winY, winW, winH), ""); // 더 진하게 표시

        GUI.Label(new Rect(winX, winY + 15, winW, 35), "■ 지하 연구동 중앙 보안 터미널 ■", titleStyle);

        // 상단 탭 버튼 (마우스 클릭 또는 숫자키 1, 2, 3)
        if (GUI.Button(new Rect(winX + 30, winY + 60, 200, 35), "[1] 연구 일지 열람", buttonStyle))
            SwitchTab(0);
        if (GUI.Button(new Rect(winX + 260, winY + 60, 200, 35), "[2] 실시간 CCTV 감시", buttonStyle))
            SwitchTab(1);
        if (GUI.Button(new Rect(winX + 490, winY + 60, 200, 35), "[3] 보안 게이트 제어", buttonStyle))
            SwitchTab(2);

        // 탭 0: 연구 일지
        if (currentTab == 0)
        {
            GUI.Label(new Rect(winX + 40, winY + 120, winW - 80, 30), logTitle, titleStyle);
            GUI.Label(new Rect(winX + 45, winY + 165, winW - 90, 260), logContent, bodyStyle);
        }
        // 탭 1: 실시간 CCTV 화면
        else if (currentTab == 1)
        {
            string camName = (cctvLocationNames != null && currentCamIndex < cctvLocationNames.Length)
                ? cctvLocationNames[currentCamIndex]
                : $"CAM 0{currentCamIndex + 1}";

            GUI.Label(new Rect(winX + 40, winY + 105, winW - 80, 30),
                $"● REC : {camName}  ([A/D] 또는 [←/→] 채널 변경)", titleStyle);

            // CCTV 실시간 영상 출력
            Rect screenRect = new Rect(winX + 60, winY + 140, 600, 310);
            if (cctvCameras != null && cctvCameras.Length > 0 && cctvCameras[currentCamIndex] != null)
            {
                GUI.DrawTexture(screenRect, cctvRenderTexture, ScaleMode.StretchToFill);
            }
            else
            {
                GUI.Box(screenRect, "연결된 CCTV 카메라가 없습니다.");
            }
        }
        // 탭 2: 원격 보안문 제어
        else if (currentTab == 2)
        {
            string status = isDoorUnlockedRemotely
                ? "현재 상태 : [보안 게이트 잠금 해제 및 개방 완료]"
                : "현재 상태 : [보안 게이트 봉쇄 중]";

            GUI.Label(new Rect(winX + 40, winY + 160, winW - 80, 40), status, titleStyle);

            if (GUI.Button(new Rect(winX + 180, winY + 230, 360, 55), "[Space / 클릭] 보안 게이트 원격 개방", buttonStyle))
            {
                UnlockRemoteDoor();
            }
        }

        // 하단 종료 버튼
        if (GUI.Button(new Rect(winX + 260, winY + 465, 200, 38), "[Q] 또는 [E] 컴퓨터 종료", buttonStyle))
        {
            CloseComputer();
        }
    }
}