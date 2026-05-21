using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 1000f, layerMask))
            {
                Debug.Log($"선택한 물체: {hitInfo.collider.name}");

                Debug.DrawLine(ray.origin, hitInfo.point, Color.green, 2.0f);
            }
        }
    }
}