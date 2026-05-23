using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /* 스크립트 역할 설명
     * 
     * 플레이어의 충돌(Trigger)로 인한 상호작용 스크립트
     * 
     * 아이템은 등에 2종류의 아이템을 각 스택 수 만큼 들 수 있음 (현재는 돌과 돈 2종류)
     * --예외로 수갑의 경우 스택류 아이템이지만 정면에서 직접 드는 작업을 진행하기에 아이템이 아니라 장비로 취급--
     * 
     * 채집 구역 진입 시 업그레이드 상태에 해당하는 아이템을 정면에 장착, 이탈 시 장착해제
     */

    [SerializeField] private CharacterController3D _controller;
    [SerializeField] private Mining _mining;
    [SerializeField] private Animator _animator;
    [SerializeField] private ItemData _stoneData;

    [SerializeField] private GameObject[] _ItemPos;
    [SerializeField] private GameObject _MaxText;
    [SerializeField] private GameObject _Arrow;
    [SerializeField] private Vector3 _StackGap;

    private Stack<GameObject> _CashStack = new Stack<GameObject>();
    private Stack<GameObject> _StoneStack = new Stack<GameObject>();
    private Coroutine _redeemCoroutine;

    private bool _isMining = false;

    private void Awake()
    {
        _controller = GetComponent<CharacterController3D>();
        _mining = transform.GetChild(0).GetComponent<Mining>();
        _animator = transform.GetChild(0).GetComponent<Animator>();

        _stoneData.currItemCount = 0;
    }

    private void FixedUpdate()
    {
        if (_isMining)                                      // 광산 안에 있을 때
        {
            _mining.MakeOverlapBox();

            if (_mining.overlapBox.Length > 0)              // 주변에 바위가 있다면
                _animator.SetBool("IsMining", true);
            else
                _animator.SetBool("IsMining", false);
        }

        if(_stoneData.currItemCount < _stoneData.maxItemCount)
        {
            _MaxText.SetActive(false);
        }
    }

    /* 접촉 시 태그 확인
     * 
     * 아이템일 시: 
     *  해당 아이템을 더 지닐 수 있다면 -> 아이템 쌓기
     *  아니라면 -> MAX 표시 
     */
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Item"))                                         // 부딪힌게 아이템일 때
        {
            ItemData itemInfo = other.GetComponent<InteractableOBJs>().itemInfo;

            if (itemInfo.currItemCount <= itemInfo.maxItemCount)                        // 아이템을 더 쌓을 수 있다면
                StackItem(other.gameObject, itemInfo);
        }

        if (other.transform.CompareTag("Mine"))
        {
            _isMining = true;
            _animator.SetLayerWeight(1, 1f);
            _mining._Equipments[_mining._upgradeLv].SetActive(true);
        }

        if (other.transform.CompareTag("Station"))
        {
            Station stationInfo = other.GetComponent<Station>();
            stationInfo.TryInteract(this);
        }

        if (other.transform.CompareTag("Redeem"))
        {
            StartCoroutine(RedeemLoop(other.transform.GetChild(0)));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Mine"))
        {
            _isMining = false;
            _animator.SetLayerWeight(1, 0f);
            _mining._Equipments[_mining._upgradeLv].SetActive(false);
        }

        if (other.transform.CompareTag("Station"))
        {
            Station stationInfo = other.GetComponent<Station>();
            stationInfo.StopInteract();
        }

        if (other.transform.CompareTag("Redeem"))
        {
            StopCoroutine(_redeemCoroutine);
            _redeemCoroutine = null;
        }
    }

    // 01_Stone, 02_Cash     
    private void StackItem(GameObject other, ItemData itemInfo)
    {
        itemInfo.currItemCount++;

        switch (itemInfo.itemID)
        {
            case null:
            case "Item_00":
                return;

            case "Item_01":
                _StoneStack.Push(other);
                PlaceItem(other, itemInfo);
                break;

            case "Item_02":
                _CashStack.Push(other);
               PlaceItem(other, itemInfo);
                break;
        }
    }

    public (int index, bool isBlank) CheckBelongings(ItemData itemInfo)     // 소지품을 확인하고 그에 따라 들어갈 수 있는 공간을 반환
    {
        if (_ItemPos[0].transform.childCount == 0)
        {
            if (_ItemPos[1].transform.childCount == 0)                          // 소지품이 하나도 없을 때
                return (0, true);
            else                                                                // 첫 번째가 비어있다면 두 번째 칸을 첫번째로 이동
            {
                _ItemPos[1].transform.GetChild(0).parent = _ItemPos[0].transform;
                _ItemPos[0].transform.localPosition = Vector3.zero;
                CheckBelongings(itemInfo);                                      // 재귀
            }
        }
        GameObject pos1Item = _ItemPos[0].transform.GetChild(0).gameObject;
        string pos1ID = pos1Item.GetComponent<InteractableOBJs>().itemInfo.itemID;

        if (pos1ID == itemInfo.itemID) return (0, false);                       // 1번 칸이 동일 아이템일 때
        else if (pos1ID == "Item_01")                                           // 1번 칸이 동일하지 않고, 바위일 땐 2번에 배치
        {
            if (_ItemPos[1].transform.childCount == 0) return (1, true);        // 2번 칸이 공란일 때
            else return (1, false);                                             // 2번 칸의 돈에 배치
        }
        else                                                                    // 1번 칸이 돈이라면, 2번으로 이동시킴
        {
            pos1Item.transform.parent = _ItemPos[1].transform;
            pos1Item.transform.localPosition = Vector3.zero;
            return (0, true);
        }
    }

    private IEnumerator RedeemLoop(Transform endPos)
    {
        while (true)
        {
            if (endPos.childCount > 0)
            {
                // 리스트로 미리 복사 (childCount 변화 방지)
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < endPos.childCount; i++)
                    children.Add(endPos.GetChild(i).gameObject);

                foreach (GameObject child in children)
                {
                    ItemData itemInfo = child.GetComponent<InteractableOBJs>().itemInfo;

                    if (itemInfo.currItemCount >= itemInfo.maxItemCount)
                    {
                        _MaxText.SetActive(true);
                        continue;
                    }

                    // 스택에 Push
                    switch (itemInfo.itemID)
                    {
                        case "Item_01": _StoneStack.Push(child); break;
                        case "Item_02": _CashStack.Push(child); break;
                        default: continue;
                    }

                    itemInfo.currItemCount++;
                    PlaceItem(child, itemInfo);

                    yield return new WaitForSeconds(0.15f);  // 아이템 간 간격
                }
            }

            yield return new WaitForSeconds(0.2f);  // 다음 루프 간격
        }
    }

    private void PlaceItem(GameObject other, ItemData itemInfo)             // 등에 아이템을 배치
    {
        var (index, isFirst) = CheckBelongings(itemInfo);
        if (isFirst)
        {
            other.transform.parent = _ItemPos[index].transform;
            other.transform.localPosition = Vector3.zero;
            other.transform.DOScale(1f, 0.4f).From(0f).SetEase(Ease.OutBack);
            other.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        else
        {
            other.transform.parent = _ItemPos[index].transform.GetChild(0);
            other.transform.localRotation = Quaternion.Euler(Vector3.zero);
            if (itemInfo.itemID == "Item_01")
                other.transform.localPosition = Vector3.zero + ((_StoneStack.Count - 1) * _StackGap);

            if (itemInfo.itemID == "Item_02")
                other.transform.localPosition = Vector3.zero + ((_CashStack.Count - 1) * _StackGap);

            other.transform.DOScale(1f, 0.4f).From(0f).SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// Station이 요구하는 아이템을 플레이어 인벤토리에서 꺼내 반환합니다.
    /// 해당 아이템이 없으면 null을 반환합니다.
    /// </summary>
    public GameObject PopItem(string itemID)
    {
        switch (itemID)
        {
            case "Item_01":
                return _StoneStack.Count > 0 ? _StoneStack.Pop() : null;
            case "Item_02":
                return _CashStack.Count > 0 ? _CashStack.Pop() : null;
            default:
                return null;
        }
    }

    public Vector3 StackGap => _StackGap;
}
