using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Item : MonoBehaviour, IOutlineable
{
    [SerializeField] private House assignedHouse;
    [SerializeField] private GameObject selection;
    private bool _collected;
    public House AssignedHouse => assignedHouse;

    public void OnMouseEnter() => GameManager.Instance.UpdateCursor(false);

    public void OnMouseExit() => GameManager.Instance.UpdateCursor(true);

    public GameObject Selection => selection;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("UnitRTS"))
            _collected = true;
    }

    public void ManageOutlineEffect()
    {
        selection.SetActive(!selection.activeSelf);
        assignedHouse.Selection.SetActive(selection.activeSelf);

        if (_collected && !selection.activeInHierarchy)
            gameObject.SetActive(false);
    }
}