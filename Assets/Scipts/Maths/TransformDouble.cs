using System.Linq;
using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class TransformDouble : MonoBehaviour
{    
//--#
    #region Variables

    
    [field: SerializeField] public Vector3d position {get; private set;}
    [field: SerializeField] public Vector3d localPosition {get; private set;}
    private TransformDouble m_parent;
    [field: SerializeField] public TransformDouble parent {get; private set;}
    [field: SerializeField] public TransformDouble[] children {get; private set;}


    #endregion
//--#



//--#
    #region Positioning


    public void SetLocalPosition(Vector3d _position) {
        localPosition = _position;

        if (parent != null)
            position = _position + parent.position;
        else 
            position = _position;

        foreach (TransformDouble child in children) {
            child.SetLocalPosition(child.localPosition);
        }
    }

    public void ToTransform() {
        transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);
    }
    public void ToTransform(Vector3d offset) {
        transform.position = new Vector3((float)(position.x + offset.x), (float)(position.y + offset.y), (float)(position.z + offset.z));
    }
    public void ToTransform(TransformDouble centre) {
        ToTransform(-centre.position);
    }



    #endregion
//--#



//--#
    #region Parenting


    public void AddChild(TransformDouble _child) {
        if (children.Contains(_child)) return;

        TransformDouble[] newChildren = new TransformDouble[children.Length + 1];
        for (int i = 0; i < children.Length; i++) {
            newChildren[i] = children[i];
        }
        newChildren[newChildren.Length - 1] = _child;
        children = newChildren;

        _child.parent = this;
    }

    public void RemoveChild(TransformDouble _child) {
        if (!children.Contains(_child)) {
            Debug.Log("Child: ("+ _child +") not found in parent");
            return;
        }

        TransformDouble[] newChildren = new TransformDouble[children.Length - 1];
        for (int i = 0, j = 0; i < children.Length; i++, j++) {
            if (children[i] == _child)
                j--;
            else
                newChildren[j] = children[i];
        }
        children = newChildren;

        _child.parent = null;
    }

    public void Parent(TransformDouble _parent) {
        if (parent != null)
            parent.RemoveChild(this);

        _parent.AddChild(this);
    }


    #endregion
//--#

//--#
    #region misc


#if UNITY_EDITOR
    private void OnValidate() {
        if (parent != m_parent) {
            m_parent?.RemoveChild(this);
            parent?.AddChild(this);
            m_parent = parent;
        }
    }
#endif


    #endregion
//--#
}
