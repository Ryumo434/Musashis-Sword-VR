using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Hinge setup")]
    [SerializeField] Transform door;          // Das bewegte Tür-Transform (z. B. Kind-Objekt)
    [SerializeField] Vector3 openEuler = new Vector3(0, 90, 0); // Zielrotation relativ zu geschlossen
    [SerializeField] float openTime = 0.75f;
    [SerializeField] bool autoClose = false;
    [SerializeField] float autoCloseDelay = 3f;
    [SerializeField] EnemyManager enemyManager;

    Quaternion _closedRot;
    Quaternion _openRot;
    bool _isOpen = false;
    bool _isAnimating = false;

    void Awake()
    {
        if (door == null) door = transform;
        _closedRot = door.localRotation;
        _openRot = Quaternion.Euler(door.localEulerAngles + openEuler);
    }

    public void TryOpen()
    {
        // Condition: alle Gegner tot

        if (_isOpen || _isAnimating) return;
        StartCoroutine(AnimateDoor(_openRot));

        if (autoClose) StartCoroutine(AutoCloseRoutine());
    }

    IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (_isOpen && !_isAnimating) StartCoroutine(AnimateDoor(_closedRot));
    }

    IEnumerator AnimateDoor(Quaternion target)
    {
        _isAnimating = true;
        Quaternion start = door.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openTime;
            door.localRotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0,1,t));
            yield return null;
        }
        door.localRotation = target;
        _isOpen = (target == _openRot);
        _isAnimating = false;
    }
}