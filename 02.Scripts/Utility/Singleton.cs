using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {

                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }

            return instance;
        }
        set { instance = value; }
    }

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }
}