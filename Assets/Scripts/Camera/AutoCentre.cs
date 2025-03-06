using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class AutoCentre : MonoBehaviour
{
    [SerializeField] UIDocument ui;
    public Transform[] objectList { get; private set; }
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
    private void Update() {
        // if ( !Application.isPlaying ) transform.position -= objectList[focus].position;
        // else if ( objectList[focus].position.magnitude > 0.5 ) transform.position = Vector3.Lerp(transform.position, transform.position - objectList[focus].position, 0.3f );
        transform.position -= objectList[focus].position;

        focusName = objectList[focus].name;
    }


    private void OnValidate() {
        UpdateObjectList();
    }


    public void UpdateObjectList() {
        List<Transform> tempList = new();

        foreach (Orbit orbit in GetComponentsInChildren<Orbit>()) {
            tempList.Add(orbit.transform);
        }

        objectList = tempList.ToArray();
    }
}
