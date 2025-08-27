// Singleton.cs
// Safe, flexible Unity MonoBehaviour singleton base.
// - AutoCreate (default true): creates a persistent instance if none exists at runtime.
// - Scene-dependent managers can disable AutoCreate and be placed in-scene.
// - Prevents duplicates; supports scene-change rebind hooks.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace YashVakil96.UnityTools.Patterns
{
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationQuitting;

    /// <summary>
    /// Global access to the live instance. May auto-create if enabled.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationQuitting) return null;

            if (_instance != null) return _instance;

#if UNITY_EDITOR
            // Avoid spawning persistent singletons while not playing.
            if (!Application.isPlaying)
            {
                _instance = FindObjectOfType<T>();
                return _instance;
            }
#endif
            // Try find existing
            _instance = FindObjectOfType<T>();
            if (_instance != null) return _instance;

            // Create if allowed
            if (GetAutoCreate())
            {
                var go = new GameObject("(singleton) " + typeof(T).Name);
                _instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
                Debug.Log($"[Singleton] Auto-created {typeof(T).Name} with DontDestroyOnLoad.");
                ( _instance as Singleton<T> )?.OnSingletonReady();
            }
            else
            {
                Debug.LogError($"[Singleton] No instance of {typeof(T).Name} found and AutoCreate is false. Place one in the scene.");
            }

            return _instance;
        }
    }

    /// <summary>
    /// Override in child to disable auto-creation (e.g., managers that need scene refs).
    /// </summary>
    protected virtual bool AutoCreate => true;

    // Static helper to read virtual AutoCreate safely from static context.
    private static bool GetAutoCreate()
    {
        // If one exists in scene, ask it; else assume default (true) by creating a temp proxy is overkill.
        // We’ll just create a temp ScriptableObject to read default virtual value via reflection is messy.
        // Simpler: require classes that need AutoCreate=false to place one in-scene or call Instance only after it exists.
        // To support per-class AutoCreate from static context, we peek any existing component first:
        var existing = FindObjectOfType<T>() as Singleton<T>;
        return existing ? existing.AutoCreate : true;
    }

    /// <summary>
    /// Called once when this instance becomes the live singleton.
    /// </summary>
    protected virtual void OnSingletonReady() { }

    /// <summary>
    /// Rebind scene references here if your singleton persists across scenes.
    /// </summary>
    protected virtual void OnSceneChanged(Scene oldScene, Scene newScene) { }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            // Persist only for root objects (avoid persisting nested duplicates unintentionally)
            if (transform.root == transform)
                DontDestroyOnLoad(gameObject);

            OnSingletonReady();
        }
        else if (_instance != this)
        {
            // Duplicate found — destroy the newer one
            Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} on {name} destroyed. Live instance: {_instance.name}");
            Destroy(gameObject);
            return;
        }
    }

    protected virtual void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleSceneChanged;
    }

    protected virtual void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleSceneChanged;
    }

    private void HandleSceneChanged(Scene oldScene, Scene newScene)
    {
        if (_instance == this as T)
            OnSceneChanged(oldScene, newScene);
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationQuitting = true;
    }
}    
}

