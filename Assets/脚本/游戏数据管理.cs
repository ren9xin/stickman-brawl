using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 游戏数据管理 : MonoBehaviour  
{
    
    public static 游戏数据管理 Instance;// 单例实例：全局唯一，任何脚本都能访问
    
    public string WinnerId;// 存储胜利者的标识（比如角色1/角色2，或角色名称）
    public GameObject 胜利玩家;
    public GameObject 游戏主体;
    public GameObject 结算界面;
    // Start is called before the first frame update
    void Awake()
    {
        // 单例初始化：确保场景中只有一个该脚本
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景也不销毁（如果游戏/结算在不同场景）
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    void Update()
    {
        GameOver();
    }
    public void GameOver()
    {
        if (游戏状态.Instance.currentGameState == GameState.GameOver)
        {
            if (游戏主体 != null)
            {
                游戏主体.SetActive(false);
            }
            if (结算界面 != null)
            {
                结算界面.SetActive(true);
            }
            if (胜利玩家 != null)
            {
                胜利玩家.GetComponent<胜利玩家>().ShowWinner();
            }
        }
    }
}
