using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class UniversalTimeSingleton : MonoBehaviour
{
    
    public static UniversalTimeSingleton Get { get; private set; } = null;
    
    [SerializeField] private UIDocument ui;

    public int timeScale = 1;
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
        // Get a nice scale while also letting you pause
        int poweredTimeScale = (int)Mathd.Pow(2, timeScale-1);
        if (timeScale == 0) poweredTimeScale = 0;

        time += Time.fixedDeltaTime * poweredTimeScale;
        timeScaleString = "Time scale: " + poweredTimeScale;
    }

    
    public static void SetSingleton(UniversalTimeSingleton input) { Get = input; }

}
