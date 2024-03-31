using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using MyLibrary;
using static MyLibrary.Static;
using System.Linq;

namespace Retrem {

    public class Enemy : CharacterBase {
        
        // キャラクターのステータスを表示する
        Canvas          canvas;
        TextMeshProUGUI status;
        Image           hp_Fill;

        // 攻撃間隔
        [SerializeField, Min(0f)]
        float           attackInterval = 5f;
        bool            attackWait     = false;

        // 倒されたか？、消滅までの時間
        bool            killEnemy = false;
        [SerializeField, Min(5f)]
        float           destroyTimer = 5f;

        // プレイヤー情報
        List<GameObject> players = new List<GameObject>();

        // === Active になる度に実行 ===
        void OnEnable() => IsHit();

        // === 起動時に始めの1回実行 ===
        void Start() {
            canvas  = this.GetChild<Canvas>();
            status  = canvas.GetChild<TextMeshProUGUI>();
            if (status != null) status.text = chara.Name;
            hp_Fill = canvas.GetChild<Image>().GetChild<Image>();
        }

        // === 繰り返しの末尾で実行  ===
        void LateUpdate() {
            if (canvas != null) {
                Camera _camera = Camera.main;
                canvas.gameObject.LookAt(_camera);
                canvas.RotateY(180f, false);
            }
            if (chara.HP_NOW <= 0) {
                if (!killEnemy) {
                    killEnemy = true;
                    data.killEnemyCountor++;
                }
                if (0 < destroyTimer) {
                    destroyTimer -= Time.deltaTime;
                }else{
                    Destroy(gameObject);
                }
            }
        }

        void OnCollisionEnter(Collision other) {
            if (other.IsTag("Player")) {
                if (!players.Contains(other.gameObject)) players.Add(other.gameObject);
            }
        }

        void OnTriggerStay(Collider other) {
            if (other.IsTag("Player")) {
                gameObject.LookAt(other.gameObject);
                if (!attackWait) {
                    attackWait = true;
                    ani.Anime(RandomFlag ? "IsAttack1" : "IsAttack2");
                    players = players.Where(obj => obj != null).ToList();
                    players.For(i => {
                        Player player = players[i].GetComponent<Player>();
                        player.chara.Damage(chara.ATK);
                        Log($"{chara.Name} が {player.chara.Name} に {(chara.ATK <= player.chara.DEF ? 0 : chara.ATK - player.chara.DEF)} ダメージ与えた！", Color.red);
                    });
                    StartCoroutine(AttackWait());
                }
            }
        }

        void OnCollisionExit(Collision other) {
            if (other.IsTag("Player")) {
                if (players.Contains(other.gameObject)) players.Remove(other.gameObject);
            }
        }

        IEnumerator AttackWait() {
            yield return new WaitForSeconds(attackInterval);
            attackWait = false;
        }

        public void IsHit() {
            if (hp_Fill != null) hp_Fill.Fill(chara.HP_PER);
            ani.Anime("HP", chara.HP_NOW);
            ani.Anime("IsHit");
        }

    } 
    
}