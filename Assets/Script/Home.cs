using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using MyLibrary;
using static MyLibrary.Static;

namespace Retrem {

    public class Home : Base {

        Camera mainCamera;
        public GameObject      home, game;
        [Min(0f)]
        public float           loadTime;

        public Action          seAction;

        public Animator        characterAnimator;

        public Image           background;
               TransValue      backgroundS, backgroundE;

        public Image           logo;
               TransValue      logoS, logoE;

        public GameObject      planeT, planeH;
               TransValue      planeS, planeV, planeE;

        public Button          startButton;
               RectTransform   startButtonR;
               TransValue      startButtonS, startButtonE;

        [Min(0f)]
        public float           sceneTime;

        public RectTransform[] menu;
               TransValue[]    menuS;
               TransValue      menuV;

        public Button          nameChangeButton,  // 名前変更
                               operationButton,   // 操作説明
                               initializeButton,  // 初期化
                               gotoTitleButton,   // タイトルに戻る
                               gotoQuestButton,   // クエスト開始
                               hideUiButton,      // UIを非表示
                               openBagButton,     // バッグを開く
                               gotoGachaButton;   // がちゃる

        public enum            State { Usually, SidePlayer, HideUI }
               State           state = State.Usually;
               bool            task  = false;

        public Button          touchPanel;
               bool            isTouch = false;

        public Image           taskPanel;

        public RectTransform[] taskType;

        public TextMeshProUGUI playerName, cost, maxScore, currentTime;
        public Image           hpGauge, spGauge;

        public TMP_InputField  tmpInputField;
        public Button          nameChange,
                               initialize;

        public GachaPanel      gachaPanel;

        public ItemList        itemlist;

        // === Active になる度に実行 ===
        void OnEnable() {
            if (mainCamera == null) mainCamera = Camera.main;
            mainCamera.orthographic = true;
            sound.PlayBGM("ho");
            EditUI();
            data.items.For(i => { data.items[i].UseSync(this); });
        }

        // === 起動時に始めの1回実行 ===
        void Start() {
            seAction = () => { sound.PlaySE("pu"); };
            ItemAction();
            gachaPanel.Initialize(this, data.items, data.gachaCost, () => { seAction?.Invoke(); EditUI(); }, () => { sound.PlaySE("er"); }, () => M0);
            itemlist.  Initialize(this, data.items, () => { seAction?.Invoke(); EditUI(); });
            GetValue();
            SetClickAction();
            EditUI();
        }

        // === 繰り返し実行(fps依存) ===
        void Update() {
            currentTime.text = CurrentTime(false, ":");
            if (isTouch) {
                isTouch = false;
                Log("touch");
                if (!task) {
                    switch (state) {
                        case State.Usually:    task = false;  break;
                        case State.SidePlayer: PlayerMove(); tmpInputField.text = ""; break;
                        case State.HideUI:     HideUI(false); break;
                    }
                }
            }
        }

        void GetValue() {
            logoS               = logo.rectTransform;
            logoE               = logo.rectTransform;
            logoE.pos_y        += Screen.height;
            backgroundS         = background.rectTransform;
            backgroundE         = background.rectTransform;
            backgroundE.pos_y  += backgroundE.size.y;
            startButtonR        = startButton.GetComponent<RectTransform>();
            startButtonS        = startButtonR;
            startButtonE        = startButtonR;
            startButtonE.pos_y -= Screen.height;
            planeS              = planeH;
            planeV              = planeT;
            planeE              = planeT;
            planeE.pos_z       -= 10f;
            menuS               = new TransValue[menu.Length];
            menu.For(i => { menuS[i] = menu[i]; });
            menuV               = menu[0];
            menuV.pos           = Vec3();
        }

        void SetClickAction() {
            startButton.     ClickAction(() => { seAction?.Invoke(); GoToTitleHome(false); });
            touchPanel.      ClickAction(() => { isTouch = true;                           });
            nameChangeButton.ClickAction(() => { seAction?.Invoke(); PlayerMove(0);        });  // 0
            operationButton. ClickAction(() => { seAction?.Invoke(); PlayerMove(1);        });  // 1
            initializeButton.ClickAction(() => { seAction?.Invoke(); PlayerMove(2);        });  // 2
            gotoTitleButton. ClickAction(() => { seAction?.Invoke(); GoToTitleHome(true);  });
            gotoQuestButton. ClickAction(() => { seAction?.Invoke(); admin.FadeAction(loadTime, () => { game.Active(true); home.Active(false);  }); });
            hideUiButton.    ClickAction(() => { seAction?.Invoke(); HideUI(true);         });
            openBagButton.   ClickAction(() => { seAction?.Invoke(); PlayerMove(3);        });  // 3
            gotoGachaButton. ClickAction(() => { seAction?.Invoke(); PlayerMove(4);        });  // 4
            tmpInputField.   InputAction(() => { Log("書き換え");      });
            nameChange.      ClickAction(() => { seAction?.Invoke(); if (!tmpInputField.text.IsEmpty()) data.Player.Name = tmpInputField.text; EditUI(); });
            initialize.      ClickAction(() => { seAction?.Invoke(); data.Initialize(); EditUI(); });
        }

        void GoToTitleHome(bool tTitle_fHome) {
            startButton.Set(false);
            characterAnimator.SetTrigger("IsJump");
            float from = tTitle_fHome ? 1f : 0f, to = tTitle_fHome ? 0f : 1f;
            StartCoroutines(StuckCoroutine(AllWait(
                TimeLerp(value => { logo.      rectTransform.ToLerp(logoS,        logoE,        value); }, sceneTime, from, to),
                TimeLerp(value => { background.rectTransform.ToLerp(backgroundS,  backgroundE,  value); }, sceneTime, from, to),
                TimeLerp(value => { startButtonR.            ToLerp(startButtonS, startButtonE, value); }, sceneTime, from, to),
                TimeLerp(value => { planeT.    transform.    ToLerp(planeV,       planeE,       value); }, sceneTime, from, to),
                TimeLerp(value => { planeH.    transform.    ToLerp(planeS,       planeV,       value); }, sceneTime, from, to),
                HideUIC(tTitle_fHome)
                ),() => { startButton.Set(true); }
            ));
        }

        void EditUI() {
            playerName.text  = $"{data.Player.Name}";
            maxScore.text    = $"Max Score\n{data.Score}";
            cost.text        = data.gachaCost.value < 1000000 ? $"{RepeatText(" ", 7 - data.gachaCost.value.Length())}{data.gachaCost.value}" : "999999+";
            hpGauge.Fill(data.Player.HP_PER);
            spGauge.Fill(data.Player.SP_PER);
            itemlist.DisplayUpdate();
        }

        void HideUI(bool hide) {
            task = true;
            StartCoroutine(StuckCoroutine(
                HideUIC(hide),
                () => { state = hide ? State.HideUI : State.Usually; task = false; }
            ));
        }
        IEnumerator HideUIC(bool hide, bool topStay = false) {
            yield return TimeLerp(value => { menu.For(i => { if (!topStay || 0 < i) menu[i].ToLerp(menuS[i], menuV, value); }); }, sceneTime, hide ? 1f : 0f, hide ? 0f : 1f);
        }

        void PlayerMove(int taskTypeIndex = -1, float movePos = -6.5f) {
            task = true;
            bool center = taskTypeIndex == -1;
            if (center) taskPanel.Active(false);
            characterAnimator.LocalAngleY(center ? 90f : 270f);
            characterAnimator.SetFloat("Move Speed", 0.1f);
            StartCoroutine(StuckCoroutine(AnyWait(
                HideUIC(!center, true),
                TimeLerp(value => { characterAnimator.LocalPosX(Lerp(0f, movePos, value)); }, sceneTime, center ? 1f : 0f, center ? 0f : 1f)),
                () => {
                    characterAnimator.SetFloat("Move Speed", 0f);
                    characterAnimator.LocalAngleY(540f);
                    characterAnimator.RotateY(0.01f);
                    if (taskTypeIndex != -1) {
                        taskType.For(i => {
                            if (i == taskTypeIndex) {
                                taskType[i].Active(true);
                            }else {
                                taskType[i].Active(false);
                            }
                        });
                    }
                    if (!center) taskPanel.Active(true);
                    if (taskTypeIndex == 2) sound.PlaySE("er");
                    state = center ? State.Usually : State.SidePlayer;
                    task = false;
                }
            ));
        }

        void ItemAction() {
            data.items.For(i => {
                switch (data.items[i].name) {
                    case "Garbage":    data.items[i].action = () => data.Player.HP_NOW   +=  3;                       break;
                    case "Herb":       data.items[i].action = () => data.Player.HP_NOW   += (data.Player.HP_MAX / 5); break;
                    case "Juice":      data.items[i].action = () => data.Player.HP_NOW   += (data.Player.HP_MAX / 4); break;
                    case "Drug":       data.items[i].action = () => data.Player.HP_MAX   +=  5;                       break;
                    case "PowerUp":    data.items[i].action = () => data.Player.ATK      +=  5;                       break;
                    case "Invin":      data.items[i].action = () => data.Player.HP_MAX   += 10;                       break;
                    case "ResetTimer": data.items[i].action = () => data.questTimeCountor = data.questTime;           break;
                    case "Random":     data.items[i].action = () => data.items.GetRandom().action?.Invoke();          break;
                }
            });
        }

    }

}