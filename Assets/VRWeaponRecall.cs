using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRWeaponRecallSimple : MonoBehaviour
{
    [Header("? Eingabe")]
    public ActionBasedController rightControllerInput;
    public ActionBasedController leftControllerInput;

    [Header("?? Ziel (AttachPoint an der Hand)")]
    public XRBaseControllerInteractor rightHandTarget;
    public XRBaseControllerInteractor leftHandTarget;

    [Header("? Einstellungen")]
    public float recallSpeed = 10f;

    private Rigidbody rb;
    private bool isRecalling = false;
    private XRBaseControllerInteractor activeInteractor;
    private ActionBasedController activeInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError("? Kein Rigidbody am Schwert!");
        if (rightControllerInput == null || leftControllerInput == null)
            Debug.LogError("? ActionBasedController für beide Hände müssen gesetzt sein!");
        if (rightHandTarget == null || leftHandTarget == null)
            Debug.LogError("? Beide Hand-Interactor müssen gesetzt sein!");
    }

    void Update()
    {
        // Prüfen: Wird Grip rechts oder links gedrückt?
        bool rightGrip = rightControllerInput.selectAction.action.ReadValue<float>() > 0.5f;
        bool leftGrip = leftControllerInput.selectAction.action.ReadValue<float>() > 0.5f;

        // Start Recall mit rechter oder linker Hand (aber nur wenn nicht bereits aktiv)
        if (rightGrip && !isRecalling)
        {
            activeInteractor = rightHandTarget;
            activeInput = rightControllerInput;
            StartRecall();
        }
        else if (leftGrip && !isRecalling)
        {
            activeInteractor = leftHandTarget;
            activeInput = leftControllerInput;
            StartRecall();
        }

        // Wenn bereits gegriffen, dann Recall stoppen
        if (isRecalling && GetComponent<XRGrabInteractable>().isSelected)
        {
            Debug.Log("?? Recall abgebrochen: Schwert wurde gegriffen");
            isRecalling = false;
            rb.isKinematic = false;
            return;
        }

        // Rückflug ausführen
        if (isRecalling)
        {
            FlyBackToHand();
        }
    }

    void StartRecall()
    {
        isRecalling = true;
        rb.isKinematic = true;
        transform.SetParent(null);
    }

    void FlyBackToHand()
    {
        Transform target = activeInteractor.attachTransform != null
            ? activeInteractor.attachTransform
            : activeInteractor.transform;

        Vector3 targetPos = target.position;
        Quaternion targetRot = target.rotation;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, recallSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            FinishRecall();
        }
    }

    void FinishRecall()
    {
        isRecalling = false;
        rb.isKinematic = false;

        var grab = GetComponent<XRGrabInteractableTwoAttach>();

        // AttachPoint passend zur Hand setzen
        if (activeInteractor.CompareTag("Left Hand"))
            grab.attachTransform = grab.leftAttachTransform;
        else if (activeInteractor.CompareTag("Right Hand"))
            grab.attachTransform = grab.rightAttachTransform;

        // Nur greifen, wenn der Grip noch gehalten wird
        bool isGripStillHeld = activeInput.selectAction.action.ReadValue<float>() > 0.5f;

        if (isGripStillHeld)
        {
            activeInteractor.interactionManager.SelectEnter(activeInteractor, grab);
            Debug.Log("?? Manuelles Greifen (Grip aktiv)");
        }
        else
        {
            Debug.Log("?? Kein Grip mehr ? nicht greifen");
        }
    }
}
