using DG.Tweening;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] private GameObject _stone;
    [SerializeField] private GameObject _Player;
    public void OnBreak()
    {
        ItemData stoneData = _stone.GetComponent<InteractableOBJs>().itemInfo;

        if (stoneData.currItemCount < stoneData.maxItemCount)
        {
            GameObject stone = Instantiate(_stone);
            stone.transform.position = _Player.transform.position;
            stone.transform.DOScale(1f, 0.4f).From(0f).SetEase(Ease.OutBack);
        }        
        gameObject.SetActive(false);
        Invoke("Respawn", 3f);
    }

    public void Respawn()
    {      
        gameObject.SetActive(true);
    }
}
