using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyLibrary;
using static MyLibrary.Static;
using System.Linq;

namespace Retrem {


    public class PlayerAttackPos : Base {
        
        List<GameObject> enemiesObj = new List<GameObject>();

        void OnTriggerEnter(Collider other) {
            if (other.IsTag("Enemy")) {
                if (!enemiesObj.Contains(other.gameObject)) enemiesObj.Add(other.gameObject);
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.IsTag("Enemy")) {
                if (enemiesObj.Contains(other.gameObject)) enemiesObj.Remove(other.gameObject);
            }
        }

        /// <summary> プレイヤーから呼んでもらう </summary>
        public void Attack(int atk) {
            enemiesObj = enemiesObj.Where (obj => obj != null).ToList();
            if (0 < enemiesObj.Count) {
                enemiesObj.For(i => {
                    Enemy enemy = enemiesObj[i].GetComponent<Enemy>();
                    enemy.chara.Damage(atk);
                    enemy.IsHit();
                    Log($"{data.Player.Name} が {enemiesObj[i].name} に {(atk <= enemy.chara.DEF ? 0 : atk - enemy.chara.DEF)} ダメージ与えた！", Color.green);
                });
                sound.PlaySE("hi");
            }else{
                sound.PlaySE("fa");
            }
        }

    }

}