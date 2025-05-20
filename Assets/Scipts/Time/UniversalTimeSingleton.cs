using EditorAttributes;
using UnityEngine;

public class UniversalTimeSingleton : MonoBehaviour
{
    public static UniversalTimeSingleton Get { get; private set; } = null;

    [field: SerializeField] public int timeScale { get; private set; } = 1;
    [field: SerializeField] public bool exponentialTimeScale { get; private set; } = false;
    [field: SerializeField] public double time { get; private set; } = 0;

#if UNITY_EDITOR
    private void OnValidate() {
        Awake();
    }
#endif

    private void Awake() {
        if (Get != null)
            if (Get != this)
                Debug.LogWarning("ERROR: Multiple instances of " + this + " exists! Are you sure you should be doing this?");

        Get = this;
    }

    private void FixedUpdate() {
        if (exponentialTimeScale) {
            // Get a nice scale while also letting you pause
            int poweredTimeScale = (int)Mathd.Pow(2, timeScale-1);
            if (timeScale == 0) poweredTimeScale = 0;
            time += Time.fixedDeltaTime * poweredTimeScale;
        }
        else time += Time.fixedDeltaTime * timeScale;
    }

    
    public static void SetSingleton(UniversalTimeSingleton input) { Get = input; }

}
