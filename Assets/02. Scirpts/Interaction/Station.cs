using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    /* 스크립트 역할 설명
     *
     * Station과 관련된 모든 상호작용을 담당
     *
     * 플레이어가 Station 트리거에 진입하면 TryInteract()로 상호작용 시작
     * 플레이어가 이탈하면 StopInteract()로 코루틴 중단
     *
     * 아이템 배치 흐름:
     *  1. 플레이어 → Station InputPos : PlaceInputItem()
     *  2. InputPos → 가공 : StartProcess() / Processing()
     *  3. 가공 완료 → OutputPos : PlaceOutputItem()
     */

    [SerializeField] private GameObject _Player;

    public GameObject InputItem;
    public GameObject OutputItem;
    public GameObject StationMaxUI, PlayerMaxUI, ArrowUI;
    public Transform[] ItemPos;

    public float processTime;
    public float processInterval;           // 아이템 간 가공 시작 간격 (Inspector에서 설정)
    public float currOutputLimitCount;
    public float maxOutputLimitCount;

    private Queue<GameObject> _stationQueue = new Queue<GameObject>();
    private Coroutine _interactCoroutine;
    private Coroutine _processCoroutine;

    // ───────────────────────────────────────────
    // Unity 생명주기
    // ───────────────────────────────────────────

    private void Update()
    {
        // 가공 루프가 꺼져 있고, 조건이 충족되면 루프 시작
        if (_processCoroutine == null && currOutputLimitCount < maxOutputLimitCount && _stationQueue.Count > 0)
        {
            _processCoroutine = StartCoroutine(ProcessLoop());
        }
    }

    // ───────────────────────────────────────────
    // Player ↔ Station 인터페이스
    // ───────────────────────────────────────────

    /// <summary>
    /// Player가 Station 트리거에 진입했을 때 호출됩니다.
    /// 플레이어가 요구 아이템을 보유 중이면 주기적으로 아이템을 받아옵니다.
    /// </summary>
    public void TryInteract(Player player)
    {
        ItemData requiredItem = InputItem.GetComponent<InteractableOBJs>().itemInfo;

        if (requiredItem.currItemCount != 0)
        {
            _interactCoroutine = StartCoroutine(InteractLoop(player, requiredItem));
        }
    }

    /// <summary>
    /// Player가 Station 트리거를 벗어났을 때 호출됩니다.
    /// </summary>
    public void StopInteract()
    {
        if (_interactCoroutine != null)
        {
            StopCoroutine(_interactCoroutine);
            _interactCoroutine = null;
        }
    }

    // ───────────────────────────────────────────
    // 상호작용 루프 (구 Player.InteractStation)
    // ───────────────────────────────────────────

    /// <summary>
    /// 일정 간격으로 플레이어 인벤토리에서 아이템을 꺼내 Station에 쌓습니다.
    /// </summary>
    private IEnumerator InteractLoop(Player player, ItemData itemInfo)
    {
        while (true)
        {
            GameObject inputItem = player.PopItem(itemInfo.itemID);

            if (inputItem != null)
            {
                ItemData inputData = inputItem.GetComponent<InteractableOBJs>().itemInfo;

                inputData.currItemCount--;

                if (inputData.currItemCount != inputData.maxItemCount)
                    PlayerMaxUI.SetActive(false);
                if (inputData.currItemCount < inputData.maxItemCount / 2)
                    ArrowUI.SetActive(false);                

                PlaceInputItem(inputItem);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    // ───────────────────────────────────────────
    // 아이템 배치 (구 Player.PlaceStation)
    // ───────────────────────────────────────────

    /// <summary>
    /// 플레이어에게서 받은 아이템을 InputPos에 배치하고 Queue에 등록합니다.
    /// </summary>
    private void PlaceInputItem(GameObject item)
    {
        Vector3 stackGap = _Player.GetComponent<Player>().StackGap;

        item.transform.parent = ItemPos[0];
        item.transform.localPosition = Vector3.zero + ItemPos[0].childCount * stackGap;
        item.transform.DOScale(1f, 0.4f).From(0f).SetEase(Ease.OutBack);

        _stationQueue.Enqueue(item);
    }

    /// <summary>
    /// 가공이 완료된 아이템을 OutputPos에 생성하고 배치를 갱신합니다.
    /// </summary>
    private void PlaceOutputItem()
    {
        Vector3 stackGap = _Player.GetComponent<Player>().StackGap;

        GameObject outputOBJ = Instantiate(OutputItem);
        outputOBJ.transform.parent = ItemPos[1];
        outputOBJ.transform.localPosition = Vector3.zero + stackGap * ItemPos[1].childCount;
        outputOBJ.transform.DOScale(1f, 0.4f).From(0f).SetEase(Ease.OutBack);

        RefreshStackPositions(ItemPos[1], stackGap);

        currOutputLimitCount++;

        if (currOutputLimitCount >= maxOutputLimitCount)
        {
            StationMaxUI.SetActive(true);
        }
    }

    /// <summary>
    /// 지정된 부모 Transform 아래의 자식들을 StackGap 간격으로 재정렬합니다.
    /// </summary>
    private void RefreshStackPositions(Transform parent, Vector3 stackGap)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).localPosition = Vector3.zero + stackGap * i;
        }
    }

    // ───────────────────────────────────────────
    // 가공 루프
    // ───────────────────────────────────────────

    /// <summary>
    /// Queue에 아이템이 있는 동안 processInterval 간격으로 하나씩 가공합니다.
    /// Queue가 비거나 출력 한도에 도달하면 루프를 종료합니다.
    /// </summary>
    private IEnumerator ProcessLoop()
    {
        while (_stationQueue.Count > 0 && currOutputLimitCount < maxOutputLimitCount)
        {
            GameObject ingredient = _stationQueue.Dequeue();
            Destroy(ingredient);

            Vector3 stackGap = _Player.GetComponent<Player>().StackGap;
            RefreshStackPositions(ItemPos[0], stackGap);

            yield return new WaitForSeconds(processTime);

            PlaceOutputItem();  // ← 여기만 남기고

            // ❌ 아래 두 줄 제거 (PlaceOutputItem 중복 호출, 간격도 잘못된 위치)
            // if (_stationQueue.Count > 0)
            //     yield return new WaitForSeconds(processInterval);
            // PlaceOutputItem();  ← 이게 버그

            if (_stationQueue.Count > 0)
                yield return new WaitForSeconds(processInterval);
        }

        _processCoroutine = null;
    }
}
