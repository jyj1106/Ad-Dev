using UnityEngine;

public class Player : MonoBehaviour
{
    /* 스크립트 역할 설명
     * 
     * 플레이어의 충돌(Trigger)로 인한 상호작용 스크립트
     * 
     * 아이템은 등에 2종류의 아이템을 각 스택 수 만큼 들 수 있믐 (현재는 돌과 돈 2종류)
     * --예외로 수갑의 경우 스택류 아이템이지만 정면에서 직접 드는 작업을 진행하기에 아이템이 아니라 장비로 취급--
     * 
     * 채집 구역 진입 시 업그레이드 상태에 해당하는 아이템을 정면에 장착, 이탈 시 장착해제
     */

    private CharacterController3D _controller;

    [SerializeField] private GameObject[] _ItemPos;
    [SerializeField] private ItemData[] _ItemInfos;
    [SerializeField] private int _ItemCount;

    private void Awake()
    {
        _controller = GetComponent<CharacterController3D>();
    }

    private void Start()
    {
        
    }

    /* 접촉 시 아이템인지 시설물인지 확인
     * 
     * 아이템일 시: 
     * 1.
     * 
     */
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Item"))                                         // 부딪힌게 아이템일 때
        {
            ItemData itemInfo = other.GetComponent<InteractableOBJs>().itemInfo;

            if(_ItemCount != 2)
            {


                _ItemCount++;
            }
        }
    } 

    public void AddStack(GameObject other)
    {
        
    }
}