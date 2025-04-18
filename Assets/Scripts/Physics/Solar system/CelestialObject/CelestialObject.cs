using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class CelestialObject : MonoBehaviour
{
    public enum CelestialObjectState {
        KEPLERIAN,
        NEWTONIAN,
        NON_ORBITTING,
        TRANSITIONING
    }
    
//--#
    #region Variables

    
    [field: Space(7)]
    [field: Title("Refs")]
    [field: SerializeField] public Orbit refOrbit { get; private set; }
    [field: SerializeField, ReadOnly]public OrbitLineRenderer orbitLineRenderer { get; private set; }
    [field: SerializeField, ReadOnly]public ScaleSpaceBody scaleSpaceBody { get; private set; }
    [field: SerializeField, ReadOnly]public LocalSpaceBody localSpaceBody { get; private set; }
    [field: SerializeField, ReadOnly]public CelestialObject parent { get; private set; }
    [field: SerializeField, ReadOnly]public List<CelestialObject> siblings { get; private set; }
    [field: SerializeField, ReadOnly]public List<CelestialObject> children { get; private set; }

    [field: Space(7)]
    [field: Title("Settings")]
    [FoldoutGroup("", nameof(patchable))]
    [SerializeField] private Void settingsHolder;
    [field: SerializeField, HideProperty] public bool patchable { get; private set; }


    [field: Space(7)]
    [field: Title("State")]
    [field: SerializeField, ReadOnly] public CelestialObjectState state { get; private set; } = CelestialObjectState.KEPLERIAN;

    [field: Space(7)]
    [field: Title("Prefabs")]
    [FoldoutGroup("", nameof(scaleSpacePrefab), nameof(localSpacePrefab))]
    [SerializeField] private Void prefabsHolder;
    [field: SerializeField, AssetPreview, HideProperty] public GameObject scaleSpacePrefab { get; private set; }
    [field: SerializeField, AssetPreview, HideProperty] public GameObject localSpacePrefab { get; private set; }

    [field: Space(7)]
    [field: Title("Debugging")]
    [FoldoutGroup("", nameof(debugging))]
    [SerializeField] private Void debugHolder;
    [field: SerializeField, HideProperty] public bool debugging { get; private set; } = false;


    private bool postStartFrame = false;

    public bool frameDelay = false;

    private Vector3 lastPos = new Vector3(0, 0, 0);
    public double lastTime { get; private set; }= 0;
    private Vector3d lastOrbitPos = new Vector3d(0, 0, 0);
    private Vector3d orbitLocalPos = new Vector3d(0, 0, 0);
    private Vector3d orbitLocalVel = new Vector3d(0, 0, 0);

    #endregion
//--#

//--#
    #region unity events

    
    private void Start() {
    }

    private void FixedUpdate() {
    }


    #endregion
//--#

//--#
    #region chain events

    /// <summary>
    /// Simulate self, then tell it's children to do the same
    /// </summary>
    public void ChainSimulate() {
        if ((UniversalTimeSingleton.Get.timeScale != 1) && (state != CelestialObjectState.NON_ORBITTING)) state = CelestialObjectState.KEPLERIAN;

        switch (state) {
            case CelestialObjectState.KEPLERIAN:
                HandleKeplerian();
                break;

            case CelestialObjectState.TRANSITIONING:
                HandleNewtonian();
                state = CelestialObjectState.NEWTONIAN;
                break;

            case CelestialObjectState.NEWTONIAN:
                HandleNewtonian();
                break;

            case CelestialObjectState.NON_ORBITTING:
                break;

            default:
                Debug.LogError("When tf did you put in a third state  || " + this);
                break;
        }

        foreach (CelestialObject child in children) {
            child.ChainSimulate();
        }

        frameDelay = false;
    }

    /// <summary>
    /// If newtonian, it needs to do some magic updating orbits BEFORE everything else
    /// Do self, then tell it's children to do the same
    /// </summary>
    public void ChainUpdateNewtonians() {

        if (state == CelestialObjectState.NEWTONIAN) {
            NewtonianOrbitUpdater();
        }

        foreach (CelestialObject child in children) {
            child.ChainUpdateNewtonians();
        }
    }

    /// <summary>
    /// Position self, then tell it's children to do the same
    /// </summary>
    public void ChainPosition() {

        if (state == CelestialObjectState.NON_ORBITTING) {
            // Do nothing
            return;
        }

        UpdateLocalSpace();
        
        if (orbitLineRenderer != null)
            orbitLineRenderer.DrawOrbit();

        foreach (CelestialObject child in children) {
            child.ChainPosition();
        }
    }

    /// <summary>
    /// Startup self, then tell it's children to do the same
    /// </summary>
    public void ChainStartup(){
        // Don't remove this setter, it acts weird if you do
        state = CelestialObjectState.KEPLERIAN;

        RecalculateFamily();
        refOrbit.Startup();

        postStartFrame = true;

        foreach (CelestialObject child in children) {
            child.ChainStartup();
        }
    }


    #endregion
//--##

//--#
    #region setters

    
    public void SetState(CelestialObjectState _state) {
        state = _state;
    }
    public void SetScaleSpaceObj(GameObject _scaleSpaceObj) { 
        scaleSpaceBody = _scaleSpaceObj.GetComponent<ScaleSpaceBody>();
        scaleSpaceBody.SetOrbit(this.refOrbit);
        scaleSpaceBody.SetCelestialObject(this);
    }
    public void SetLocalSpaceObj(GameObject _localSpaceObj) { 
        localSpaceBody = _localSpaceObj.GetComponent<LocalSpaceBody>(); 
        localSpaceBody.SetOrbit(this.refOrbit);
        localSpaceBody.SetCelestialObject(this);
    }
    public void SetOrbitLineRenderer(OrbitLineRenderer _orbitLineRenderer) {
        orbitLineRenderer = _orbitLineRenderer;
        orbitLineRenderer.SetCelestialObject(this);
    }


    #endregion
//--#

//--#
    #region state handlers


    private void HandleKeplerian() {
        refOrbit._physics_process(UniversalTimeSingleton.Get.time);
        if (patchable) refOrbit.patch_conics();
    }

    private void HandleNewtonian() {
        Vector3 gravityAcceleration = (Vector3)refOrbit.GetNewtonianAcceleration();
        gravityAcceleration = new(gravityAcceleration.x, gravityAcceleration.z, -gravityAcceleration.y);

        localSpaceBody.rb.AddForce(gravityAcceleration, ForceMode.Acceleration);
        
        // This is useful to do for another bit
        refOrbit._physics_process(UniversalTimeSingleton.Get.time);
        if (patchable) refOrbit.patch_conics();
        
    }

    private void NewtonianOrbitUpdater() {
        Vector3 posFromLast = localSpaceBody.GetPosition() - lastPos;
        posFromLast = new(posFromLast.x, -posFromLast.z, posFromLast.y);
        orbitLocalPos = lastOrbitPos + new Vector3d(posFromLast);

        Vector3 vel = localSpaceBody.rb.linearVelocity;
        vel = new(vel.x, -vel.z, vel.y);
        orbitLocalVel = new Vector3d(vel);

        // Logging for debugging
        if (debugging) {
            Debug.Log($"{this.name} + NewtonianOrbitUpdater: \n" +
                     $"pos local: {orbitLocalPos} \n"+
                     $"pos orbit: {refOrbit.GetLocalPos()} \n"+
                     $"pos diff vec: {orbitLocalPos - refOrbit.GetLocalPos()} \n"+
                     $"pos sq diff: {(orbitLocalPos - refOrbit.GetLocalPos()).sqrMagnitude} \n"+
                     $"vel sq diff: {(orbitLocalVel - refOrbit.GetLocalVel()).sqrMagnitude} \n"
            );
        }
        
        if ((orbitLocalPos - refOrbit.GetLocalPos()).sqrMagnitude > 2.5e-7d
            || (orbitLocalVel - refOrbit.GetLocalVel()).sqrMagnitude > 2.5e-7d
        ) {
            if (debugging) Debug.Break();

            refOrbit.SetCartesianElements(orbitLocalVel, orbitLocalPos);
            refOrbit.SetOrbitStartTime(lastTime);
        }
        else {
            Vector3 kVel = (Vector3)refOrbit.GetLocalVel();
            kVel = new(kVel.x, kVel.z, -kVel.y);
            localSpaceBody.rb.linearVelocity = kVel;
        }
    }


    #endregion
//--#

//--#
    #region local space
    
    Vector3d posFromFocus;
    
    private void UpdateLocalSpace() {
        posFromFocus = refOrbit.GetWorldPos() - SpaceControllerSingleton.Get.GetFocusPosition();
        posFromFocus = new(posFromFocus.x, posFromFocus.z, -posFromFocus.y);

        // Logging for debugging
        if (debugging) {
            Debug.Log($"{this.name} + UpdateLocalSpace: \n" +
                     $"pos from focus: {posFromFocus} \n"
            );
        }
        
        localSpaceBody.SetPosition((Vector3)posFromFocus);
        scaleSpaceBody.transform.position = (Vector3)(posFromFocus * ScaleSpaceSingleton.Get.GetSpaceScale());

        lastOrbitPos = refOrbit.GetLocalPos();
        lastPos = localSpaceBody.GetPosition();
        lastTime = UniversalTimeSingleton.Get.time;

        CheckDistance();
    }


    #endregion
//--#

//--#
    #region scaled space


    


    #endregion
//--#

//--#
    #region setup


    public void RecalculateFamily() {
        // Parent
        if (transform.parent.TryGetComponent<CelestialObject>(out CelestialObject _parent)) parent = _parent;
        else parent = null;

        // Siblings
        Transform[] siblingTransforms = new Transform[transform.parent.childCount];
        for (int i = 0; i < siblingTransforms.Length; i++)
            siblingTransforms[i] = transform.parent.GetChild(i);

        siblings = new List<CelestialObject>();

        foreach (Transform sibling in siblingTransforms){
            if (sibling.TryGetComponent<CelestialObject>(out CelestialObject _sibling))
                if (_sibling != this)
                    siblings.Add(_sibling);
        }

        // Children
        Transform[] childrenTransforms = new Transform[transform.childCount];
        for (int i = 0; i < childrenTransforms.Length; i++)
            childrenTransforms[i] = transform.GetChild(i);

        children = new List<CelestialObject>();

        foreach (Transform child in childrenTransforms){
            if (child.TryGetComponent<CelestialObject>(out CelestialObject _child))
                children.Add(_child);
        }
    }


    #endregion
//--##

//--#
    #region misc


    private void CheckDistance() {
        if (frameDelay) return;

        if (localSpaceBody.GetPosition().magnitude > SpaceControllerSingleton.Get.localRange) {
            if (localSpaceBody.isLoaded) localSpaceBody.Unload();
            state = CelestialObjectState.KEPLERIAN;
        }
        else {
            if (!localSpaceBody.isLoaded
                && localSpaceBody.rb != null
                && UniversalTimeSingleton.Get.timeScale == 1
            ) { 
                localSpaceBody.Load();
                state = CelestialObjectState.NEWTONIAN;

                Vector3 vel = (Vector3)refOrbit.GetLocalVel();
                vel = new(vel.x, vel.z, -vel.y);

                localSpaceBody.rb.linearVelocity = vel;
            }
            else if (UniversalTimeSingleton.Get.timeScale != 1) {
                if (localSpaceBody.isLoaded) localSpaceBody.Unload();
                state = CelestialObjectState.KEPLERIAN;
            }
        }
    }

    public Vector3d GetOrbitPosition() {
        if (state == CelestialObjectState.NON_ORBITTING)
            return localSpaceBody.GetComponent<VesselController>().parentVessel.celestialObject.GetOrbitPosition();

        else
            return refOrbit.GetWorldPos();
    }


    #endregion
//--##
}
