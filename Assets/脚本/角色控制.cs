using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class 角色控制 : MonoBehaviour
{
    public GameObject platformPre;//复活平台
    Rigidbody2D body;
    public float speed;
    public bool IsGround;
    public float jumpforce;
    private Transform gunTrans;//手枪生成点
    private Transform platformTrans;//复活平台生成点
    枪控制 nowgun;//当前手枪
    法杖控制 nowstaff;//当前法杖
    public 枪控制 gun;
    private Vector2 dir;
    private float moveStartTime;// 运动开始时间
    private float moveSpeed;//击飞速度
    bool isKnocked;//击飞延迟判断
    public float knockbackDuration = 0.3f; // 击飞硬直时间（惯性保留时间）
    private float knockbackStartTime; // 击飞开始时间
    public float jumpcount;//空中可以跳跃次数
    public int maxHealth=4;//玩家最大生命值
    public int currentHealth;//玩家当前生命值
    public 血条 healthBar;//玩家血条
    public KeyCode leftKey;//左移键
    public KeyCode rightKey;//右移键
    public KeyCode jumpKey;//跳跃键
    public KeyCode fireKey;//发射键
    public KeyCode upKey;//上移键
    public KeyCode downKey;//下移键
    public string characterId;//角色Id
    public GameObject 胜利玩家;
    public 角色控制 enermy;//攻击对象
    public bool ismove;//允许移动
    public bool isjump;//允许跳跃
    public int playerID; // 玩家编号（1=玩家1，2=玩家2，在预制体上设置
    // 武器触发防抖变量
    private float weaponTriggerCoolDown = 0.2f;
    private float lastWeaponTriggerTime;
    private Vector3 PlayerLocalScale;//玩家初始大小
    void Awake()
    {
        currentHealth = maxHealth;
        // 注册到全局玩家管理器
        if (血条匹配.Instance != null)
        {
            血条匹配.Instance.RegisterPlayer(playerID, gameObject);
        }
        if (healthBar != null)
        {
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(maxHealth);
        }
        PlayerLocalScale = transform.localScale;
        currentHealth = maxHealth;
        jumpcount = 2;
        body = GetComponent<Rigidbody2D>();
        speed = 3;
        IsGround = true;
        jumpforce = 9;
        gunTrans = transform.GetChild(0);//手枪生成点
        platformTrans = transform.GetChild(1);
        moveSpeed = 400;
        // 将角色刚体设为不受子物体物理影响（防止枪的碰撞体推挤角色）
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        isjump = true;
        ismove = true;
    }
    void Start()
    {
        
        
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(游戏状态.Instance.currentGameState!= GameState.GamePlaying)
        {
            Destroy(gameObject);
        }
        GameOver();
        outOfBound();
        // 优先处理击飞逻辑：每帧检查击飞状态是否结束
        CheckKnockbackEnd();
        if (!isKnocked)
        {
            right_left();
            jump();
            getGun();
            /*DropGun();*/
        }

        //beAttacked();
    }

    void right_left()
    {
        if (ismove)
        {
            if (Input.GetKey(leftKey))
            {
                body.velocity = new Vector2(-speed, body.velocity.y);
                transform.localScale = new Vector3(-PlayerLocalScale.x,PlayerLocalScale.y,PlayerLocalScale.z);


            }
            else if (Input.GetKey(rightKey))
            {
                body.velocity = new Vector2(speed, body.velocity.y);
                transform.localScale = PlayerLocalScale;
            }
            /*else消除惯性
            {
                body.velocity = new Vector2(0, body.velocity.y);

            }*/
            else
            {
                // 无输入时，给X轴速度加阻尼，模拟地面摩擦力
                body.velocity = new Vector2(Mathf.Lerp(body.velocity.x, 0, 0.1f), body.velocity.y);
            }
        }
    }

    void jump()
    {
        if (isjump)
        {
            /*if (Input.GetKeyDown(jumpKey) && jumpcount > 0 && IsGround)//离地时，防止和碰撞框一起被判定两次
            {
                body.velocity = new Vector2(body.velocity.x, jumpforce);
            }*/
            if (Input.GetKeyDown(jumpKey) && jumpcount > 0)
            {
                jumpcount -= 1;
                body.velocity = new Vector2(body.velocity.x, jumpforce);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "ground" && body.velocity.y <= 0)//地面
        {
            IsGround = true;
            jumpcount = 2;
        }

        if (collision.collider.tag == "bullet")//受击
        {
            子弹控制 b = collision.collider.gameObject.GetComponent<子弹控制>();
            if (b != null)
            {
                beAttacked(collision.collider.transform.localScale.x, b.power);
            }
            Destroy(collision.gameObject);
        }
        if (collision.collider.tag == "waterlaser")//受击
        {
            水柱 b = collision.collider.gameObject.GetComponent<水柱>();
            if (b != null)
            {
                float m = moveSpeed;
                moveSpeed = b.moveSpeed;
                beAttacked(GetLeftRightFlag(transform, collision.collider.transform), b.power);
                moveSpeed = m;
            }
        }
        if (collision.collider.tag == "boom")//受击
        {
            爆炸特效 b = collision.collider.gameObject.GetComponent<爆炸特效>();
            float m = moveSpeed;
            moveSpeed = b.moveSpeed;
            beAttacked(GetLeftRightFlag(transform, collision.collider.transform), b.power);
            moveSpeed = m;
        }
        /*if (collision.collider.tag == "laser")//受击
        {
            beAttacked(UnityEngine.Random.value > 0.5f ? 1 : -1, 10f);
        }*/
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        /*if (collision.collider.tag == "ground")
        {
            IsGround = false;
            jumpcount -= 1;
        }*/
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 武器触发防抖，避免重复触发
        if (Time.time - lastWeaponTriggerTime < weaponTriggerCoolDown)
        {
            return;
        }
        if (collision.tag == "Player")//进入攻击范围
        {
            enermy = collision.GetComponent<角色控制>();
        }
        else if (collision.tag == "gun")//抓手枪
        {
            gun = collision.GetComponent<枪控制>();
            lastWeaponTriggerTime = Time.time; // 更新触发时间
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            enermy = null;
        }
        if (collision.tag == "gun")
        {

            gun = null;
        }
    }

    void getGun()
    {
        if (Input.GetKeyDown(fireKey))
        {
            if (enermy != null && nowgun == null)//徒手攻击
            {
                enermy.beAttacked(transform.localScale.x, 0.5f);//击飞
            }
            //拿枪
            else if (gunTrans.childCount == 0 && gun != null && gun.isAttachedToPlayer==false)//持枪时无法被抢
            {
                if (Mathf.Sign(gun.transform.localScale.x) != Mathf.Sign(-transform.localScale.x))//改变手枪朝向
                {
                    gun.transform.localScale = new Vector3(-gun.transform.localScale.x, gun.transform.localScale.y, gun.transform.localScale.z);
                }
                gun.SetGunReady();

                // 使用枪的缓存组件
                if (gun.gunRb != null)
                {
                    gun.gunRb.isKinematic = true; //不受物理影响（比只改gravityScale更彻底）
                    gun.gunRb.velocity = Vector2.zero;// 清空速度，避免残留运动
                    gun.gunRb.angularVelocity = 0;// 清空角速度，避免旋转
                    gun.gunRb.gravityScale = 0;// 双重保障：重力为0
                }

                //使用枪的缓存碰撞体，禁用枪的碰撞体
                if (gun.gunColliders != null && gun.gunColliders.Length > 0)
                {
                    foreach (var col in gun.gunColliders)
                    {
                        if (col != null)
                            col.isTrigger = true;// 设为触发器，不参与物理碰撞
                    }
                }

                // 2. 设置父物体（挂载到生成点）
                gun.transform.SetParent(gunTrans);

                // 3. 精准固定位置：本地坐标归零（完全贴合生成点）
                gun.transform.localPosition = Vector3.zero;

                // 4. 固定旋转：本地旋转归零（和生成点角度一致，不会歪）
                gun.transform.localRotation = Quaternion.identity;
                if (gun.gunid == "staff")
                {
                    法杖控制 staff = gun.GetComponent<法杖控制>();
                    if (staff != null)
                    {
                        staff.InitPlayerAndColumn();
                        staff.SetGunReady();
                    }
                }
                nowgun = gun;

            }

        }
    }




    public void beAttacked(float sign, float power)//受击(方向,力道）
    {
        isKnocked = true;
        knockbackStartTime = Time.time;
        dir = new Vector2(Mathf.Sign(sign) * power, 0);
        body.AddForce((Vector2.up + dir) * moveSpeed);
        dir = Vector2.zero;
    }
    // 检查击飞状态是否结束
    private void CheckKnockbackEnd()
    {
        if (!isKnocked) return;

        float elapsedTime = Time.time - knockbackStartTime;
        // 击飞硬直时间到，恢复手动控制和原物理参数
        if (elapsedTime >= knockbackDuration)
        {
            isKnocked = false;
        }
    }

    void outOfBound()//出界
    {
        if (transform.position.x < -9.8f || transform.position.x > 9.8f || transform.position.y < -6.6f || transform.position.y > 6.6f)
        {
            if (nowgun != null)//死亡掉枪
            {
                // 使用枪的缓存组件
                if (nowgun.gunRb != null)
                {
                    nowgun.gunRb.isKinematic = false;
                    nowgun.gunRb.gravityScale = 1;//恢复重力
                }

                // 恢复枪的碰撞体
                if (nowgun.gunColliders != null && nowgun.gunColliders.Length > 0)
                {
                    foreach (var col in nowgun.gunColliders)
                    {
                        if (col != null)
                            col.isTrigger = false;
                    }
                }


                nowgun.transform.SetParent(null);
                Destroy(nowgun.gameObject);
            }
            currentHealth -= 1;
            if(healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }
            
            transform.position = new Vector2(UnityEngine.Random.Range(-6.6f, 5.8f), 3.5f);
            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
            if (platformPre != null && platformTrans!=null)
            {
                GameObject platform = Instantiate(platformPre, platformTrans.position, Quaternion.identity);
                Destroy(platform, 3f);
            }
            
        }

    }

    void GameOver()
    {

        if (currentHealth <= 0)//角色死亡
        {
            if (characterId == "Player1")
            {
                游戏数据管理.Instance.WinnerId = "Player2";
            }
            else
            {
                游戏数据管理.Instance.WinnerId = "Player1";
            }
            /*if (游戏主体 != null)
            {
                游戏主体.SetActive(false);
            }
            if(结算界面 != null)
            {
                结算界面.SetActive(true);
            }
            if (胜利玩家 != null)
            {
                胜利玩家.GetComponent<胜利玩家>().ShowWinner();
            }*/
            游戏状态.Instance.currentGameState = GameState.GameOver;
            
        }
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

    /*void FindEnermy()//找到敌人
    {
        // 步骤1：先查找所有标签为"Player"的对象（避免只找第一个可能是自身的情况）
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        角色控制 targetPlayer = null;

        // 步骤2：遍历所有Player对象，排除自身，找到真正的玩家
        foreach (GameObject obj in playerObjects)
        {
            // 关键判断：obj != this.gameObject（排除当前执行脚本的敌人自身）
            if (obj != this.gameObject)
            {
                targetPlayer = obj.GetComponent<角色控制>();
                break; // 找到第一个非自身的Player对象，退出循环
            }
        }

        // 步骤3：赋值给enermy变量，同时添加容错判断
        if (targetPlayer != null)
        {
            enermy = targetPlayer;
        }
    }*/
}
