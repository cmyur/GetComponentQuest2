using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using MyLibrary;
using static MyLibrary.Static;

using Random = UnityEngine.Random;

namespace Retrem {

    public class Game : Base {

        Camera mainCamera;
        public GameObject home, game;
        [Min(0f)]
        public float loadTime;
         
        public Action seAction;

        // Player参照
        public Player player;

        // 太陽光、一日の時間()
        public Light sunLight;
        [Min(1f)]
        public float deyTime = 20f;

        

        // Menuパネル(左)、Menuパネル(右)
        public RectTransform menuLeftPanel, menuRightPanel;
               TransValue    menuInLeft, menuOutLeft, menuInRight, menuOutRight;

        // Menuパネル開閉時間、管理コルーチン
        public Button        menuInButton, menuOutButton;
        [Min(0f)]
        public float         menuMoveSpeed = 0.5f;
               Coroutine     menuMove;

        // 表示情報
        [SerializeField] Image           hp_Fill, sp_Fill, mapPin;
        [SerializeField] Slider          mapHeight;
        [SerializeField] TextMeshProUGUI playerName, hp_gauge, sp_gauge, hp, sp, atk, def;
        [SerializeField] ItemList        itemlist;

        // 攻略時間、倒した敵の数
        [SerializeField] TextMeshProUGUI questTimeText, killEnemy;

        // ホームに戻るボタン
        [SerializeField] Button          gotoHome;

        // リザルトパネル
        [SerializeField] Image           result;
        [SerializeField] TextMeshProUGUI resultText;
        [SerializeField] Button          clearButton;
                         Coroutine       isResult;

        // ぷれふぁぶ
        [SerializeField] GameObject       parent;
        [SerializeField] List<GameObject> prehab          = new List<GameObject>();
        [SerializeField, Min(0.1f)] float generatInterval = 10f;
        [SerializeField] Vector3          posA, posB;
                         Coroutine        generatTask     = null;

        // === Active になる度に実行 ===
        void OnEnable() {
            if (mainCamera == null) mainCamera = Camera.main;
            mainCamera.orthographic = false;
            sound.PlayBGM("ga");
            playerName.text         = $"{data.Player.Name}";
            player.SetWorldPos(new Vector3(0f, 6f, -20f));
            data.questTimeCountor   = data.questTime;
            data.useItemCount       = 0;
            data.killEnemyCountor   = 0;
            result.Fill(0f);
            resultText.text         = "";
            result.Active(false);
            clearButton.Set(false);
            isResult                = null;
            data.items.For(i => { data.items[i].UseSync(this); });
            itemlist.DisplayUpdate();
            parent.ChildDestroy();
            generatTask = StartCoroutine(Generator());
        }

        // === 起動時に始めの1回実行 ===
        void Start() {
            seAction = () => { sound.PlaySE("pu"); };
            // Transform記憶
            menuInLeft   = menuLeftPanel;
            menuOutLeft  = menuLeftPanel;
            menuOutLeft.pos_x *= -1;
            menuOutLeft.angle_y -= 140f;
            menuInRight  = menuRightPanel;
            menuOutRight = menuRightPanel;
            menuOutRight.pos_x *= -1;
            menuOutRight.angle_y += 140f;
            // ボタンに処理を割り当て
            NewButtonAction();
            // アイテムリスト初期化
            itemlist.Initialize(this, data.items, () => { seAction?.Invoke(); data.useItemCount++; });
        }

        // === 繰り返し実行(fps依存) ===
        void Update() {
            sunLight.GetParent().RotateX(deyTime != 0f ? 360 / deyTime : 0f);
            data.questTimeCountor   = 0f < data.questTimeCountor ? data.questTimeCountor -= Time.deltaTime : 0f;
            questTimeText.text = data.questTimeCountor.FormatTime();
            killEnemy.    text = $"倒した敵の数：{data.killEnemyCountor}";
            GaugeFill();
            hp. text = $"HP : {(player.chara.HP_NOW < 10000 ? ($"{RepeatText(" ", 4 - player.chara.HP_NOW.Length())}{player.chara.HP_NOW}") : ("9999+"))}";
            sp. text = $"SP : {(player.chara.SP_NOW < 10000 ? ($"{RepeatText(" ", 4 - player.chara.SP_NOW.Length())}{player.chara.SP_NOW}") : ("9999+"))}";
            atk.text = $"ATK: {(player.chara.ATK    < 10000 ? ($"{RepeatText(" ", 4 - player.chara.ATK.   Length())}{player.chara.ATK   }") : ("9999+"))}";
            def.text = $"DEF: {(player.chara.DEF    < 10000 ? ($"{RepeatText(" ", 4 - player.chara.DEF.   Length())}{player.chara.DEF   }") : ("9999+"))}";
            mapPin.WorldAngleZ(-player.GetWorldAngle().y);
            mapHeight.value = Clamp(player.GetWorldPos().y / 4, 0f, 1f);
            if (player.GetWorldPos().y < -3f) player.SetWorldPos(new Vector3(0f, 10f, -20f));
            if (isResult == null && data.questTimeCountor <= 0f) isResult = StartCoroutine(Result());
        }

        /// <summary> ボタンに関数の割り当て </summary>
        void NewButtonAction() {
            menuInButton. ClickAction(() => { seAction?.Invoke(); MenuMove( true); });
            menuOutButton.ClickAction(() => { MenuMove(false); });
            gotoHome.     ClickAction(() => { seAction?.Invoke(); admin.FadeAction(loadTime, () => { home.Active(true); game.Active(false); }); });
            clearButton.  ClickAction(() => { if (isResult != null) StopCoroutine(isResult); data.gachaCost.value += 100; seAction?.Invoke(); admin.FadeAction(loadTime, () => { home.Active(true); game.Active(false); }); });
        }

        /// <summary> Menuを出したり引っ込めたり </summary>
        void        MenuMove   (bool flag) { if (menuMove == null) menuMove = StartCoroutine(MenuMoveCor(flag)); }
        IEnumerator MenuMoveCor(bool flag) {
            menuInButton.Set(!flag);
            menuOutButton.Active(flag);
            yield return TimeLerp(
                value => { menuLeftPanel.ToLerp(menuInLeft, menuOutLeft, value); menuRightPanel.ToLerp(menuInRight, menuOutRight, value); },
                menuMoveSpeed,
                flag ? 1f : 0f,
                flag ? 0f : 1f
            ); menuMove = null;
        }

        /// <summary> ゲージ系のリアタイ反映 </summary>
        void GaugeFill() {
            hp_gauge.text = $"{player.chara.HP_NOW} / {player.chara.HP_MAX}";
            sp_gauge.text = $"{player.chara.SP_NOW} / {player.chara.SP_MAX}";
            hp_Fill.Fill(player.chara.HP_PER);
            sp_Fill.Fill(player.chara.SP_PER);
        }

        IEnumerator Generator() {
            while (true) {
                yield return new WaitForSeconds(generatInterval);
                int     re  = Random.Range(0, prehab.Count);
                Vector3 pos = new Vector3(Lerp(posA.x, posB.x, Random.Range(0f, 1f)), Lerp(posA.y, posB.y, Random.Range(0f, 1f)), Lerp(posA.z, posB.z, Random.Range(0f, 1f)));
                Instantiate(prehab[re], pos, Quaternion.identity, parent.transform);
            }
        }

        IEnumerator Result() {
            if (generatTask != null) StopCoroutine(generatTask);
            result.Active(true);
            yield return TimeLerp(value => { result.Fill(value); });
            int score = (data.useItemCount * 300) + (data.killEnemyCountor * 1000);
            string[] resultTexts = {
                $"アイテム使用数　：　{RepeatText("", 6 - data.useItemCount)}{data.useItemCount}",
                $"\r\n倒した敵の数　　：　{RepeatText("", 6 - data.killEnemyCountor)}{data.killEnemyCountor}",
                $"\r\n\r\n計スコア　　　　：　{RepeatText("", 6 - score)}{score}",
            };
            for (int i = 0; i < resultTexts.Length; i++) {
                yield return new WaitForSeconds(1f);
                seAction?.Invoke();
                resultText.text = $"{resultText.text}{resultTexts[i]}";
            }
            if (data.Score < score) data.Score = score;
            clearButton.Set(true);
        }

    }

}