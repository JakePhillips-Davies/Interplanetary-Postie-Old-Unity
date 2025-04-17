using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SolarSystemEditorTool : MonoBehaviour 
{
    [SerializeField] private RootCelestialObject rootCelestialObject;
    [SerializeField] private bool isActive;

    private void Update() {
        
        if ( Application.isPlaying )
            return;

        // Check all the singletons because unity editor scripts and singletons hate eachother
        if( ScaleSpaceSingleton.Get == null ) ScaleSpaceSingleton.SetSingleton(Object.FindFirstObjectByType<ScaleSpaceSingleton>());
        if( UniversalTimeSingleton.Get == null ) UniversalTimeSingleton.SetSingleton(Object.FindFirstObjectByType<UniversalTimeSingleton>());
        if( SpaceControllerSingleton.Get == null ) SpaceControllerSingleton.SetSingleton(Object.FindFirstObjectByType<SpaceControllerSingleton>());

        if ( !isActive )
            return;

        rootCelestialObject.Validate();

    }

}