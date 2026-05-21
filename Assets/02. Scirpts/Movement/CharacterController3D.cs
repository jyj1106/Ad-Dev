using UnityEngine;

/// <summary>
/// 3D 캐릭터 이동 컨트롤러
/// - 카메라는 독립 오브젝트 (CameraFollow.cs가 별도로 처리)
/// - PC: 마우스 드래그 방향으로 이동
/// - 모바일: 터치 위치에 조이스틱 생성 후 이동
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CharacterController3D : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed     = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("PC 드래그 설정")]
    [Tooltip("드래그 픽셀이 이 값 이상일 때만 이동 처리 (오차 방지)")]
    [SerializeField] private float dragDeadZone = 2f;

    [Header("카메라 (이동 기준축 계산용)")]
    [Tooltip("독립 카메라 오브젝트의 Transform. 비워두면 Camera.main 자동 사용.")]
    [SerializeField] private Transform cameraTransform;

    [Header("조이스틱 UI (모바일)")]
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float         joystickRadius = 75f;
    [SerializeField] private Canvas        uiCanvas;

    // ──────────────────────────────────────────────
    // 내부 상태
    // ──────────────────────────────────────────────
    private CharacterController _cc;

    // 카메라 고정 방향 → Awake에서 1회 캐시
    private Vector3 _camForward;
    private Vector3 _camRight;

    // PC 드래그
    private bool    _isDragging;
    private Vector2 _lastMousePos;

    // 모바일 조이스틱
    private bool    _joystickActive;
    private int     _joystickTouchId = -1;
    private Vector2 _joystickOrigin;
    private Vector2 _joystickInput;

    // ──────────────────────────────────────────────
    // Unity 생명주기
    // ──────────────────────────────────────────────
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        CacheCameraAxes();
        SetJoystickVisible(false);
    }

    private void Update()
    {
        bool isMobile = Application.isMobilePlatform || Input.touchCount > 0;

        if (isMobile)
            HandleMobileInput();
        else
            HandlePCInput();
    }

    // ──────────────────────────────────────────────
    // 카메라 축 캐시 (고정 카메라 → 1회만 계산)
    // ──────────────────────────────────────────────
    private void CacheCameraAxes()
    {
        if (cameraTransform == null)
        {
            _camForward = Vector3.forward;
            _camRight   = Vector3.right;
            return;
        }

        _camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        _camRight   = Vector3.ProjectOnPlane(cameraTransform.right,   Vector3.up).normalized;

        if (_camForward.sqrMagnitude < 0.001f)
        {
            _camForward = Vector3.forward;
            _camRight   = Vector3.right;
        }
    }

    // ──────────────────────────────────────────────
    // PC 마우스 드래그
    // ──────────────────────────────────────────────
    private void HandlePCInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging   = true;
            _lastMousePos = Input.mousePosition;

            PositionJoystick(_lastMousePos);
            SetJoystickVisible(true);
        }

        if (Input.GetMouseButton(0) && _isDragging)
        {
            Vector2 offset = Input.mousePosition - (Vector3)_lastMousePos;
            Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadius);
            Vector2 joystickInput = clamped / joystickRadius;

            joystickInput = clamped / joystickRadius;

            if (joystickHandle != null)
                joystickHandle.anchoredPosition = clamped;

            MoveCharacter(joystickInput);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            ResetJoystick();
        }
    }

    // ──────────────────────────────────────────────
    // 모바일 터치 / 조이스틱
    // ──────────────────────────────────────────────
    private void HandleMobileInput()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (_joystickActive) break;

                    _joystickActive  = true;
                    _joystickTouchId = touch.fingerId;
                    _joystickOrigin  = touch.position;
                    _joystickInput   = Vector2.zero;

                    PositionJoystick(_joystickOrigin);
                    SetJoystickVisible(true);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (touch.fingerId != _joystickTouchId) break;

                    Vector2 offset  = touch.position - _joystickOrigin;
                    Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadius);
                    _joystickInput  = clamped / joystickRadius;

                    if (joystickHandle != null)
                        joystickHandle.anchoredPosition = clamped;

                    MoveCharacter(_joystickInput);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId != _joystickTouchId) break;
                    ResetJoystick();
                    break;
            }
        }
    }

    // ──────────────────────────────────────────────
    // 이동 핵심 로직
    // ──────────────────────────────────────────────
    private void MoveCharacter(Vector2 input)
    {
        if (input.magnitude < 0.05f) return;

        Vector3 moveDir = (_camForward * input.y + _camRight * input.x).normalized;

        _cc.Move(moveDir * moveSpeed * Time.deltaTime);

        // 캐릭터 메시를 이동 방향으로 부드럽게 회전
        // 카메라는 독립 오브젝트이므로 이 회전에 전혀 영향받지 않음
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ──────────────────────────────────────────────
    // 조이스틱 UI 헬퍼
    // ──────────────────────────────────────────────
    private void SetJoystickVisible(bool visible)
    {
        if (joystickBackground != null)
            joystickBackground.gameObject.SetActive(visible);
    }

    private void PositionJoystick(Vector2 screenPos)
    {
        if (joystickBackground == null || uiCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.GetComponent<RectTransform>(),
            screenPos,
            uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera,
            out Vector2 localPos);

        joystickBackground.anchoredPosition = localPos;

        if (joystickHandle != null)
            joystickHandle.anchoredPosition = Vector2.zero;
    }

    private void ResetJoystick()
    {
        _joystickActive  = false;
        _joystickTouchId = -1;
        _joystickInput   = Vector2.zero;
        SetJoystickVisible(false);
    }
}
