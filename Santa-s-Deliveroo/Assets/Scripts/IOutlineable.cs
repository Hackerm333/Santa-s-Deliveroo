using UnityEngine;

public interface IOutlineable
{
    void OnMouseEnter();
    void OnMouseExit();
    GameObject Selection { get; }
    void ManageSelection();
}