using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSwapper : MonoBehaviour
{
    public GameObject swapFrom;
    public GameObject swapTo;
    public void Activate()
    {
        swapFrom.SetActive(false);
        swapTo.SetActive(true);
    }
}
