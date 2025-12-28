using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : Singleton<GameInput>
{
    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Pause,
        Jump,
        Attack,
        Accelerate,
        Heal,
    }



    public EventHandler OnJumpEvent;
    public EventHandler<SkillType> OnUseSkillEvent;
    
    
    private PlayerInputActions playerInputActions;
    
    protected override void Awake()
    {
        base.Awake();
        
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        
        playerInputActions.Player.Jump.performed += Jump_Performed;
        playerInputActions.Player.Attack.performed += Attack_Performed;
        playerInputActions.Player.Accelerate.performed += Accelerate_Performed;
        playerInputActions.Player.Heal.performed += Heal_Performed;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Jump.performed -= Jump_Performed;
        playerInputActions.Player.Attack.performed -= Attack_Performed;
        playerInputActions.Player.Accelerate.performed += Accelerate_Performed;
        playerInputActions.Player.Heal.performed += Heal_Performed;
        
        playerInputActions.Dispose();
    }
    
    private void Jump_Performed(InputAction.CallbackContext obj)
    {
        OnJumpEvent?.Invoke(this, EventArgs.Empty);
        Debug.Log("跳跃");
    }
    
    private void Attack_Performed(InputAction.CallbackContext obj)
    {
        // Todo: 攻击逻辑
        OnUseSkillEvent?.Invoke(this, SkillType.Attack);
        Debug.Log("攻击");
    }

    private void Accelerate_Performed(InputAction.CallbackContext obj)
    {
        // Todo: 加速逻辑
        OnUseSkillEvent?.Invoke(this, SkillType.Accelerate);
        Debug.Log("加速");
    }
    
    private void Heal_Performed(InputAction.CallbackContext obj)
    {
        // Todo: 回血逻辑
        OnUseSkillEvent?.Invoke(this, SkillType.Heal);
        Debug.Log("回血");
    }
    
    /// <summary>
    /// 获取玩家移动方向向量 (归一化后)
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMovementVectorNormalized()
    {
        // 移动平台输入
        if (Application.isMobilePlatform)
        {
            return default;
        }
        // PC输入
        else
        {
            return playerInputActions.Player.Move.ReadValue<Vector2>().normalized;
        }
    }

    public void DisablePlayerInput()
    {
        playerInputActions.Player.Disable();
    }

    public void EnablePlayerInput()
    {
        playerInputActions.Player.Enable();
    }
}
