using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;
using Utils;
using RelicSystem;
using GamePlayer;
using OutGame;

namespace GameFramework
{
    /// <summary>
    /// 씬 전환 및 캐릭터 전달을 관리하는 싱글턴 매니저
    /// </summary>
    public class PlayScoreLogger
    {
        private int killScore;
        private int moneyScore;
        private int totalScore => killScore + moneyScore;

        public PlayScoreLogger()
        {
            killScore = 0;
            moneyScore = 0;
        }

        public void AddKillScore(int score)
        {
            killScore += score;
        }

        public void AddMoneyScore(int score)
        {
            moneyScore += score;
        }
        
        public int GetKillScore()
        {
            return killScore;
        }
        
        public int GetMoneyScore()
        {
            return moneyScore;
        }
        
        public int GetTotalScore()
        {
            return totalScore;
        }
    }
}
