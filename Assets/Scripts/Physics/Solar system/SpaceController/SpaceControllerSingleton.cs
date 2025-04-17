using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class SpaceControllerSingleton : MonoBehaviour
{
    public static SpaceControllerSingleton Get { get; private set; } = null;

    public CelestialObject[] objectList { get; private set; }

    [field: Space(7)]
    [field: Title("Refs")]
    [field: SerializeField] public CelestialObject focus { get; private set; }
    [field: SerializeField] public GameObject cameraObj { get; private set; }
    [field: SerializeField] public ScaleSpaceBodiesPositioner scaleSpaceBodiesPositioner { get; private set; }
    [field: SerializeField] public LocalSpacePositioner localSpacePositioner { get; private set; }
    
    [field: SerializeField] public Transform scaleSpaceContainer { get; private set; }
    [field: SerializeField] public Transform localSpaceContainer { get; private set; }
    
    [field: Space(7)]
    [field: Title("--")]
    [field: SerializeField] public float localRange { get; private set; }
    [field: SerializeField] public int patchDepthLimit { get; private set; } = 5;

    [field: SerializeField] public Material lineMat;

    [field: Space(7)]
    [field: Title("Editor Gizmos")]
    [field: SerializeField] public bool SOIGizmo { get; private set; } = true;
    [field: SerializeField] public bool VelGizmo { get; private set; } = true;

    [SerializeField, Button] private void Update_Spaces() {
        ClearSpaces();
        UpdateObjectList();
        UpdateCelestialObjects();
    }

    private void Awake() {
        if ( Get == null ) {
            Get = this;
        }
        else {
            Debug.Log("SINGLETON INSTANCE ALREADY SET!!!!   check for duplicates of: " + this); 
        }
        
        ClearSpaces();

        UpdateObjectList();
        UpdateCelestialObjects();

    }



    /*
        Celestial object setup
    */
    public void AddCelestialObject(CelestialObject celestialObject) {
        celestialObject.SetLocalSpaceObj(
            Instantiate(celestialObject.localSpacePrefab, localSpaceContainer)
        );
        celestialObject.SetScaleSpaceObj(
            Instantiate(celestialObject.scaleSpacePrefab, scaleSpaceContainer)
        );
    }
    public void RemoveCelestialObject(CelestialObject celestialObject) {
        if (celestialObject.localSpaceBody != null)
            DestroyImmediate(celestialObject.localSpaceBody);
        
        if (celestialObject.scaleSpaceBody != null)
            DestroyImmediate(celestialObject.scaleSpaceBody);
    }
    public void UpdateCelestialObjects() {
        foreach (CelestialObject celestialObject in objectList) {
            RemoveCelestialObject(celestialObject);
            AddCelestialObject(celestialObject);
        }
    }

    void ClearSpaces() {

        while (localSpaceContainer.childCount > 0) DestroyImmediate(localSpaceContainer.GetChild(0).gameObject);
        while (scaleSpaceContainer.childCount > 0) DestroyImmediate(scaleSpaceContainer.GetChild(0).gameObject);

    }


    /*
        Misc
    */

    public void SetCameraObj(GameObject cam) {
        cameraObj = cam;
    }

    public void UpdateObjectList() {
        List<CelestialObject> tempList = new();

        foreach (CelestialObject orbit in GetComponentsInChildren<CelestialObject>()) {
            tempList.Add(orbit);
        }

        objectList = tempList.ToArray();
    }
    
    public static void SetSingleton(SpaceControllerSingleton input) { Get = input; }
}
