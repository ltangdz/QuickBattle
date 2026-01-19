using System;
using UnityEngine;

/// <summary>
/// 地面检测
/// </summary>
public class GroundDetectPoint : MonoBehaviour
{
    [Space(10)]
    [Header("地面检测")]
    [Tooltip("是否在地上")]
    public bool isGrounded;
    [Tooltip("检测触发器半径")]
    [SerializeField] float groundDetectRadius = 0.3f;

    private void OnTriggerEnter(Collider other)
    {
        // 碰到地面 且 速度向下
        if (other.gameObject.CompareTag(Settings.TAG_GROUND))
        {
            Debug.Log("落地");
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 离地检测
        if (other.gameObject.CompareTag(Settings.TAG_GROUND))
        {
            Debug.Log("离地");
            isGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundDetectRadius);
    }
}