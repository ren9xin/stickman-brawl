using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class 按钮 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Animator animator;
    public GameObject 开始界面;
    public GameObject 游戏主体;
    public GameObject 结算界面;
    public GameObject 游戏数据管理;
    public 随机背景 backgroundScript;
    public 角色随机位置生成 playerSpawnScript;
    public 刺刺生成 bulletSpawnScript; // 引用刺刺生成脚本（协程版本）
    public 风控制 ForceManager;//风力与风向控制
    public 血条 血条1;
    public 血条 血条2;
    void Start()
    {
        游戏主体.SetActive(false);
        结算界面.SetActive(false);
        开始界面.SetActive(true);
        游戏数据管理.SetActive(true);
    }


    //public void OnBeingDrag(PointerEventData eventData)//开始拖拽
    //{

    //}

    //public void OnDrag(PointerEventData eventData)//拖拽时每一帧
    //{

    //}

    //public void OnEndDrag(PointerEventData eventData)//结束拖拽
    //{

    //}



    //public void OnPointerDown(PointerEventData eventData)//鼠标按下
    //{

    //}
    //public void OnPointerUp(PointerEventData eventData)//鼠标抬起
    //{

    //}


    // Update is called once per frame

    public void OnPointerEnter(PointerEventData eventData)//鼠标进入
    {
        animator.SetBool("选中", true);
    }
    public void OnPointerExit(PointerEventData eventData)//鼠标退出
    {
        animator.SetBool("选中", false);
    }

    public void OnPointerClick(PointerEventData eventData)//点击
    {
        OnStartButtonClick();
    }
    public void OnStartButtonClick()// 按钮点击的触发方法（绑定到UI按钮的OnClick事件）
    {
        // 1. 立即触发动画
        animator.SetTrigger("点击");

        // 2. 启动协程，延迟执行后续逻辑
        StartCoroutine(DelayToStartGame(0.23f));
    }

    // 协程方法：延迟指定时间后执行游戏启动逻辑
    IEnumerator DelayToStartGame(float delayTime)
    {
        // 等待指定的延迟时间（秒）
        yield return new WaitForSeconds(delayTime);

        // 延迟结束后执行的逻辑
        开始界面.SetActive(false);
        游戏主体.SetActive(true);
        游戏状态.Instance.currentGameState = GameState.GamePlaying;
        if (backgroundScript != null)
        {
            backgroundScript.TriggerRandomBackground();
        }
        if (playerSpawnScript != null)
        {
            playerSpawnScript.TriggerPlayerSpawn();
        }
        //手动启动刺刺生成协程（此时背景已激活，无时序冲突）
        if (bulletSpawnScript != null)
        {
            bulletSpawnScript.StartSpawnBullet();
        }
        if(ForceManager!=null)
        {
            ForceManager.StartPlayerGunRandomForce();
        }
        血条1.restart();
        血条2.restart();
        
    }

}
