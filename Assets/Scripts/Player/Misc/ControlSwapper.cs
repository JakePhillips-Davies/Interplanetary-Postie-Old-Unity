using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSwapper : MonoBehaviour
{
    [SerializeField] private bool fromPlayer;
    [SerializeField] private bool toPlayer;
    public GameObject swapFrom;
    public GameObject swapTo;

    private void Start() {
        if (fromPlayer) {
            swapFrom = SpaceControllerSingleton.Get.player;
        }
        else if (toPlayer) {
            swapTo = SpaceControllerSingleton.Get.player;
        }
    }

    public void Activate()
    {
        swapFrom.SetActive(false);
        swapTo.SetActive(true);
    }
}
