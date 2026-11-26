using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class test : MonoBehaviour
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
    public int maxHealth;//玩家最大生命值
    public int currentHealth;//玩家当前生命值
    public 血条 healthBar;//玩家血条
    public KeyCode leftKey;//左移键
    public KeyCode rightKey;//右移键
    public KeyCode jumpKey;//跳跃键
    public KeyCode fireKey;//发射键
    public KeyCode upKey;//上移键
    public KeyCode downKey;//下移键
    public Transform gameob;//游戏主体
    public GameObject 游戏主体;
    public GameObject 结算界面;
    public string characterId;//角色Id
    public GameObject 胜利玩家;
    public 角色控制 enermy;//攻击对象
    public bool ismove;//允许移动
    public bool isjump;//允许跳跃
    void Start()
    {
        maxHealth = 4;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(maxHealth);
        currentHealth = maxHealth;
        jumpcount = 2;
        body = GetComponent<Rigidbody2D>();
        speed = 4;
        IsGround = true;
        jumpforce = 10;
        gunTrans = transform.GetChild(0);//手枪生成点
        platformTrans = transform.GetChild(1);
        moveSpeed = 400;
        // 将角色刚体设为不受子物体物理影响（防止枪的碰撞体推挤角色）
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        isjump = true;
        ismove = true;
    }

    // Update is called once per frame
    void Update()
    {
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
                transform.localScale = new Vector3(-0.38f, 0.38f, 0.5f);


            }
            else if (Input.GetKey(rightKey))
            {
                body.velocity = new Vector2(speed, body.velocity.y);
                transform.localScale = new Vector3(0.38f, 0.38f, 0.5f);
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
            if (Input.GetKeyDown(jumpKey) && jumpcount > 0 && IsGround)//离地时，防止和碰撞框一起被判定两次
            {
                body.velocity = new Vector2(body.velocity.x, jumpforce);
            }
            else if (Input.GetKeyDown(jumpKey) && jumpcount > 0)
            {
                jumpcount -= 1;
                body.velocity = new Vector2(body.velocity.x, jumpforce);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "ground")//地面
        {
            IsGround = true;
            jumpcount = 2;
        }

        if (collision.collider.tag == "bullet")//受击
        {
            子弹控制 b = collision.collider.gameObject.GetComponent<子弹控制>();
            beAttacked(collision.collider.transform.localScale.x, b.power);
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
        if (collision.collider.tag == "ground")
        {
            IsGround = false;
            jumpcount -= 1;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")//进入攻击范围
        {
            enermy = collision.GetComponent<角色控制>();
        }
        else if (collision.tag == "gun")//抓手枪
        {
            //保存箱子
            gun = collision.GetComponent<枪控制>();
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
            else if (gunTrans.childCount == 0 && gun != null && (gun.transform.parent == null || gun.transform.parent == gameob))//持枪时无法被抢
            {
                if (Mathf.Sign(gun.transform.localScale.x) != Mathf.Sign(-transform.localScale.x))//改变手枪朝向
                {
                    gun.transform.localScale = new Vector3(-gun.transform.localScale.x, gun.transform.localScale.y, gun.transform.localScale.z);
                }
                gun.SetGunReady();

                // 1. 获取枪的刚体，完全禁用物理影响（核心：防止物理导致偏移）
                Rigidbody2D gunRb = gun.GetComponent<Rigidbody2D>();
                if (gunRb != null)
                {
                    gunRb.isKinematic = true; // 不受物理影响（比只改gravityScale更彻底）
                    gunRb.velocity = Vector2.zero; // 清空速度，避免残留运动
                    gunRb.angularVelocity = 0; // 清空角速度，避免旋转
                    gunRb.gravityScale = 0; // 双重保障：重力为0
                    // 2. 禁用枪的碰撞体（避免和角色/地面碰撞导致推挤）
                    Collider2D[] gunColliders = gun.GetComponents<Collider2D>();
                    foreach (var col in gunColliders)
                    {
                        col.isTrigger = true; // 设为触发器，不参与物理碰撞
                        // 也可以用 col.enabled = false; 直接禁用，二选一
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
                    staff.InitPlayerAndColumn();
                    staff.SetGunReady();
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
                Rigidbody2D gunRb = nowgun.GetComponent<Rigidbody2D>();
                if (gunRb != null)
                {
                    gunRb.isKinematic = false;
                    gunRb.gravityScale = 1; // 恢复重力
                }

                // 恢复枪的碰撞体
                Collider2D[] gunColliders = nowgun.GetComponents<Collider2D>();
                foreach (var col in gunColliders)
                {
                    col.isTrigger = false;
                }

                nowgun.transform.SetParent(null);
                Destroy(nowgun.gameObject);
                /*// 解除父子关系
                nowgun.transform.SetParent(null);
                nowgun = null;*/
            }
            currentHealth -= 1;
            healthBar.SetHealth(currentHealth);
            transform.position = new Vector2(UnityEngine.Random.Range(-6.6f, 5.8f), 3.5f);
            Rigidbody2D playerRb = GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero; // 将速度设为零向量，彻底归零.若有角速度（旋转），也可一并归零
                playerRb.angularVelocity = 0f;
            }
            GameObject platform = Instantiate(platformPre, platformTrans.position, Quaternion.identity);
            Destroy(platform, 3f);
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
            游戏主体.SetActive(false);
            结算界面.SetActive(true);
            胜利玩家.GetComponent<胜利玩家>().ShowWinner();
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
}
