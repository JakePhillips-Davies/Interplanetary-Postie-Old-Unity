using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class OrbitSettingsSingleton : MonoBehaviour
{    
//--#
    #region Variables


    public static OrbitSettingsSingleton Get { get; private set; } = null;
    
    [field: SerializeField] public int patchLimit  {get; private set;}


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



//--#
    #region Misc functions


    


    #endregion
//--#
}
