using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpaceControllerSingleton : MonoBehaviour
{
    public static SpaceControllerSingleton Get { get; private set; } = null;

    public CelestialObject[] objectList { get; private set; }
    [field: SerializeField] public CelestialObject focus { get; private set; }
    [field: SerializeField] public ScaleSpaceBodiesPositioner scaleSpaceBodiesPositioner { get; private set; }
    [field: SerializeField] public LocalSpacePositioner localSpacePositioner { get; private set; }
    
    [field: SerializeField] public Transform scaleSpaceContainer { get; private set; }
    [field: SerializeField] public Transform localSpaceContainer { get; private set; }
    
    [field: SerializeField] public float localRange { get; private set; }

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
    private void LateUpdate() {
        scaleSpaceBodiesPositioner.UpdatePositions();
        localSpacePositioner.UpdatePositions();
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
        if (celestialObject.localSpaceObj != null)
            DestroyImmediate(celestialObject.localSpaceObj);
        
        if (celestialObject.scaleSpaceObj != null)
            DestroyImmediate(celestialObject.scaleSpaceObj);
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

    public void UpdateObjectList() {
        List<CelestialObject> tempList = new();

        foreach (CelestialObject orbit in GetComponentsInChildren<CelestialObject>()) {
            tempList.Add(orbit);
        }

        objectList = tempList.ToArray();
    }
    
    public static void SetSingleton(SpaceControllerSingleton input) { Get = input; }
}
