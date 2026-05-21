using UnityEngine;

/// <summary>
/// 카메라 팔로우 컨트롤러
/// - 플레이어와 동일한 오프셋 거리를 유지하며 따라다님
/// - 회전은 고정 (항상 같은 방향을 바라봄)
/// - 오프셋은 인스펙터에서 자유롭게 조정 가능
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;          // 플레이어 Transform

    [Header("카메라 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f); // 플레이어 기준 상대 위치

    // ──────────────────────────────────────────────
    // 내부 상태
    // ──────────────────────────────────────────────
    private Vector3 _fixedRotation;  // 시작 시 카메라 회전을 그대로 고정

    private void Awake()
    {
        // 씬 배치 시점의 회전을 영구 고정
        _fixedRotation = transform.eulerAngles;
    }

    private void Start()
    {
        // 타깃이 없으면 'Player' 태그로 자동 검색
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("[CameraFollow] target이 없습니다. 'Player' 태그를 확인하세요.");
        }
    }

    // 물리 이후 카메라 이동 (떨림 방지)
    private void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 = 플레이어 월드 위치 + 고정 오프셋
        Vector3 desiredPos = target.position + offset;

        // 이동
        Vector3 newPos = desiredPos;

        transform.position = newPos;

        // 회전은 절대 변경하지 않음 (항상 같은 방향 고정)
        transform.eulerAngles = _fixedRotation;
    }

#if UNITY_EDITOR
    // 에디터에서 오프셋 위치 미리보기
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(target.position, target.position + offset);
        Gizmos.DrawWireSphere(target.position + offset, 0.2f);
    }
#endif
}
