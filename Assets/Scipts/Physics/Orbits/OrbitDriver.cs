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
[RequireComponent(typeof(SpaceSimTransform))]
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

    [field: Space(10)]
    [field: Title("Orbit")]
    [field: SerializeField, ReadOnly] public Orbit orbit {get; private set;} = new();
    [Button] public void LogOrbit() => Debug.Log(orbit);

    public SpaceSimTransform simTransform {get; private set;}
    
    public OrbitConic orbitConic;


    #endregion
//--#



//--#
    #region Unity events


#if UNITY_EDITOR
    private void OnValidate() {
        Awake();
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
                    Gizmos.DrawLine((Vector3)((parent.GetComponent<SpaceSimTransform>().position + orbitConic.points[i].position) / ScaleSpaceSingleton.Get.scaleDownFactor), 
                                    (Vector3)((parent.GetComponent<SpaceSimTransform>().position + orbitConic.points[i + 1].position) / ScaleSpaceSingleton.Get.scaleDownFactor));
            }
        }
    }
#endif

    private void Awake() {
        simTransform = GetComponent<SpaceSimTransform>();
        simTransform.AddSimComponent(this);
    }
    private void Start() {

        if (keplerInitialise) {
            orbit = new Orbit(initPeriapsis, initEccentricity, initInclination, initRightAscensionOfAscendingNode, initArgumentOfPeriapsis, initTrueAnomaly, parent, UniversalTimeSingleton.Get.time);
        }
        else if (cartesianInitialise) {
            orbit = new Orbit(initPos, initVel, parent, UniversalTimeSingleton.Get.time);
        }

        simTransform.SetLocalPosition(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos);
        simTransform.ToTransform();

        orbitConic = new(orbit, 64); // Patch

    }

    private void OnDestroy() {
        simTransform.RemoveSimComponent(this);
    }

    private void FixedUpdate() {
        simTransform.SetLocalPosition(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos);
        simTransform.ToTransform();
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}

}