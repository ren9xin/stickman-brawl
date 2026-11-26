using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class 回到主界面 : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;
    public GameObject 开始界面;
    public GameObject 结算界面;
    // 防抖延迟时间（建议0.05-0.1秒，可根据需求调整，太小无效，太大有延迟感）
    public float eventDelay = 0.08f;
    // 状态锁：标记当前是否为「选中状态」，避免重复触发
    private bool isSelected = false;
    // 延迟协程的引用，用于中途终止无效协程
    private Coroutine delayCoroutine;

    void Awake()
    {
        // 动态获取EventTrigger组件，没有则自动添加
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }
        trigger.triggers = new List<EventTrigger.Entry>();

        // 绑定PointerClick事件
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
        trigger.triggers.Add(entry);
    }

    public void OnPointerEnter(PointerEventData eventData)//鼠标进入
    {
        // 若已经是选中状态，直接返回，避免重复执行
        if (isSelected) return;

        // 终止之前的退出延迟协程（防止进入和退出协程冲突）
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }

        // 启动进入延迟协程，延迟后再触发选中动画
        delayCoroutine = StartCoroutine(DelayEnterEvent());
    }

    public void OnPointerExit(PointerEventData eventData)//鼠标退出
    {
        // 若已经是非选中状态，直接返回
        if (!isSelected) return;

        // 终止之前的进入延迟协程（防止退出和进入协程冲突）
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }

        // 启动退出延迟协程，延迟后再取消选中动画
        delayCoroutine = StartCoroutine(DelayExitEvent());
    }

    public void OnPointerClick(PointerEventData eventData)//点击
    {
        结算界面.SetActive(false);
        开始界面.SetActive(true);
        游戏状态.Instance.currentGameState = GameState.StartUI;
    }

    /// <summary>
    /// 延迟触发选中动画的协程
    /// </summary>
    private IEnumerator DelayEnterEvent()
    {
        // 等待防抖延迟时间
        yield return new WaitForSeconds(eventDelay);
        // 标记为选中状态
        isSelected = true;
        // 触发动画
        animator.SetBool("选中", true);
        // 清空协程引用
        delayCoroutine = null;
    }

    /// <summary>
    /// 延迟取消选中动画的协程
    /// </summary>
    private IEnumerator DelayExitEvent()
    {
        // 等待防抖延迟时间
        yield return new WaitForSeconds(eventDelay);
        // 标记为非选中状态
        isSelected = false;
        // 取消动画
        animator.SetBool("选中", false);
        // 清空协程引用
        delayCoroutine = null;
    }
}
