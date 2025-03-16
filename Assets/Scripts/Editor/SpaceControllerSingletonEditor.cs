using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpaceControllerSingleton))]
public class SpaceControllerSingletonEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
         SpaceControllerSingleton spaceControllerSingleton = (SpaceControllerSingleton)target;
         if (GUILayout.Button("Update bodies")) {
            spaceControllerSingleton.UpdateObjectList();
            spaceControllerSingleton.UpdateCelestialObjects();
         }
    }
}