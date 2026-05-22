using UnityEngine;

public class InteractableOBJs : MonoBehaviour
{
    public ItemData itemInfo;

    private void Awake()
    {
        itemInfo.currItemCount = 0;
    }

}
