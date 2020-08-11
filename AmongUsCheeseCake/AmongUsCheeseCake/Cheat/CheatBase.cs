﻿
using AmongUsCheeseCake.Game;
using Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsCheeseCake.Cheat
{
    public class CheatBase
    {
        static string PlayerControllPattern = "48 52 06 11 ?? ?? ?? ??";
        static string GameDataPattern = "A8 A4 B0 06 ?? ?? ?? ??";

        public static Mem Memory = new Mem();


        /// <summary>
        /// 플레이어 위치정보
        /// </summary>
        public List<CachedPlayerControllInfo> playersPositions = new List<CachedPlayerControllInfo>();  


        private string m_cached_gameDataOffset = null;


        List<S_PlayerControll> SearchPlayerInfoList()
        {
            List<S_PlayerControll> list = new List<S_PlayerControll>();
            var result = Memory.AoBScan(PlayerControllPattern, true, true);
            result.Wait();
            var results =    result.Result; 
            foreach (var x in results)
            {
                var bytes = Memory.ReadBytes(x.GetAddress(), S_PlayerControll.SizeOf());
                var playerControll = S_PlayerControll.FromBytes(bytes);
                list.Add(playerControll); 
            }
            return list;
        }


       
        /// <summary>
        /// 게임데이터 오프셋을 새로고침함
        /// </summary>
        public void RefreshGameDataOffset()
        {
            string offset = null;
            var result = Memory.AoBScan(GameDataPattern, true, true);
            result.Wait();
            var results = result.Result;

            foreach (var x in results)
            {
                var bytes = Memory.ReadBytes(x.GetAddress(), S_GameData.SizeOf());
                var gameData = S_GameData.FromBytes(bytes);
                // OWNER ID가 -2이고, NetId가 4294967295가 아닌 객체는 실제 인스턴스이다.
                // 4294967295(uint의 max값)은, 이미 인스턴스가 해제된 가비지값을 가리킴
                if (gameData.OwnerId == -2 && gameData.NetId != 4294967295)
                    offset = x.GetAddress();
            }
            this.m_cached_gameDataOffset = offset;
        }

        public void UpdatePlayerPositions()
        {
            var players = SearchPlayerInfoList(); 
        }
        public S_GameData ReadGameData()
        {
            try
            {
                return S_GameData.FromBytes(Memory.ReadBytes(m_cached_gameDataOffset, S_GameData.SizeOf()));
            }
            catch
            {
                return null;
            }
        }
    }
}