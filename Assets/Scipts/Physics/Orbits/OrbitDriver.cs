using System.Threading;
using EditorAttributes;
using Orbits;
using Unity.VisualScripting;
using UnityEngine;

namespace Orbits
{

/*
    #==============================================================#
	
	
	
	
*/
[RequireComponent(typeof(TransformDouble))]
public class OrbitDriver : MonoBehaviour
{
//--#
    #region Variables


    [field: Title("Initialisation")]
    [HorizontalGroup(true, nameof(cartesianInitialise), nameof(keplerInitialise))]
    [SerializeField] private Void initButtonsHolder;

    public void InitCartesian() { keplerInitialise = !cartesianInitialise; }
    [SerializeField, HideInInspector, OnValueChanged(nameof(InitCartesian))] private bool cartesianInitialise;

    public void InitKepler() { cartesianInitialise = !keplerInitialise; }
    [SerializeField, HideInInspector, OnValueChanged(nameof(InitKepler))] private bool keplerInitialise;

    [field: Title("")]
    [field: SerializeField] public CelestialObject parent {get; private set;}
    [field: Space(5)]
    [field: SerializeField, ShowField(nameof(cartesianInitialise))] public Vector3d initPos {get; private set;}
    [field: SerializeField, ShowField(nameof(cartesianInitialise))] public Vector3d initVel {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initPeriapsis {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initEccentricity {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initInclination {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initRightAscensionOfAscendingNode {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initArgumentOfPeriapsis {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initTrueAnomaly {get; private set;}

    [Space(10)]
    [Title("Orbit")]
    [SerializeField, ReadOnly] private Orbit orbit = new();
    [Button] public void LogOrbit() => Debug.Log(orbit);

    public TransformDouble transformD {get; private set;}
    
    public OrbitConic orbitConic;


    #endregion
//--#



//--#
    #region Unity events


#if UNITY_EDITOR
    private void OnValidate() {
        Start();
        if (ScaleSpaceSingleton.Get.scaledSpaceTransform.Find(gameObject.name).TryGetComponent<ScaleSpaceMovement>(out ScaleSpaceMovement scaleSpaceMovement)){
            scaleSpaceMovement.OnValidate();
        };
    }

    private void OnDrawGizmos() { // TODO: Get shot for an actual orbit drawer
        if (orbitConic.points != null) {
            Gizmos.color = Color.red;
            for (int i = 0; i < orbitConic.points.Length; i++) {
                if (i != orbitConic.points.Length - 1)
                    Gizmos.DrawLine((Vector3)((parent.GetComponent<TransformDouble>().position + orbitConic.points[i].position) / ScaleSpaceSingleton.Get.scaleDownFactor), 
                                    (Vector3)((parent.GetComponent<TransformDouble>().position + orbitConic.points[i + 1].position) / ScaleSpaceSingleton.Get.scaleDownFactor));
            }
        }
    }
#endif

    private void Start() {

        transformD = GetComponent<TransformDouble>();
        transformD.AddComponentD(this);

        if (keplerInitialise) {
            orbit = new Orbit(initPeriapsis, initEccentricity, initInclination, initRightAscensionOfAscendingNode, initArgumentOfPeriapsis, initTrueAnomaly, parent, UniversalTimeSingleton.Get.time);
        }
        else if (cartesianInitialise) {
            orbit = new Orbit(initPos, initVel, parent, UniversalTimeSingleton.Get.time);
        }

        transformD.SetLocalPosition(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos);
        transformD.ToTransform();

        orbitConic = new(orbit, 64); // Patch

    }

    private void OnDestroy() {
        transformD.RemoveComponentD(this);
    }

    private void FixedUpdate() {
        transformD.SetLocalPosition(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos);
        transformD.ToTransform();
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}

}