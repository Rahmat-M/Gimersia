using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;

    private PlayerMovement move;
    private bool isAttacking = false;

    void Start()
    {
        move = GetComponent<PlayerMovement>();
    }

    public void PlayCard(CardInstance card)
    {
        if (isAttacking) return; // cegah spam attack

        switch (card.data.cardType)
        {
            case CardType.Melee:
                StartCoroutine(DoMelee(card));
                break;

            case CardType.Range:
                DoRange(card);
                break;
        }
    }

    IEnumerator DoMelee(CardInstance card)
    {
        isAttacking = true;

        // Hitung arah ke mouse (bukan cuma arah gerak terakhir)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDir = (mouseWorld - transform.position);
        attackDir.Normalize();

        // Tentukan animasi arah serangan
        string attackAnim = GetAttackAnimationName(attackDir);
        animator.Play(attackAnim);

        Debug.Log($"[PlayerCombat] Melee attack {card.data.cardName} ({attackAnim})");

        // durasi animasi bisa kamu sesuaikan (misal 0.4 detik)
        yield return new WaitForSeconds(0.4f);

        animator.SetBool("IsAttacking", false);
        isAttacking = false;
    }

    void DoRange(CardInstance card)
    {
        // Hitung arah ke mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - projectileSpawnPoint.position).normalized;

        Debug.Log($"[PlayerCombat] Range attack: {card.data.cardName}, dir={dir}");

        // Buat projectile
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            var obj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            var proj = obj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetDirection(dir);
                proj.SetDamage(card.data.damage);
            }
        }

        // Putar animasi idle menghadap arah tembakan (tanpa rotasi transform)
        UpdateFacingDirection(dir);
    }

    string GetAttackAnimationName(Vector2 dir)
    {
        // Bandingkan sumbu dominan
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
                return "attack1_right";
            else
                return "attack1_left";
        }
        else
        {
            if (dir.y > 0)
                return "attack1_up";
            else
                return "attack1_down";
        }
    }

    void UpdateFacingDirection(Vector2 dir)
    {
        // Simpan arah terakhir (agar animasi idle menghadap ke arah terakhir tembakan)
        move.SetLastMoveDirection(dir);

        // Update parameter animator agar animasi idle tetap arah yang benar
        animator.SetFloat("LastMoveX", dir.x);
        animator.SetFloat("LastMoveY", dir.y);
    }
}
