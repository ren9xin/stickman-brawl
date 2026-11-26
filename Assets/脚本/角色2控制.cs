using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class 角色2控制 : MonoBehaviour
{
    public GameObject platformPre;//复活平台
    Rigidbody2D body;
    public float speed;
    public bool IsGround;
    public float jumpforce;
    private Transform gunTrans;//手枪生成点
    private Transform platformTrans;//复活平台生成点
    枪控制 nowgun;//当前手枪
    枪控制 gun;
    float bulletNumber;//子弹数
    private Vector2 dir;
    private float moveStartTime;// 运动开始时间
    private bool isMoving = false;     // 是否正在运动
    private float moveSpeed;//击飞速度
    bool isKnocked;//击飞延迟判断
    public float knockbackDuration = 0.3f; // 击飞硬直时间（惯性保留时间）
    private float knockbackStartTime; // 击飞开始时间
    public float jumpcount;//空中可以跳跃次数
    public int maxHealth;//玩家最大生命值
    public int currentHealth;//玩家当前生命值
    public 血条 healthBar;//玩家血条
    
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
        bulletNumber = 7;
        moveSpeed = 400;
        // 将角色刚体设为不受子物体物理影响（防止枪的碰撞体推挤角色）
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Update is called once per frame
    void Update()
    {
        outOfBound();
        // 优先处理击飞逻辑：每帧检查击飞状态是否结束
        CheckKnockbackEnd();
        if (!isKnocked)
        {
            right_left();
            jump();
            getGun();
            DropGun();
        }

        beAttacked();
    }

    void right_left()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            body.velocity = new Vector2(-speed, body.velocity.y);
            transform.localScale = new Vector3(-0.38f, 0.38f, 0.5f);


        }
        else if (Input.GetKey(KeyCode.RightArrow))
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

    void jump()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && jumpcount > 0 && IsGround)//离地时，防止和碰撞框一起被判定两次
        {
            body.velocity = new Vector2(body.velocity.x, jumpforce);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && jumpcount > 0)
        {
            jumpcount -= 1;
            body.velocity = new Vector2(body.velocity.x, jumpforce);
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
            isKnocked = true;
            knockbackStartTime = Time.time;
            isMoving = true;
            dir = new Vector2(Mathf.Sign(collision.collider.transform.localScale.x) * 0.5f, 0);
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "ground")
        {
            IsGround = false;
            jumpcount -= 1;
        }
    }

    //抓手枪
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "gun")
        {
            //保存箱子
            gun = collision.GetComponent<枪控制>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "gun")
        {

            gun = null;
        }
    }

    void getGun()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            //拿枪
            if (gunTrans.childCount == 0 && gun != null && gun.transform.parent==null)//持枪时无法被抢
            {
                if(Mathf.Sign(gun.transform.localScale.x)!=Mathf.Sign(-transform.localScale.x))//改变手枪朝向
                {
                    gun.transform.localScale=new Vector3(-gun.transform.localScale.x, gun.transform.localScale.y, gun.transform.localScale.z);
                }
                bulletNumber = 7;
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
                nowgun = gun;

            }
            else if (gunTrans.childCount > 0)//射击
            {
                nowgun.fire();
                bulletNumber--;
            }
        }
    }

    void DropGun()//子弹用完枪掉下
    {
        if (bulletNumber <= 0 && nowgun != null)
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

            // 解除父子关系
            nowgun.transform.SetParent(null);
            nowgun = null;
        }
    }

    public void beAttacked()//受击
    {

        if (isMoving)
        {
            Move();
            isMoving = false;
        }
    }

    private void Move()//被击飞
    {
        if (dir != Vector2.zero)
        {
            body.AddForce((Vector2.up + dir) * moveSpeed);

            dir = Vector2.zero;
        }

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
        if(transform.position.x < -8.8f|| transform.position.x > 8.8f || transform.position.y < -5.6f || transform.position.y > 5.6f)
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

                // 解除父子关系
                nowgun.transform.SetParent(null);
                nowgun = null;
            }
            currentHealth -= 1;
            healthBar.SetHealth(currentHealth);
            transform.position = new Vector2(UnityEngine.Random.Range(-6.6f,5.8f),3.5f);
            GameObject platform = Instantiate(platformPre, platformTrans.position, Quaternion.identity);
            Destroy(platform, 3f);
        }
        
    }

}
