using UnityEngine;

[ExecuteInEditMode]
public class ScaleSpaceSingleton : MonoBehaviour
{
    
    public static ScaleSpaceSingleton Get { get; private set; } = null;
    
    [SerializeField] float spaceScaleDownFactor = 10000000;

    private void Awake() {
        if ( Get == null ) {
            Get = this;
        }
        else {
            Debug.Log("SINGLETON INSTANCE ALREADY SET!!!!   check for duplicates of: " + this); 
        }

    }

    public float GetSpaceDownScale() { return spaceScaleDownFactor; }
    public float GetSpaceScale() { return 1 / spaceScaleDownFactor; }
    public static void SetSingleton(ScaleSpaceSingleton input) { Get = input; }
    
}
