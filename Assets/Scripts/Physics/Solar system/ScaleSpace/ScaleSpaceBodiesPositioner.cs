using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class AutoCentre : MonoBehaviour
{
    [SerializeField] UIDocument ui;
    public ScaleSpaceBody[] objectList { get; private set; }
    public int focus = 3;
    public string focusName;
    
    private void Awake() {
        UpdateObjectList();

        ui.rootVisualElement.Q<TextField>("Focus").dataSource = this;
        ui.rootVisualElement.Q<Button>("Previous").clickable.clicked += () => {
            if(focus > 0) focus--; 
        };
        ui.rootVisualElement.Q<Button>("Next").clickable.clicked += () => {
            if(focus < objectList.Length-1) focus++;
        };
    }
    private void LateUpdate() {
        Orbit focusOrbit = objectList[focus].refOrbit;
        foreach (ScaleSpaceBody _object in objectList) {
            
            Orbit orbit = _object.refOrbit;

            Vector3d posFromFocus = orbit.getWorldPos() - focusOrbit.getWorldPos();
            posFromFocus = new(posFromFocus.x, posFromFocus.z, -posFromFocus.y);

            _object.transform.position = (Vector3)(posFromFocus * CelestialPhysics.get_singleton().get_spaceScale()); 

        }

        focusName = objectList[focus].name;
    }


    private void OnValidate() {
        UpdateObjectList();
    }


    public void UpdateObjectList() {
        List<ScaleSpaceBody> tempList = new();

        foreach (ScaleSpaceBody orbit in GetComponentsInChildren<ScaleSpaceBody>()) {
            tempList.Add(orbit);
        }

        objectList = tempList.ToArray();
    }
}
