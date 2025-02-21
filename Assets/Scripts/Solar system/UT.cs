using UnityEngine;

public class UT : MonoBehaviour
{
    //
    public static double UniverseTime { get; private set;} 
    //

    private void Start() {
        UniverseTime = 0.0;
    }

    private void FixedUpdate() {
        UniverseTime += Time.fixedDeltaTime;
    }
}
