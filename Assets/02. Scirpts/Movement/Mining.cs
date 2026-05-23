using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mining : MonoBehaviour
{
    [SerializeField] private Vector3[] _OverlapBox_Center;
    [SerializeField] private Vector3[] _OverlapBox_Size;
    [SerializeField] private LayerMask _layerRock;
    [SerializeField] private ItemData _stoneInfo;

    [SerializeField] private GameObject ArrowUI, MAXUI;

    public Collider[] overlapBox;
    public GameObject[] _Equipments;
    public int _upgradeLv;

    private void Start()
    {
        _upgradeLv = 0;
    }

    public void MakeOverlapBox()
    {
        overlapBox = Physics.OverlapBox(transform.position + _OverlapBox_Center[_upgradeLv], _OverlapBox_Size[_upgradeLv], transform.rotation, _layerRock);
    }

    public void Mine()
    {
        if(overlapBox.Length > 0)
        {
            float mindis = float.MaxValue;
            GameObject nearObj = null;

            foreach (Collider c in overlapBox)
            {
                if(mindis > Vector3.Distance(transform.position, c.gameObject.transform.position))
                {
                    mindis = Vector3.Distance(transform.position, c.gameObject.transform.position);
                    nearObj = c.gameObject;
                }
            }


            if(nearObj != null)
                nearObj.GetComponent<Rock>().OnBreak();
        }

        if (_stoneInfo.currItemCount > _stoneInfo.maxItemCount / 2)
            ArrowUI.SetActive(true);
        else
            ArrowUI.SetActive(false);

        if ((_stoneInfo.currItemCount == _stoneInfo.maxItemCount))
        {
            MAXUI.SetActive(true);
            MAXUI.transform.DOScale(0.5f, 1f).From(0f).SetEase(Ease.OutBack);
        }
        else
            MAXUI.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + _OverlapBox_Center[_upgradeLv], _OverlapBox_Size[_upgradeLv] * 2);
    }
}
