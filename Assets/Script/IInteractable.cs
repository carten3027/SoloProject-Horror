public interface IInteractable
{
    // 화면에 띄울 안내 문구 (예: "[E] 스위치 켜기", "[E] 발전기 가동")
    string GetInteractPrompt();

    // 플레이어가 상호작용 키(E)를 눌렀을 때 실행될 동작
    void OnInteract();
}