using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 玩家管理器
/// </summary>
public class PlayerManager : Singleton<PlayerManager>
{
    public List<Player> players { get; set; }// 服务器上的所有玩家数据列表

    /// <summary>
    /// 服务器上的本地玩家数据
    /// </summary>
    public Player LocalPlayer { get; set; }
    private PlayerManager() { }

    public Player GetLocalPlayer()
    {
        return LocalPlayer;
    }
    public void Init(UserBasicInfo testInfo, ArmyDetailInfo armyDetailInfo)
    {
        players = new List<Player>();
        // 初始化玩家数据和敌人数据到MiddleContainers
        LocalPlayer = new Player();// 这里需要从服务器拿数据
        LocalPlayer.Init(armyDetailInfo,testInfo);
    }

    public void InitRemotePlayers()
    {
        Debug.Log("获取服务器上其他玩家");
        Player _player = new Player();
        _player.Init(null);
        players.Add(_player);
    }
    /// <summary>
    /// 获取服务器上的玩家数据
    /// </summary>
    /// <param name="num">获取num个符合匹配规则的玩家</param>
    /// <returns></returns>
    public Player GetRemotePlayerData(int num)
    {
        // 从服务器获取所有玩家数据
        //players.Add(player);
        if (players.Count > 0)
        {
            return players.GetRandomItem();
        }
        return null;
    }
    public void Init()
    {
        players = new List<Player>();
        Player player = new Player();// 这里需要从服务器拿数据
        player.Init(null);
        players.Add(player);
    }
}