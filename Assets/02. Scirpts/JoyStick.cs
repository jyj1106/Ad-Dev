using UnityEngine;
using UnityEngine.UI;

public class JoyStick : MonoBehaviour
{
    [SerializeField] private Image Joystick_BG;
    [SerializeField] private Image Joystick_Handle;

    private void Update()
    {
        if(Input.touchCount > 0)                                // 터치가 발생해을 때
        {
            Touch finger = Input.GetTouch(0);
            Joystick_BG.gameObject.SetActive(true);             // 조이스틱 활성화

            if(finger.phase == TouchPhase.Moved)                // 드래그 중일 때
            {

            }
        }
    }

    private void OnDrag()
    {

    }
}
