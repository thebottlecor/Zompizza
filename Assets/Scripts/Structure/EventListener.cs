using UnityEngine;

public abstract class EventListener : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        AddListeners();
    }

    protected virtual void OnDisable()
    {
        RemoveListeners();
    }


    protected abstract void AddListeners();
    protected abstract void RemoveListeners();
}