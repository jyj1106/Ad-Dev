using UnityEngine;

public class InteractableOBJs : MonoBehaviour, IIteractable
{
    [SerializeField] private ItemData[] itemInfo;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {

        }
    }
}
