using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class 平台移动跟随 : MonoBehaviour
{
    private Rigidbody2D _weaponRb;
    private Transform _originalParent; // 记录武器原始父物体，方便后续解除绑定
    枪控制 gun;
    private bool isBinding = false;
    private Transform _tempPlatformTrans; // 临时存储平台Transform，解决Invoke无法传参问题
    void Start()
    {
        // 获取武器刚体组件
        _weaponRb = GetComponent<Rigidbody2D>();
        gun=GetComponent<枪控制>();
        _originalParent = transform.parent;
    }

    // 碰撞检测（非触发模式：武器与平台直接碰撞时触发）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_weaponRb.velocity.y<=0 && gun.isAttachedToPlayer==false)
        {
            BindToPlatform(collision.gameObject);
        }
        
    }



    /// <summary>
    /// 核心逻辑：绑定武器到移动平台
    /// </summary>
    /// <param name="collisionObj">碰撞到的对象（平台）</param>
    private void BindToPlatform(GameObject collisionObj)
    {
        if (collisionObj.CompareTag("ground") && !isBinding)
        {
            isBinding = true; // 防止重复触发
                              // 延迟0.1秒绑定，跳过碰撞抖动帧
            _tempPlatformTrans = collisionObj.transform; // 存储平台Transform到临时变量
            Invoke(nameof(DoBindToPlatform), 0.1f);
        }
    }

    
    /// <summary>
    /// 实际执行绑定逻辑
    /// </summary>
    /// <param name="platformTrans">平台Transform</param>
    private void DoBindToPlatform()
    {
        if (_tempPlatformTrans == null)
        {
            isBinding= false;
            return;
        }
        transform.SetParent(_tempPlatformTrans);
        if (_weaponRb != null)
        {
            _weaponRb.velocity = Vector3.zero;
            _weaponRb.angularVelocity = 0f;

            //_weaponRb.constraints = RigidbodyConstraints2D.FreezeRotation| RigidbodyConstraints2D.FreezePositionY;
        }
        _tempPlatformTrans = null; // 清空临时变量，避免残留
        isBinding = false;
    }
}
