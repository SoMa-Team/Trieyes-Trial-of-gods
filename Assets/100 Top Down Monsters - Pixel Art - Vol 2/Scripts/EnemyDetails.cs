using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdmurinsMonsters
{
    public class EnemyDetails : MonoBehaviour
    {
        public bool hasMove, hasAbility, hasAttack, hasAttack2;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (animator.GetBool("Move") == true)
            {
                if (!hasMove)
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 0.2f;
                    spriteRenderer.color = tmp;
                }
                else
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 1f;
                    spriteRenderer.color = tmp;
                }
            }
            else if (animator.GetBool("Ability") == true)
            {
                if (!hasAbility)
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 0.2f;
                    spriteRenderer.color = tmp;
                }
                else
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 1f;
                    spriteRenderer.color = tmp;
                }
            }
            else if (animator.GetBool("Attack") == true)
            {
                if (!hasAttack)
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 0.2f;
                    spriteRenderer.color = tmp;
                }
                else
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 1f;
                    spriteRenderer.color = tmp;
                }
            }
            else if (!hasAttack2)
            {
                if (animator.GetBool("Attack 2") == true)
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 0.2f;
                    spriteRenderer.color = tmp;
                }
                else
                {
                    Color tmp = spriteRenderer.color;
                    tmp.a = 1f;
                    spriteRenderer.color = tmp;
                }
            }
            else
            {
                Color tmp = spriteRenderer.color;
                tmp.a = 1f;
                spriteRenderer.color = tmp;
            }
        }
    }
}
