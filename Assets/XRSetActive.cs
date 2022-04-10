using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

public class XRSetActive : MonoBehaviour
{
    [SerializeField] private bool enableWithXR;

    void Start()
    {
        if (enableWithXR) {
            gameObject.SetActive(XRGeneralSettings.Instance?.Manager?.activeLoader != null);
        } else {
            gameObject.SetActive(XRGeneralSettings.Instance?.Manager?.activeLoader == null);
        }
    }
}
