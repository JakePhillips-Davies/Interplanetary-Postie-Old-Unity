using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class UniversalTimeSingleton : MonoBehaviour
{
    
    public static UniversalTimeSingleton Get { get; private set; } = null;
    
    [SerializeField] private UIDocument ui;

    public double timeScale = 1.0;
    public string timeScaleString;
    public double time { get; private set; } = 100;

    private void Awake() {
        if ( Get == null ) {
            Get = this;
        }
        else {
            Debug.Log("SINGLETON INSTANCE ALREADY SET!!!!   check for duplicates of: " + this); 
        }

        time = 100;
        ui.rootVisualElement.Q<Slider>().dataSource = this;
        
    }

    private void FixedUpdate() {
        time += Time.fixedDeltaTime * timeScale * timeScale;
        timeScaleString = "Time scale: " + timeScale * timeScale;
    }

    
    public static void SetSingleton(UniversalTimeSingleton input) { Get = input; }

}
