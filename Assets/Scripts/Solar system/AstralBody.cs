using UnityEngine;

public class AstralBody : MonoBehaviour
{
    //
    [field: SerializeField] public Vector3d position { get; private set; }
    [field: SerializeField] public double mass { get; private set; }
    public double mu { get; private set; }
    
    public static double G = 6.6743E-11;
    //

    private void Start() {
        mu = mass*G;    
    }
}
