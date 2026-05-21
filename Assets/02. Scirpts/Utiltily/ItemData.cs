using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("해당 아이템을 플레이어가 지닐 수 있는 최대 개수")]
    public int maxItemCount;

    [Header("현재 해당 아이템을 지닌 개수")]
    public int currItemCount;    
}