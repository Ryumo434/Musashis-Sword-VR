using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRWeaponRecallSimple : MonoBehaviour
{
    [Header("Input")]
    public ActionBasedController rightControllerInput;
    public ActionBasedController leftControllerInput;

    [Header("Hand Targets")]
    public XRBaseControllerInteractor rightHandTarget;
    public XRBaseControllerInteractor leftHandTarget;

    [Header("Optional Climbing Wall Trigger")]
    [Tooltip("Trigger-Collider der rechten oder allgemeinen Climbing Wall")]
    public Collider climbingWallTrigger;

    [Header("Settings")]
    public float recallSpeed = 10f;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private bool isRecalling = false;
    private XRBaseControllerInteractor activeInteractor;
    private ActionBasedController activeInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (rb == null)
            Debug.LogError("Kein Rigidbody am Schwert!", this);

        if (grabInteractable == null)
            Debug.LogError("Kein XRGrabInteractable am Schwert!", this);

        if (rightControllerInput == null || leftControllerInput == null)
            Debug.LogError("ActionBasedController f?r beide H?nde m?ssen gesetzt sein!", this);

        if (rightHandTarget == null || leftHandTarget == null)
            Debug.LogError("Beide Hand-Interactor m?ssen gesetzt sein!", this);
    }

    private void Update()
    {
        bool rightGrip = rightControllerInput != null &&
                         rightControllerInput.selectAction.action.ReadValue<float>() > 0.5f;

        bool leftGrip = leftControllerInput != null &&
                        leftControllerInput.selectAction.action.ReadValue<float>() > 0.5f;

        bool rightHandInsideClimbTrigger = IsHandInsideClimbingWall(rightHandTarget);
        bool leftHandInsideClimbTrigger = IsHandInsideClimbingWall(leftHandTarget);

        // Recall nur starten, wenn die entsprechende Hand NICHT an der Kletterwand ist
        if (rightGrip && !isRecalling && !rightHandInsideClimbTrigger)
        {
            activeInteractor = rightHandTarget;
            activeInput = rightControllerInput;
            StartRecall();
        }
        else if (leftGrip && !isRecalling && !leftHandInsideClimbTrigger)
        {
            activeInteractor = leftHandTarget;
            activeInput = leftControllerInput;
            StartRecall();
        }

        // Wenn bereits gegriffen, Recall abbrechen
        if (isRecalling && grabInteractable != null && grabInteractable.isSelected)
        {
            Debug.Log("Recall abgebrochen: Schwert wurde gegriffen");
            isRecalling = false;
            rb.isKinematic = false;
            return;
        }

        if (isRecalling)
        {
            FlyBackToHand();
        }
    }

    private bool IsHandInsideClimbingWall(XRBaseControllerInteractor handInteractor)
    {
        if (handInteractor == null)
            return false;

        Transform handTransform = handInteractor.attachTransform != null
            ? handInteractor.attachTransform
            : handInteractor.transform;

        Vector3 handPosition = handTransform.position;

        return IsPointInsideTrigger(climbingWallTrigger, handPosition);
    }

    private bool IsPointInsideTrigger(Collider triggerCollider, Vector3 point)
    {
        if (triggerCollider == null)
            return false;

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"{triggerCollider.name} ist kein Trigger-Collider.", triggerCollider);
        }

        Vector3 closestPoint = triggerCollider.ClosestPoint(point);

        // Wenn der Punkt innerhalb des Colliders liegt, ist ClosestPoint praktisch derselbe Punkt
        return Vector3.Distance(closestPoint, point) < 0.001f;
    }

    private void StartRecall()
    {
        isRecalling = true;
        rb.isKinematic = true;
        transform.SetParent(null);
    }

    private void FlyBackToHand()
    {
        Transform target = activeInteractor.attachTransform != null
            ? activeInteractor.attachTransform
            : activeInteractor.transform;

        Vector3 targetPos = target.position;
        Quaternion targetRot = target.rotation;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            recallSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 10f
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            FinishRecall();
        }
    }

    private void FinishRecall()
    {
        isRecalling = false;
        rb.isKinematic = false;

        var grab = GetComponent<XRGrabInteractableTwoAttach>();
        if (grab == null)
        {
            Debug.LogError("XRGrabInteractableTwoAttach fehlt am Schwert.", this);
            return;
        }

        if (activeInteractor.CompareTag("Left Hand"))
            grab.attachTransform = grab.leftAttachTransform;
        else if (activeInteractor.CompareTag("Right Hand"))
            grab.attachTransform = grab.rightAttachTransform;

        bool isGripStillHeld = activeInput != null &&
                               activeInput.selectAction.action.ReadValue<float>() > 0.5f;

        if (isGripStillHeld)
        {
            activeInteractor.interactionManager.SelectEnter(
                activeInteractor as IXRSelectInteractor,
                grab as IXRSelectInteractable
            );

            Debug.Log("Manuelles Greifen (Grip aktiv)");
        }
        else
        {
            Debug.Log("Kein Grip mehr -> nicht greifen");
        }
    }
}