using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdmurinsMonsters
{
    public class MonsterManager : MonoBehaviour
    {
        public Animator[] monsterAnimators;
        public bool facingUp, facingRight, facingLeft, facingDown;
        // Start is called before the first frame update
        void Start()
        {

        }

        private void Update()
        {
            ChangeDirection();
            ChangeAnimation();
        }

        private void ChangeDirection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _FaceUp();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _FaceDown();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                _FaceLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                _FaceRight();
            }
        }

        private void ChangeAnimation()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _AnimationIdle();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _AnimationMove();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _AnimationAttack();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _AnimationAttack_2();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _AnimationAbility();
            }
        }

        public void _FaceRight()
        {
            ResetDirection();
            facingRight = true;
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetFloat("Horizontal", 1);
                monster.SetFloat("Vertical", 0);
            }
        }
        public void _FaceLeft()
        {
            ResetDirection();
            facingLeft = true;
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetFloat("Horizontal", -1);
                monster.SetFloat("Vertical", 0);
            }
        }
        public void _FaceUp()
        {
            ResetDirection();
            facingUp = true;
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetFloat("Horizontal", 0);
                monster.SetFloat("Vertical", 1);
            }
        }
        public void _FaceDown()
        {
            ResetDirection();
            facingDown = true;
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetFloat("Horizontal", 0);
                monster.SetFloat("Vertical", -1);
            }
        }

        public void _AnimationIdle()
        {
            ResetAnimations();
        }

        public void _AnimationAttack()
        {
            ResetAnimations();
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetBool("Attack", true);
            }
        }

        public void _AnimationAttack_2()
        {
            ResetAnimations();
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetBool("Attack 2", true);
            }
        }

        public void _AnimationMove()
        {
            ResetAnimations();
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetBool("Move", true);
            }
        }

        public void _AnimationAbility()
        {
            ResetAnimations();
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetBool("Ability", true);
            }
        }
        private void ResetDirection()
        {
            facingRight = false;
            facingLeft = false;
            facingUp = false;
            facingDown = false;
        }
        private void ResetAnimations()
        {
            foreach (Animator monster in monsterAnimators)
            {
                monster.SetBool("Move", false);
                monster.SetBool("Attack", false);
                monster.SetBool("Attack 2", false);
                monster.SetBool("Ability", false);
            }
        }

        public void OpenLink(string link)
        {
            Application.OpenURL(link);
        }

    }
}
