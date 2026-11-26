using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class 咏唱结束事件: MonoBehaviour
{
    public Animator magicCircleAnim; // 法阵的Animator
    public GameObject BulletPre; // 光柱
    public 法杖控制 staff;
    public float speed;//激光下降速度
    public Transform FirePoint;//子弹生成点
    public KeyCode attackKey;
    public KeyCode upKey;//上移键
    public KeyCode downKey;//下移键
    public int bulletNumber;
    public float horizontal;//朝向
    float angle;
    private System.Action RestoreMovementCallback;// 定义委托：用于恢复玩家移动
    private Vector3 bulletOriginalScale;//缓存BulletPre的原始缩放
    public float maxangle;
    public float minangle;
    public float scaleDuration = 0.1f;
    private void Awake()// 脚本初始化时，缓存预制体原始缩放
    {
        angle = 0;
        maxangle = 40;
        minangle = -40;
        // 空引用保护：避免BulletPre未赋值导致报错
        if (BulletPre != null)
        {
            bulletOriginalScale = BulletPre.transform.localScale;
            BulletPre.transform.localRotation = Quaternion.identity;
        }
    }

    public void OnMagicCircleEnd()//爆裂魔法施法
    {
        if (magicCircleAnim != null &&BulletPre!=null)
        {   
            speed=staff.speed;
            if (staff!=null)
            {
                staff.DisablePlayerMovement(true);
                staff.IsFinished = true;
                
            }
            GameObject bullet = Instantiate(BulletPre,new Vector3(transform.position.x, 20.5f, 0), Quaternion.identity);
            angle=bullet.transform.rotation.eulerAngles.z;
            magicCircleAnim.SetBool("IsPlaying", false); // 关闭吟唱动画
            // 启动协程手动控制下落（完全脱离物理碰撞）
            StartCoroutine(ManualBulletFall(bullet));
            Destroy(bullet,0.5f);
            staff.GetComponent<枪控制>().bulletNumber--;
        }
        
    }

    // 协程：手动控制光柱下落，无视所有碰撞
    private IEnumerator ManualBulletFall(GameObject bullet)
    {
        if (bullet == null) yield break;

        while (bullet != null)
        {
            // 手动向下移动，Time.deltaTime保证帧率无关
            bullet.transform.Translate(Vector2.down * speed * Time.deltaTime);
            yield return null; // 每帧执行一次
        }
    }

    public void OnWaterMagicCircleEnd()//水龙卷施法
    {
        if (staff != null)
        {
            attackKey=staff.triggerKey;
            枪控制 s=staff.GetComponent<枪控制>();
            horizontal=s.horizontal;
            if (s!=null)
            {
                bulletNumber=s.bulletNumber;
                if(horizontal==-1)
                {
                    upKey = s.downKey;
                    downKey = s.upKey;
                }
                else
                {
                    upKey = s.upKey;
                    downKey=s.downKey;
                }
                    
                
            }
            // 关键：缓存恢复移动的委托（在staff未销毁时）
            RestoreMovementCallback = () =>
            {
                staff.DisablePlayerMovement(true);
                staff.IsFinished = false;
            };

        }
        
        if (magicCircleAnim != null && BulletPre != null)
        {
            speed = staff.speed;
            GameObject bullet = Instantiate(BulletPre, FirePoint.position, Quaternion.identity);
            if (bullet != null)
            {
                StartCoroutine(ScaleUpFromZero(bullet, horizontal));
                水柱 b = bullet.GetComponent<水柱>();
                b.staff = staff;
                角色控制 p = staff.player;
                b.player = p;

                // 启动协程： 持续检测攻击键状态
                StartCoroutine(ManualBulletFallWithAttackCheck(bullet));
            }
            // 空引用保护：避免staff或枪控制为空导致报错
            if (staff != null && staff.GetComponent<枪控制>() != null)
            {
                staff.GetComponent<枪控制>().bulletNumber--;
            }
        }
    }

    // 协程：持续检测攻击键 + 每秒bulletNumber-1
    private IEnumerator ManualBulletFallWithAttackCheck(GameObject bullet)
    {
        if (bullet == null) yield break;

        // 时间累计变量：用于统计是否达到1秒
        float timeAccumulator = 0f;
        angle = 0;

        // 循环条件：子弹存在 + bulletNumber > 0
        while (bullet != null && bulletNumber > 0 && Input.GetKey(attackKey))
        {
            // 1. 检测攻击键：按住继续，松开销毁子弹并终止协程
            if (Input.GetKey(attackKey))
            {
                // 执行子弹旋转逻辑
                rotation(bullet);

                // 2. 时间累计：累加每帧的时间差
                timeAccumulator += Time.deltaTime;

                // 3. 判断是否达到1秒：达到则执行bulletNumber-1，并重置时间累计器
                if (timeAccumulator >= 1f)
                {
                    bulletNumber--; // 每秒减1
                    timeAccumulator = 0f; // 重置时间，为下一次1秒计数做准备

                    // 同步更新枪控制中的bulletNumber
                    if (staff != null && staff.GetComponent<枪控制>() != null)
                    {
                        staff.GetComponent<枪控制>().bulletNumber = bulletNumber;
                    }
                }
            }
            else// 松开攻击键：销毁子弹+恢复状态+终止协程
            {
                angle = 0;
                Destroy(bullet);
                if (staff != null)
                {
                    staff.DisablePlayerMovement(true);
                    staff.IsFinished = false;
                }
                yield break; // 终止协程，结束所有逻辑
            }

            yield return null; // 每帧执行一次，保证帧率无关（关键：不可缺失）
        }
        // 当bulletNumber减到0时，自动销毁子弹并恢复状态
        if (bullet != null)
        {
            Destroy(bullet);
        }
        // 子弹耗尽后调用委托恢复移动
        RestoreMovementCallback?.Invoke();
    }

    /// 协程：0.1秒内从0缩放至原始大小
    IEnumerator ScaleUpFromZero(GameObject bullet,float horizontal)
    {
        float elapsedTime = 0f; // 已流逝的时间
        Vector3 startScale = Vector3.zero; // 起始缩放
        Vector3 targetScale = new Vector3(horizontal * bulletOriginalScale.x, bulletOriginalScale.y, bulletOriginalScale.z);
        // 当已流逝时间 < 目标时长时，持续执行缩放
        while (elapsedTime < scaleDuration &&bullet!=null)
        {
            // 计算缩放进度
            float progress = elapsedTime / scaleDuration;
            if (bullet != null)
            {
                // 线性插值计算当前缩放值，实现平滑过渡
                bullet.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            }
            
            // 累加帧间隔时间，确保进度计算准确
            elapsedTime += Time.deltaTime;
            // 暂停协程，下一帧继续执行
            yield return null;
        }

        // 确保最终缩放精准等于原始大小（避免因帧率误差导致偏差）
        if (bullet != null)
        {
            bullet.transform.localScale = targetScale;
        }
    }

    void rotation(GameObject bullet)
    {
        if (Input.GetKey(upKey) && angle<=maxangle)
        {
            angle += 0.05f;
        }
        else if (Input.GetKey(downKey) && angle>=minangle)
        {
            angle -= 0.05f;
        }
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        //旋转
    }
}
