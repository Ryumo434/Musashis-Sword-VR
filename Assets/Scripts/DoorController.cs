using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorOpenOnPress : MonoBehaviour
{
    Animator anim;
    void Awake() => anim = GetComponent<Animator>();
    public void Open() => anim.SetTrigger("OpenDoor");
}