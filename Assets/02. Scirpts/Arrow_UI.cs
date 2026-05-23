using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_UI : MonoBehaviour
{
    [SerializeField] private GameObject rockStation;

    void Update()
    {
        transform.LookAt(rockStation.transform.position);
    }
}
