using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 退出游戏按钮脚本（支持UGUI按钮点击）
/// 挂载方式：直接挂载到退出游戏按钮对象上
/// </summary>
public class 退出游戏 : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;
    public void OnPointerEnter(PointerEventData eventData)//鼠标进入
    {
        animator.SetBool("选中", true);
    }
    public void OnPointerExit(PointerEventData eventData)//鼠标退出
    {
        animator.SetBool("选中", false);
    }
    /// <summary>
    /// UGUI按钮点击回调（无需手动绑定，挂载即生效）
    /// </summary>
    /// <param name="eventData">点击事件数据</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 调用退出游戏核心方法
        ExitGame();
    }

    /// <summary>
    /// 退出游戏核心逻辑（兼容编辑器+真机）
    /// </summary>
    private void ExitGame()
    {
        // 调试日志：打印退出信息（控制台可见）
        Debug.Log("点击退出游戏按钮，准备退出游戏...");

#if UNITY_EDITOR
        // Unity编辑器中：停止游戏运行（回到编辑模式，不关闭编辑器）
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 真机环境（PC/移动端/主机等）：退出应用程序
        Application.Quit();
#endif
    }

    // 可选：通过键盘按键退出（如ESC键，按需保留）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }
}
