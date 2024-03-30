using UnityEngine;

public abstract class Singleton<T> : EventListener where T : MonoBehaviour
{
    private static T _Instance;

    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                return null;
            }
            return _Instance;

        }
        protected set
        {
            _Instance = value;
        }
    }

    protected virtual void Awake()
    {
        if (Instance == null)
            Instance = this as T;
        else
        {
            Destroy(this.gameObject);
        }
    }


    protected override void AddListeners() { }
    protected override void RemoveListeners() { }

    protected virtual void OnDestroy()
    {

    }
}