using UnityEngine;

// 定义游戏状态枚举（明确区分不同流程）
public enum GameState
{
    StartUI, // 开始界面（默认状态，不执行角色生成）
    GamePlaying, // 游戏运行中（进入游戏主体，执行角色生成）
    GameOver // 游戏结束（可选，用于后续扩展）
}

/// <summary>
/// 全局游戏状态管理器（单例模式，跨脚本共享状态）
/// </summary>
public class 游戏状态 : MonoBehaviour
{
    // 单例实例（全局唯一，其他脚本可直接访问）
    public static 游戏状态 Instance;

    // 当前游戏状态（默认是开始界面）
    public GameState currentGameState = GameState.StartUI;

    void Awake()
    {
        // 单例初始化：确保全局只有一个实例
        if (Instance == null)
        {
            Instance = this;
            // 跨场景保留（若分场景可保留，单场景也可删除此行）
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 若已有实例，销毁当前重复对象
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 公共方法：切换游戏状态（供外部脚本调用，如开始界面跳转）
    /// </summary>
    /// <param name="newState">要切换的新状态</param>
    public void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
    }


}
