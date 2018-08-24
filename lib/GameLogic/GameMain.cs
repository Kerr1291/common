using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public class GameMain : GameSingleton<GameMain>
    {
        public GameObject playerPrefab;
        GameObject player;

        public MapRenderer gameView;

        [EditScriptable]
        public ProcGenMap startingMap;        
        ProcGenMap currentMap;

        // Use this for initialization
        IEnumerator Start()
        {
            yield return startingMap.Generate();

            currentMap = startingMap;

            gameView.mapData = startingMap;

            player = Instantiate(playerPrefab);
            player.SetActive(true);
        }
    }
}