using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableTwoAttach : XRGrabInteractable
{
    public Transform leftAttachTransform;
    public Transform rightAttachTransform;

    protected override void Awake()
    {
        base.Awake();
        attachTransform = null; // wichtig: initial leer lassen
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        SetCorrectAttachTransform(args);
        base.OnSelectEntering(args); // kein return nötig!
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        SetCorrectAttachTransform(args); // optional doppelte Absicherung
        base.OnSelectEntered(args);
    }

    private void SetCorrectAttachTransform(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("Left Hand"))
        {
            attachTransform = leftAttachTransform;
            Debug.Log("AttachPoint gesetzt: Linke Hand");
        }
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
        {
            attachTransform = rightAttachTransform;
            Debug.Log("AttachPoint gesetzt: Rechte Hand");
        }
    }
}
