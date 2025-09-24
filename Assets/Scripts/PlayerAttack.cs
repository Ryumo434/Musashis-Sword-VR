// PlayerAttack.cs
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;
    public int attackDamage = 1;
    public Camera cam; 
    

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                    Debug.Log("Hit " + enemy.name);
                }
            }
        }
    }
}
