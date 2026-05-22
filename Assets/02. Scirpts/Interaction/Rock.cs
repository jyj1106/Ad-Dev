using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] private GameObject _stone;

    public void OnBreak()
    {
        GameObject stone = Instantiate(_stone);
        stone.transform.position = transform.position;
        gameObject.SetActive(false);
        Respawn(3);
    }

    IEnumerator Respawn(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
