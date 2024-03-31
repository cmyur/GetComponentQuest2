using UnityEngine;
using UnityEngine.UI;

using MyLibrary;
using static MyLibrary.Static;
using System.Collections;
using Unity.VisualScripting;
using static SuperCharacterController;

namespace Retrem {

    public class Player : CharacterBase {

        public Button attack;
               bool   atkFlag = false;

        public PlayerAttackPos attackPos;

        [Min(0f)]
        public float attackInterval = 1f;
               bool  attackWait     = false;

        // 地面にいるか？の判定
        bool isGround = false;

        // === Active になる度に実行 ===
        void OnEnable() {
            // スクタブを読取
            chara = data.Player;
        }

        // === 起動時に始めの1回実行 ===
        void Start() {
            attack.ClickAction(() => { atkFlag = true; });
        }

        // === 繰り返し実行(fps依存) ===
        void Update() {
            if (atkFlag) {
                atkFlag = false;
                if (!attackWait) {
                    attackWait = true;
                    attackPos.Attack(chara.ATK);
                    ani.Anime("IsAttack");
                    StartCoroutine(AttackWait());
                }
            }
            // ジャンプ
            else if (IsJump() && isGround) {
                isGround = false;
                rb.ForceJump(chara.JumpPower);
                ani.Anime("IsJump");
            }
            // 横移動
            else if (0f < GetAxisPower()) {
                rb.ConstRotation(null, false);
                gameObject.InputMove(chara.MoveSpeed, chara.RotateSpeed, chara.Dash);
                ani.Anime("Move Speed", GetAxisPower());
            }
            // 動いてないなら
            else {
                rb.ConstRotation(null, true);
                ani.Anime("Move Speed", GetAxisPower());
            }
            
            // スクタブ更新
            data.Player = chara;
        }

        IEnumerator AttackWait() {

            yield return new WaitForSeconds(attackInterval);

            attackWait = false;

        }

        void OnCollisionStay(Collision collision) {
            if (collision.IsTag("Ground")) isGround = true;
        }

    } 
    
}