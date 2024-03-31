using System;
using System.Collections.Generic;
using UnityEngine;

using MyLibrary;
using static MyLibrary.Static;
using TMPro;

namespace Retrem {

    // ===== GameData =====
    [CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObject/GameData (Retrem)", order = 1)]
    public class GameData : ScriptableObject {

        // 今フェード中か？
        public bool IsFade { get; set; } = false;

        // score
        [SerializeField, Min(0)] int maxScore = 0;
        public int        Score     { get { return maxScore;    } set { maxScore    = ClampMin(value, 0); } }

        // ガチャ石の数
        public intR gachaCost;

        // キャラクター (Save)
        [SerializeField]
        BattleChara player = new BattleChara();
        public BattleChara Player    { get { return player;      } set { player      = value;              } }

        // アイテム
        public List<Item> items = new List<Item>();

        // 攻略時間、倒した敵の数
        [Min(0f)]
        public float questTime = 180f, questTimeCountor;

        // アイテム使用数 と 倒した敵の数
        [HideInInspector]
        public int useItemCount = 0, killEnemyCountor = 0;

        /// <summary> 初期化関数 </summary>
        public void Initialize() {
            maxScore        = 0;
            gachaCost.value = 300;
            player.Name     = "サフィー";
            player.HP_MAX   = 10;
            player.SP_MAX   = 10;
            player.HP_NOW   = 10;
            player.SP_NOW   = 10;
            player.ATK      = 5;
            player.DEF      = 5;
            items.For(i => { items[i].quantity = 0; });
        }

    }

    // ===== Bace =====
    public class Base : MyLibrary.Base {

        protected GameData data;

        // === 起動時の最初に1回実行 ===
        [Obsolete]
        protected override void Awake() {
            base.Awake();
            data = admin.data as GameData;
        }

    }

    /// <summary> キャラクターベース </summary>
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Animator))]
    public class CharacterBase : Base {

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public Animator  ani;

        // キャラクター情報
        public BattleChara chara;

        [Obsolete]
        protected override void Awake() {
            base.Awake();
            if (this.IsTag("Untagged")) Log("キャラタグあってる？");
            rb  = GetComponent<Rigidbody>();
            ani = GetComponent<Animator>();
        }

    }

}