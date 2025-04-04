﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "Battle", menuName = "Ulko/Battle/Battle Asset", order = 1)]
    public class BattleAsset : ScriptableObject
    {
        [Serializable]
        public class Enemy
        {
            public int positionIndex;
            public Characters.EnemyAsset asset;
        }

        public string sceneAddress;
        public List<Enemy> enemies = new();

        public int EnemyTypeCount(string enemyId)
        {
            int count = 0;
            foreach(var enemy in enemies)
            {
                if (enemy.asset.id == enemyId)
                {
                    count++;
                }
            }

            return count;
        }

        public string EnemySuffix(int enemyIndex)
        {
            if (EnemyTypeCount(enemies[enemyIndex].asset.id) > 1)
            {
                int count = 1;
                for (int i = 0; i < enemyIndex; ++i)
                {
                    if (enemies[i].asset.id == enemies[enemyIndex].asset.id)
                    {
                        count++;
                    }
                }

                return ((char)(count + 64)).ToString();
            }

            return "";
        }
    }
}
