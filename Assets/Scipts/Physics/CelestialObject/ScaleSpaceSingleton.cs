using EditorAttributes;
using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class ScaleSpaceSingleton : MonoBehaviour
{    
//--#
    #region Variables


    public static ScaleSpaceSingleton Get { get; private set; } = null;

    [field: Title("Settings")]
    [field: SerializeField] public float scaleDownFactor { get; private set; } = 100000;
    [field: SerializeField] public Transform localSpaceTransform { get; private set; } = null;
    [field: SerializeField] public Transform scaledSpaceTransform { get; private set; } = null;


    #endregion
//--#



//--#
    #region Unity events


#if UNITY_EDITOR
    private void OnValidate() {
        Awake();
    }
#endif

    private void Awake() {
        if (Get != null)
            if (Get != this)
                Debug.LogWarning("ERROR: Multiple instances of " + this + " exists! Are you sure you should be doing this?");

        Get = this;
    }


    #endregion
//--#
}
