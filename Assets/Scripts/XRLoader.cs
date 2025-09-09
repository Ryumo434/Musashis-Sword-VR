using UnityEngine;
using UnityEngine.XR.Management;

public class XRLoader : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(StartXR());
    }

    System.Collections.IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check XR Plug-in settings.");
        }
        else
        {
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            Debug.Log("XR started successfully.");
        }
    }

    void OnDestroy()
    {
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    }
}

