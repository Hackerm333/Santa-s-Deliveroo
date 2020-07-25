using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IOutlineable
{
    [SerializeField] private List<Item> associatedItems = new List<Item>();
    [SerializeField] private GameObject selection;

    public void RemoveItem(Item item)
    {
        if (associatedItems.Contains(item))
            associatedItems.Remove(item);

        if (associatedItems.Count == 0)
        {
            associatedItems.Clear();
            gameObject.layer = 0;
            if (TryGetComponent(out BoxCollider boxCollider))
                Destroy(boxCollider);
            Destroy(this);
        }
    }

    public void OnMouseEnter() => GameManager.Instance.UpdateCursor(false);

    public void OnMouseExit() => GameManager.Instance.UpdateCursor(true);

    public GameObject Selection => selection;

    public void ManageOutlineEffect()
    {
        selection.SetActive(!selection.activeInHierarchy);
        foreach (var item in associatedItems)
            item.Selection.SetActive(selection.activeInHierarchy);
    }
}