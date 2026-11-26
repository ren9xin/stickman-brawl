using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class 血条 : MonoBehaviour
{
    public UnityEngine.UI.Slider slider;
    public int targetPlayerID;
    private GameObject targetPlayer;

    void Start()
    {
        
    }

    public void restart()
    {
        // 优化1：延迟一帧执行，避开玩家注册与血条初始化的顺序问题
        StartCoroutine(DelayBindPlayer());
    }
    /// <summary>
    /// 延迟绑定玩家，解决执行顺序问题，同时完善判空逻辑
    /// </summary>
    IEnumerator DelayBindPlayer()
    {
        // 延迟0帧（等待当前帧所有Start执行完毕），也可改为 new WaitForSeconds(0.1f) 更稳妥
        yield return null;

        //先判空 血条匹配 管理器，避免空指针
        if (血条匹配.Instance == null)
        {
            yield break; // 终止协程，不再执行后续逻辑
        }

        // 获取对应ID的玩家
        targetPlayer = 血条匹配.Instance.GetPlayerByID(targetPlayerID);

        //先判空 targetPlayer，再执行后续操作（核心修正：解决逻辑错位）
        if (targetPlayer == null)
        {
            yield break;
        }

        // 获取玩家身上的 角色控制 组件，并判空
        角色控制 player = targetPlayer.GetComponent<角色控制>();
        if (player == null)
        {
            yield break;
        }

        player.healthBar = this;

        // 可选：初始化血条最大值（若角色控制中有最大血量属性）
        if (player.maxHealth > 0)
        {
            SetMaxHealth(player.maxHealth);
        }

    }

    public void SetMaxHealth(int health)
    {
        // 给 slider 加判空，避免未赋值导致的异常
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }
    }

    public void SetHealth(int health)
    {
        if (slider != null)
        {
            slider.value = health;
        }
    }
}
