using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 枪控制 : MonoBehaviour
{
    public GameObject BulletPre;
    //public static 枪控制 Instance;
    public float speed;//子弹速度
    public float horizontal;
    public Animator MagicCircle;//法阵
    public KeyCode triggerKey;    // 指定触发动画的按键
    public KeyCode upKey;//上移键
    public KeyCode downKey;//下移键
    public string gunid;//武器类别（枪/法杖）
    public string gunclass;//枪械种类
    public int bulletNumber;//子弹数
    [SerializeField] private string playerTag = "Player";
    public bool isAttachedToPlayer;// 标记是否已找到玩家（避免重复获取组件）
    private bool isGunReady = false; // 枪是否已拾取并就绪
    public float attackCooldown; // 攻击间隔（秒），可在Inspector面板调节
    private float currentCooldown = 0f; // 当前冷却剩余时间
    public float burstInterval = 0.1f; // 连发子弹之间的间隔时间（秒），默认0.1秒每颗
    public int burstCount = 3; // 每次连发的子弹数量
    //缓存自身组件（消除重复GetComponent）
    [HideInInspector] public Rigidbody2D gunRb;
    [HideInInspector] public Collider2D[] gunColliders;
    private 子弹控制 bulletPrefabCache; // 缓存子弹预制体组件
    void Start()
    {
        // 关键修改2：初始化组件缓存
        CacheSelfComponents();
        //Instance = this;
        if (gunid == "gun")
        {
            if (BulletPre != null)
            {
                bulletPrefabCache = BulletPre.GetComponent<子弹控制>();
                if (bulletPrefabCache != null)
                {
                    speed = bulletPrefabCache.speed;
                }
            }
        }



    }

    // Update is called once per frame
    void Update()
    {
        Out();
        CheckParentAndGetFireKey();
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime; // 随时间减少冷却剩余
        }
        fire();
        DropGun();

    }
    private void CacheSelfComponents()//组件缓存
    {
        gunRb = GetComponent<Rigidbody2D>();
        gunColliders = GetComponents<Collider2D>();
    }

    public void fire()//射击
    {
        if (!isGunReady || !isAttachedToPlayer || !Input.GetKeyDown(triggerKey) || currentCooldown > 0)
        {
            return;
        }
        
        currentCooldown = attackCooldown;
        if (gunid == "gun" && gunclass == "normal")//手枪
        {
            GunFire();

        }
        else if (gunid == "gun" && gunclass == "ak")//连发步枪
        {
            AkGunFire();
        }
        else if (gunid == "staff")//法杖
        {
                //StaffFire();
        }
        



    }

    public void SetGunReady()
    {
        //取消重复延迟调用，避免逻辑冲突
        CancelInvoke("SetReadyDelayed");
        // 延迟激活，避免帧同步误触（比如延迟0.1秒）
        Invoke("SetReadyDelayed", 0.2f);
    }
    private void SetReadyDelayed()
    {
        isGunReady = true;
    }

    void Out()//出界
    {
        if (transform.position.x > 8.6f || transform.position.x < -8.6f ||
            transform.position.y > 5 || transform.position.y < -5)
        {
            isGunReady = false;
            Destroy(gameObject);
        }
    }

    void DropGun()//子弹用完枪掉下
    {
        if (bulletNumber <= 0 && isAttachedToPlayer)
        {
            if (gunRb != null)
            {
                gunRb.isKinematic = false;
                gunRb.gravityScale = 1;
                gunRb.velocity = Vector2.zero; // 关键修复：清空残留速度
                gunRb.angularVelocity = 0; // 关键修复：清空残留角速度
            }

            // 恢复枪的碰撞体
            if (gunColliders != null && gunColliders.Length > 0)
            {
                foreach (var col in gunColliders)
                {
                    if (col != null)
                        col.isTrigger = false;
                }
            }

            gameObject.transform.SetParent(null);
            Invoke("DestroySelf", 0.01f);// 延迟销毁，避免帧内逻辑冲突
            isAttachedToPlayer = false;
            

        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void CheckParentAndGetFireKey()// 检测父物体并获取攻击键
    {
        // 情况1：无父物体 → 重置状态
        if (transform.parent == null || transform.parent.parent == null)
        {
            isAttachedToPlayer = false;
            triggerKey = KeyCode.None;
            upKey = KeyCode.None;
            downKey = KeyCode.None;
            return;
        }
        else
        {
            // 情况2：有父物体 → 检查是否是玩家
            GameObject parentObj = transform.parent.parent.gameObject;
            if (parentObj.CompareTag(playerTag))
            {
                // 获取玩家脚本（需确保玩家物体挂载了PlayerInput脚本）
                角色控制 playerInput = parentObj.GetComponent<角色控制>();
                horizontal = transform.parent.parent.localScale.x;
                horizontal = Mathf.Sign(horizontal);//获得player的方向
                if (playerInput != null)
                {
                    // 从玩家脚本读取攻击键fireKey
                    triggerKey = playerInput.fireKey;
                    upKey = playerInput.upKey;
                    downKey = playerInput.downKey;
                    isAttachedToPlayer = true;
                }
                else
                {
                    // 父物体是玩家但无PlayerInput脚本
                    isAttachedToPlayer = false;
                }
            }
            else
            {
                // 父物体不是玩家 → 重置状态
                isAttachedToPlayer = false;
                triggerKey = KeyCode.None;
                upKey = KeyCode.None;
                downKey = KeyCode.None;
            }
        }

    }

    void GunFire()//单次射击
    {
        if (bulletNumber <= 0 || BulletPre == null || transform.GetChild(0) == null) return;
        bulletNumber--;
        GameObject bullet = Instantiate(BulletPre, transform.GetChild(0).transform.position, Quaternion.identity);
        bullet.transform.localScale = new Vector3(horizontal * bullet.transform.localScale.x, bullet.transform.localScale.y, 1);
        float rotationZ = horizontal > 0 ? 270f : 90f;
        bullet.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = new Vector3(horizontal, 0, 0) * speed * 2;
        }
    }

    void AkGunFire()//连发步枪射击
    {
        StartCoroutine(GunBurstCoroutine());
    }

    // 协程：固定间隔连发3颗子弹
    IEnumerator GunBurstCoroutine()
    {
        // 循环3次，发射3颗子弹
        for (int i = 0; i < burstCount; i++)
        {
            // 先判断子弹是否充足，不足则中断连发
            if (bulletNumber <= 0)
            {
                break;
            }
            // 执行单次射击
            GunFire();
            // 若不是最后一颗子弹，等待固定间隔时间（最后一颗无需等待，避免多余延迟）
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }
        // 连发结束后，重置攻击冷却
        currentCooldown = attackCooldown;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "boom")
        {
            爆炸特效 b = collision.collider.gameObject.GetComponent<爆炸特效>();
            if (b != null)
            {
                ApplyHitForce(b.moveSpeed, b.power, collision.collider.transform);
            }
        }
        else if (collision.collider.tag == "waterlaser")
        {
            水柱 b = collision.collider.gameObject.GetComponent<水柱>();
            if (b != null)
            {
                ApplyHitForce(b.moveSpeed, b.power, collision.collider.transform);
            }
        }
    }

    private void ApplyHitForce(float moveSpeed, float power, Transform hitTransform)//推力受击
    {
        if (gunRb == null) return;
        int dirFlag = GetLeftRightFlag(transform, hitTransform);
        Vector2 dir = new Vector2(dirFlag * power, 0);
        gunRb.AddForce((Vector2.up + dir) * moveSpeed * 0.7f);
    }

    public int GetLeftRightFlag(Transform objectA, Transform objectB)//判断受击，A是主物体，A在B左侧返回-1，在右侧返回1
    {
        // 安全判断：避免未赋值导致空指针异常
        if (objectA == null || objectB == null)
        {
            Debug.LogError("请先为objectA和objectB赋值！");
            return 0; // 异常情况下返回0作为标识
        }

        // 提取物体A和B的世界坐标X值（水平方向）
        float posA_X = objectA.position.x;
        float posB_X = objectB.position.x;

        // 判断左右并返回对应值
        if (posA_X < posB_X)
        {
            // A的X坐标更小 → A在B的左侧 → 返回-1
            return -1;
        }
        else
        {
            // A的X坐标更大（或相等） → A在B的右侧 → 返回1
            return 1;
        }
    }
}
