using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SPUM_move : MonoBehaviour
{
    private Animator animator;
    public InputActionReference moveAction;
    public InputActionReference attackAction;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
        if (attackAction != null)
        {
            attackAction.action.Enable();
        }
    }

    void Update()
    {
        // Input Action 값을 받아서 animator의 State 값을 변경
        // Parameter 목록 : 2_Attack, 1_Move, 3_Damaged, 4_Death, 6_Other, 5_Debuff, IsDeath
        if (moveAction != null)
        {
            Vector2 moveDir = moveAction.action.ReadValue<Vector2>();
            if(moveDir.magnitude > 0.1f)
            {
                animator.SetBool("1_Move", true);
            }
            else
            {
                animator.SetBool("1_Move", false);
            }
            transform.Translate(moveDir * Time.deltaTime * 5f);
        }
        if (attackAction != null)
        {
            // attackAction이 눌렸을 때 true, 떼면 false로 처리
            if (attackAction.action.ReadValue<float>() > 0)
            {
                animator.SetTrigger("2_Attack");
            }
        }
    }
}