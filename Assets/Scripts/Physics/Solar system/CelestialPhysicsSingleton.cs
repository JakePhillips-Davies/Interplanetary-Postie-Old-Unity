using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// 
/// 
/// Liberal help from this c++ Godot orbital mechanics addon: https://github.com/ivanoovii/GDCelestial/tree/master
/// Would have taken a loooooot longer without them.
///  
/// </summary>
public class CelestialPhysicsSingleton : MonoBehaviour
{
    //
    
    
    
    /// <summary>
    /// This is the object everything orbits. This object will not itself do any orbiting
    /// </summary>
    [SerializeField] private CelestialObject rootCelestialObj;
    
    public static CelestialPhysicsSingleton Get { get; private set; } = null;
    //

    private void Awake() {
        if ( Get == null ) {
            Get = this;
        }
        else {
            Debug.Log("SINGLETON INSTANCE ALREADY SET!!!!   check for duplicates of: " + this); 
        }
    }
    public static void SetSingleton(CelestialPhysicsSingleton input) { Get = input; }

    // private void Start() {
    //     UpdateCelestialTree();
    // }

    // private void FixedUpdate() {
    //     ProcessCelestialPhysics();
    // }
    
    

    




    /*
        Running the simulation
    */

    
    
    // public void UpdateCelestialTree() {
    //     rootCelestialObject = new() {
    //         orbitManager = rootCelestialBody,
    //         children = new()
    //     };
        
    //     UpdateCelestialChildren(rootCelestialObject);

    //     SetupCelestialChildren(rootCelestialObject);

    // }
    // private void UpdateCelestialChildren(CelestialObject parent) {
    //     Transform[] children = new Transform[parent.orbitManager.transform.childCount];
    //     for (int i = 0; i < children.Length; i++)
    //         children[i] = parent.orbitManager.transform.GetChild(i);

    //     foreach (Transform child in children) {
    //         if (child.TryGetComponent<OrbitManager>(out var childOrbit)) {
    //             CelestialObject childCelestialObject = new()
    //             {
    //                 orbitManager = childOrbit,
    //                 children = new()
    //             };

    //             UpdateCelestialChildren(childCelestialObject);

    //             parent.children.Add(childCelestialObject);
    //         }
    //     }
    // }
    // private void LogChildrenRecurs(CelestialObject obj) {
    //     Debug.Log(obj.orbitManager);

    //     foreach (var child in obj.children)
    //     {
    //         LogChildrenRecurs(child);
    //     }
    // }

    // private void SetupCelestialChildren(CelestialObject obj) {
    //     foreach (var child in obj.children) {
            
    //         // child.orbitManager.Setup();

    //         SetupCelestialChildren(child);

    //     }
    // }
    // public void ProcessCelestialPhysics() {
    //     ProcessCelestialChildren(rootCelestialObject, UniversalTimeSingleton.Get.time, true);
    // }
    // public void ProcessCelestialPhysics(double t) {
    //     ProcessCelestialChildren(rootCelestialObject, t, false);
    // }
    // private void ProcessCelestialChildren(CelestialObject obj, double t, bool drawing) {
    //     foreach (var child in obj.children) {
            
    //         if(drawing)child.orbitManager.ProcessOrbit(t);
    //         else child.orbitManager.ProcessOrbitGhost(t);

    //         ProcessCelestialChildren(child, t, drawing);
            
    //     }
    // }
    // public List<Orbit> GetBigCelestialBody() {
    //     List<Orbit> orbits = new();

    //     void CheckChildrenRecurs(CelestialObject obj){
    //         foreach (var child in obj.children)
    //         {
    //             CheckChildrenRecurs(child); 
    //             if (child.orbitManager.orbit.get_influence_radius() > 10) orbits.Add(child.orbitManager.orbit);
    //         }
    //     }

    //     CheckChildrenRecurs(rootCelestialObject);

    //     return orbits;
    // }





    /*
            Getters n shit
    */

    //public Material getLineMat() { return lineMat; }
    
    public static CelestialPhysicsSingleton get_singleton() { return Get; }
}
