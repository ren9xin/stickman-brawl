using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;

public class 法杖控制 : MonoBehaviour
{
    public GameObject BulletPre;
    public Animator MagicCircle;//法阵
    //public static 枪控制 Instance;
    public float speed;//子弹速度
    public float horizontal;
    public KeyCode triggerKey;    // 指定触发动画的按键
    // 玩家标签（需在Inspector给玩家物体设置相同标签，比如"Player"）
    private bool isAttachedToPlayer;// 标记是否已找到玩家（避免重复获取组件）
    private float fixedWorldY;
    private float fixedWorldZ;
    private Vector3 fixedWorldPos;// 缓存子物体的世界坐标（固定点）
    private Quaternion fixedWorldRot;// 缓存子物体的世界旋转（可选）
    public GameObject targetChild; // 需控制的子物体（法阵）
    public Transform followTarget; // 跟随的父物体（玩家/法杖）
    private Transform playerTransform; // 缓存玩家Transform，避免重复查找
    private bool isGunReady = false; // 枪是否已拾取并就绪
    private float lastPlayerScaleX; // 记录玩家上一帧的朝向，避免重复计算
    public bool IsFinished=false;//咏唱结束
    public 角色控制 player;//使用的玩家的角色控制组件
    private float xOffset;// 子物体与父物体的X轴偏移（避免重叠）
    public string StaffClass;//法杖种类
    // Start is called before the first frame update
    void Start()
    {
        if(StaffClass=="bl")
        {
            GameObject circleObj = GameObject.Find("爆裂法阵");
            if (circleObj != null)
            {
                // 自动获取法阵的Animator组件
                MagicCircle= circleObj.GetComponent<Animator>();
                targetChild= circleObj;
            }
        }
        
        //Instance = this;
        followTarget = transform;
        speed = 40;
        fixedWorldRot = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        isAttachedToPlayer = gameObject.GetComponent<枪控制>().isAttachedToPlayer;
        triggerKey = gameObject.GetComponent<枪控制>().triggerKey;
        if (isAttachedToPlayer)
        {
            FixChildPosition();
        }
        StaffFire();
        

    }


    // 初始化玩家和光柱参数（仅拾取时执行一次）
    public void InitPlayerAndColumn()
    {
        // 找到玩家（父物体）：递归向上找带Player标签的物体
        playerTransform = FindPlayerParent(transform);
        player=playerTransform.GetComponent<角色控制>();
        if (playerTransform == null)
        {
            return;
        }

        // 初始化光柱固定参数：以拾取瞬间的光柱Y/Z为固定值（不硬编码）
        if (targetChild != null)
        {
            fixedWorldY = targetChild.transform.position.y;
            fixedWorldZ = targetChild.transform.position.z;
            // 计算光柱与玩家的X轴偏移（拾取瞬间的相对位置）
            xOffset = 7; 
            // 记录初始玩家朝向
            lastPlayerScaleX = Mathf.Sign(playerTransform.localScale.x);
        }
    }

    // 递归查找带Player标签的父物体（适配玩家子物体挂载的情况）
    private Transform FindPlayerParent(Transform currentTrans)
    {
        if (currentTrans == null) return null;
        // 检查当前物体是否是玩家（标签为Player）
        if (currentTrans.CompareTag("Player"))
        {
            return currentTrans;
        }
        // 递归向上找父物体
        return FindPlayerParent(currentTrans.parent);
    }

    void StaffFire()
    {
        CastMagic();
    }

    void CastMagic()//吟唱
    {
        if (!IsFinished)
        {
            if (isGunReady && isAttachedToPlayer && Input.GetKey(triggerKey) && player.IsGround == true)//在地面上才能施法
            {
                MagicCircle.SetBool("IsPlaying", true);
                DisablePlayerMovement(false);//吟唱时角色不能动
            }
            else
            {
                MagicCircle.SetBool("IsPlaying", false);
                DisablePlayerMovement(true);
            }   
        }
        
    }
    // 父物体位置变化时触发（比如捡起法杖时）
    public void FixChildPosition()
    {

        if (targetChild == null || followTarget == null) return;
        // 1. 获取玩家当前朝向
        float currentPlayerScaleX = Mathf.Sign(playerTransform.localScale.x);
        // 2. 计算镜像后的X偏移（轴对称核心）
        float mirroredXOffset = xOffset * currentPlayerScaleX;
        // 核心：X轴跟随父物体（叠加偏移），Y、Z轴固定
        Vector3 newPos = new Vector3(mirroredXOffset + playerTransform.position.x, fixedWorldY,fixedWorldZ);
        targetChild.transform.position = newPos;
    }

    public void SetGunReady()
    {
        // 延迟激活，避免帧同步误触（比如延迟0.1秒）
        Invoke("SetReadyDelayed", 0.2f);
    }
    private void SetReadyDelayed()
    {
        isGunReady = true;
    }

    // 同步法阵位置（轴对称）和旋转

    public void DisablePlayerMovement(bool isDisable)
    {
        if(player!=null)
        {
            player.ismove = isDisable;
            player.isjump = isDisable;
        }
    }
}
