// #define Pun2                             // Photon Unity Networking 2 を Assets Store から入手してる場合のみ定義

using System;                            // [Serializable]とかに必要
using System.IO;                         // File書き込みのSave,Loadするために必要
using System.Text;
using System.Linq;                       // Linqの戻り値はIEnumerable<T>型、Linqのラムダ式 (単変数 => 単変数を使う処理) と ((単変数, index) => 単変数とindexを使う処理)
using System.Reflection;
using System.Diagnostics;
using System.Collections;                // コレクションに関連するクラス（List、Dictionaryなど）を使用
using System.Collections.Generic;        // ジェネリック（ジェネリッククラスやジェネリックメソッド）に関連するクラスを使用
using System.Threading.Tasks;            // 非同期処理を扱うためのクラスやメソッドを使う時追加

using UnityEngine;                       // Unityエンジンのクラスや関数を使用
using UnityEngine.Events;                // 関数付きオブジェクトの窓口
using UnityEngine.EventSystems;          // EventSystemを扱うのに必要
using UnityEngine.UI;                    // UIを扱う時追加 
using UnityEngine.UIElements;            // UIのさらに高度な機能 
using UnityEngine.SceneManagement;       // シーンを扱う時追加 
using TMPro;                             // TextMethProを使う時追加

#if UNITY_EDITOR

using UnityEditor;                       // Unityのエディタ拡張を作成
using TMPro.EditorUtilities;             // TMPのさらに高度な機能
using UnityEditor.AnimatedValues;

#endif

using Image        = UnityEngine.UI.Image;
using Button       = UnityEngine.UI.Button;
using Toggle       = UnityEngine.UI.Toggle;
using Random       = UnityEngine.Random;
using Object       = UnityEngine.Object;
using Color        = UnityEngine.Color;
using ColorUtility = UnityEngine.ColorUtility;
using Cursor       = UnityEngine.Cursor;
using Debug        = UnityEngine.Debug;

using MyLibrary;
using static MyLibrary.Static;

#if Pun2
using Photon.Pun;
using Photon.Realtime;
#endif

/* ===== コピー用フォーマット (Super) =====

using UnityEngine;

using MyLibrary;
using static MyLibrary.Static;

public class C#script名 : Super {

    // === unity上に表示する機能 ===

    // === 起動時に始めの1回実行 ===
    protected override void Start(){
        base.Start();
    }

    // === 繰り返し実行(fps依存) ===
    protected override void Update(){
        base.Update();
    }

    // === 繰り返し実行(1/50fps) ===
    protected override void FixedUpdate(){
        base.FixedUpdate();
    }

    // === 使用する関数()を定義  ===

} 

*/

/* ===== コピー用フォーマット (Base) =====

using UnityEngine;

using MyLibrary;
using static MyLibrary.Static;

public class C#script名 : Base {

    // === unity上に表示する機能 ===

    // === 起動時に始めの1回実行 ===
    void Start(){
        
    }

    // === 繰り返し実行(fps依存) ===
    void Update(){
        
    }

    // === 繰り返し実行(1/50fps) ===
    void FixedUpdate(){
        
    }

    // === 使用する関数()を定義  ===

} 

*/

/* ===== コピー用フォーマット (Editor) =====

#if UNITY_EDITOR

using UnityEditor;

#endif

#if UNITY_EDITOR

[CustomEditor(typeof(書換対象Class))]
public class C#script名 : BaseE<書換対象Class> {

    protected override void Inspector() {

        

    }

}

#endif

*/

/* ===== コピー用フォーマット (Baseの上書きGameData、namespace内宣言) =====

// ===== Bace =====
public class Base : MyLibrary.Base {

    protected GameData data;

    // === 起動時の最初に1回実行 ===
    protected override void Awake() {
        base.Awake();
        data = admin.data as GameData;
    }

}

*/

// ===== namespace =====      // namespace、重複を許容
namespace System.Runtime.CompilerServices {
    public class IsExternalInit { }
}
namespace MyLibrary {

// ===== Pun2専用Baseを継承させる =====
#if Pun2
    public class MoBe : MonoBehaviourPunCallbacks{}
#else
    public class MoBe : MonoBehaviour {}
#endif

    // ===== Admin =====          // アタッチデータセット
    public class Admin : MoBe {
    
        // Screenサイズ変更を検出する (横画面)
        public Vector2          screen = new Vector2(1920f, 1080f);
    
        // ScriptableObject をアタッチ
        public ScriptableObject data;
    
        // class SoundSystem   を実装
        public SoundSystem      sound = new SoundSystem();
    
        // シングルトンでインスタンスを１つにする
        public static Admin     single { get; private set; }
    
        // フェード用のImage と その上のテキスト
        public ImageBool        fade = new ImageBool();
        public bool             IsFade { get => fade.isFlag; }
               TextMeshProUGUI  fadeText;
               Coroutine        fadeCor;

#if Pun2

        // class Network  を実装
        public Network network = new Network();

#endif

        // === 起動時の最初に1回実行 ===
        void Awake() {
            // 同類が、存在してたら後者が消滅、存在しないなら残る
            if (single == null) { single = this; DontDestroyOnLoad(gameObject); } else Destroy(gameObject);
            // フレームレート調整
            Application.targetFrameRate = 60;
            // サウンドシステムにソース提供
            sound.Set(gameObject.ReferenceComponent<AudioSource>());
        }

        // === 起動時に始めの1回実行 ===
        [Obsolete]
        void Start() {
            // シーン内にEventSystemがなければ子に追加
            if (!FindComponent<EventSystem>()) GenerateEventSystem("EventSystem", gameObject);
#if Pun2
            // サーバーに繋ぐ
            ServerLogin();
#endif
        }
    
        // === 繰り返し実行(fps依存) ===
        void Update(){
            // イベントリスナー実行
            ListenerUpdate();
            // テスト用フェード
            // if (Input.GetMouseButton(1)) FadeAction();
#if Pun2
            // サーバーに繋いでる時の処理
            if (network.inServer) {
                if (!InLobby && network.authority == Network.AuthorityType.gameMaster) {
                    if (Input.GetMouseButtonDown(2)) {
                        if (InRoom) OutRoom();
                        LobbyLogin();
                    }
                }
                if (InRoom) {
                    network.nowRoom = RoomName;
                }else {
                    network.nowRoom = "";
                    if (Input.GetMouseButtonDown(1)) {
                        RoomLogin(network.roomID, network.create);
                        network.roomID++;
                    }
                }
            }
#endif
        }

        // === 繰り返しの末尾で実行  ===
        void LateUpdate() {
            // スクリーンサイズが違うなら？
            if (IsScreenSizeDiffer(screen)) {
                // MenuPanel調整
                if (fade.panel != null) fade.panel.rectTransform.ScreenAdjust(Vec2(1f), Vec2(0f));
                // 新しいスクリーンサイズに設定
                screen = GetScreenSize();
            }
        }

        // === Object消失時に1回実行 ===
        void OnDestroy() {
            if (single == this) ListenerClear();
        }

        // === バックグラウンド出帰  ===
        void OnApplicationPause(bool flag) {
            if (flag) {
                // アプリがバックグラウンドに移動した時の処理
            } else {
                // アプリがバックグラウンドから復帰した時の処理
            }
        }
    
        /// <summary> 処理をフェードで挟む </summary>
        public void FadeAction(float fadeTime = 2f, UnityAction action = null) {
            if (fadeCor != null) StopCoroutine(fadeCor);
            fadeCor = StartCoroutine(FadeActionCor(fadeTime, action));
        }
        /// <summary> 処理をフェードで挟む </summary>
        public IEnumerator FadeActionCor(float fadeTime = 2f, UnityAction action = null) {
            if (fadeTime <= 0f) {
                if (action != null) action();
                fadeCor = null;
                yield break;
            }
            yield return FadeOut(fadeTime);
            action?.Invoke();
            yield return FadeIn(fadeTime);
            fadeCor = null;
        }
        /// <summary> 処理をフェードで挟む </summary>
        public void FadeCor(float fadeTime = 2f, IEnumerator action = null) {
            if (fadeCor != null) StopCoroutine(fadeCor);
            fadeCor = StartCoroutine(FadeCorCor(fadeTime, action));
        }
        /// <summary> 処理をフェードで挟む </summary>
        public IEnumerator FadeCorCor(float fadeTime = 2f, IEnumerator action = null) {
            if (fadeTime <= 0f) {
                if (action != null) yield return StartCoroutine(action);
                fadeCor = null;
                yield break;
            }
            yield return FadeOut(fadeTime);
            if (action != null) yield return StartCoroutine(action);
            yield return FadeIn(fadeTime);
            fadeCor = null;
        }
        /// <summary> フェードアウト </summary>
        IEnumerator FadeOut(float fadeTime) {
            fade.isFlag = true;
            if (fade.panel == null) {
                Canvas canvas = this.GetChild<Canvas>();
                if (canvas == null) canvas = GenerateCanvas("Canvas", 100, gameObject);
                fade.panel = GenerateImage("Fade Panel", canvas.gameObject, null, Color.black);
                fade.panel.rectTransform.ScreenAdjust(Vec2(1f), Vec2(0f));
            }
            fade.panel.enabled = true;
            yield return Fade(fade.panel, 0f, 1f, fadeTime);
        }
        /// <summary> フェードイン </summary>
        IEnumerator FadeIn(float fadeTime) {
            if (fade.panel == null) {
                Canvas canvas = this.GetChild<Canvas>();
                if (canvas == null) canvas = GenerateCanvas("Canvas", 100,gameObject);
                fade.panel = GenerateImage("FadeImage", canvas.gameObject, null, Color.black);
                fade.panel.rectTransform.ScreenAdjust(Vec2(1f), Vec2(0f));
            }
            yield return Fade(fade.panel, 1f, 0f, fadeTime);
            fade.panel.enabled = false;
            fade.isFlag = false;
        }

        /// <summary> (f) 物理2D歩行、X軸 </summary> <param name="rb"> Rigid参照 </param> <param name="speed"> 一定速度 </param>
        public void ForceWalk(Rigidbody2D rb, float speed) {
            float force = speed - rb.velocity.x;
            rb.AddForce(Vector2.right * force, ForceMode2D.Force);
        }
        /// <summary> (f) 物理3D歩行、X軸 </summary> <param name="rb"> Rigid参照 </param> <param name="speed"> 一定速度 </param>
        public void ForceWalk(Rigidbody rb, float speed) {
            float force = speed - rb.velocity.x;
            rb.AddForce(Vector3.right * force, ForceMode.Force);
        }

#if Pun2

        /// <summary> 全てのルーム情報取得 </summary>
        public override void OnRoomListUpdate(IReadOnlyList<RoomInfo> roomList) {
            if (network.room.Count < roomList.Count) {
                for (int i = 0; i < (roomList.Count - network.room.Count); i++) {
                    Room addRoom = new Room();
                    network.room.Add(addRoom);
                }
            }
            if (network.room.Count > roomList.Count) {
                Debug.Log($"{network.room.Count}  {roomList.Count}");
                network.room.RemoveRangeAll(network.room.Count - roomList.Count, false);
            }
            for (int i = 0; i < roomList.Count; i++) {
                network.room[i].name        = roomList[i].Name;
                network.room[i].maxPlayer   = roomList[i].MaxPlayers;
                network.room[i].playerCount = roomList[i].PlayerCount;
            }
        }

        /// <summary> マスターサーバーに正常に接続された時に実行 </summary>
        public override void OnConnectedToMaster() {
            network.inServer = true;
            Debug.Log("サーバーに接続しました");
        }

        /// <summary> ルームに参加した時に実行 </summary>
        public override void OnJoinedRoom() {
            // プレイヤーをインスタンス化するなど
            network.roomID = 0;
            Debug.Log("ルームに参加しました");
        }

        /// <summary> ルームに参加した時に実行 </summary>
        public override void OnJoinedLobby() {
            Debug.Log("ロビーに参加しました");
        }

#endif

    }

    // ===== 静的,拡張メソッド =====   // 静的,拡張(既存の型を拡張)
    public static class Static {

        #if UNITY_EDITOR

        // デバッグ用
        static int    taskLog = 0;

        #endif

        // デリゲートの定義
        public delegate void Listener();

        // イベントの定義
        public static event Listener listener;

        /// <summary> リスナー登録されている仕事の数を返す </summary>
        public static int ListenerCount() { return listener?.GetInvocationList().Length ?? 0; }

        /// <summary> リスナー登録されている仕事の名前配列を返す </summary>
        public static string[] ListenerNames() { return listener?.GetInvocationList().Select(d => d.Method.Name) .ToArray() ?? new string[0]; }

        /// <summary> (u) リスナーイベント点火 </summary>
        public static void ListenerUpdate() { if (Application.isPlaying) listener?.Invoke(); }

        /// <summary> リスナーイベントを空にする </summary>
        public static void ListenerClear() { listener = null; }

        // ひらがなテーブル
        public static readonly string[] hiragana  = { "あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ", "さ", "し", "す", "せ", "そ", "た", "ち", "つ", "て", "と", "な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ", "ま", "み", "む", "め", "も", "や", "ゆ", "よ", "ら", "り", "る", "れ", "ろ", "わ", "を", "ん" };

        // カタカナテーブル
        public static readonly string[] katakana  = { "ア", "イ", "ウ", "エ", "オ", "カ", "キ", "ク", "ケ", "コ", "サ", "シ", "ス", "セ", "ソ", "タ", "チ", "ツ", "テ", "ト", "ナ", "ニ", "ヌ", "ネ", "ノ", "ハ", "ヒ", "フ", "ヘ", "ホ", "マ", "ミ", "ム", "メ", "モ", "ヤ", "ユ", "ヨ", "ラ", "リ", "ル", "レ", "ロ", "ワ", "ヲ", "ン" };

        // アルファベット小文字テーブル
        public static readonly string[] lowercase = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        // アルファベット大文字テーブル
        public static readonly string[] uppercase = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        // 素数テーブル
        public static readonly int[]    primes    = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113 };

        // キーボード中段テーブル
        public static readonly KeyCode[] keyASD    = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L };

        /// <summary> 1フレーム間の時間 ( Time.deltaTime ) </summary>
        public static float frame { get => Time.deltaTime; }

        /// <summary> ローカルの現在時刻を取得 </summary> <param name="seconds">秒を含むか？</param> <param name="sp"> 間の文字 </param>
        public static string CurrentTime(bool seconds = false, string sp = " : ") {
            // 現在時刻の取得
            DateTime currentTime = DateTime.Now;
            // 時と分の取得
            string hour   = $"{currentTime.Hour}";
            string minute = $"{sp}{(currentTime.Minute.Length() < 2 ? "0" : "")}{currentTime.Minute}";
            string second = $"{(seconds ? ($"{sp}{(currentTime.Second.Length() < 2 ? "0" : "")}{currentTime.Second}") : "")}";
            // 時と分 や秒を返す
            return $"{hour}{minute}{second}";
        }

        // InputDownの短縮コード
        public static bool A  { get => Input.GetKeyDown(KeyCode.A); }
        public static bool B  { get => Input.GetKeyDown(KeyCode.B); }
        public static bool C  { get => Input.GetKeyDown(KeyCode.C); }
        public static bool D  { get => Input.GetKeyDown(KeyCode.D); }
        public static bool E  { get => Input.GetKeyDown(KeyCode.E); }
        public static bool F  { get => Input.GetKeyDown(KeyCode.F); }
        public static bool G  { get => Input.GetKeyDown(KeyCode.G); }
        public static bool H  { get => Input.GetKeyDown(KeyCode.H); }
        public static bool I  { get => Input.GetKeyDown(KeyCode.I); }
        public static bool J  { get => Input.GetKeyDown(KeyCode.J); }
        public static bool K  { get => Input.GetKeyDown(KeyCode.K); }
        public static bool L  { get => Input.GetKeyDown(KeyCode.L); }
        public static bool M  { get => Input.GetKeyDown(KeyCode.M); }
        public static bool N  { get => Input.GetKeyDown(KeyCode.N); }
        public static bool O  { get => Input.GetKeyDown(KeyCode.O); }
        public static bool P  { get => Input.GetKeyDown(KeyCode.P); }
        public static bool Q  { get => Input.GetKeyDown(KeyCode.Q); }
        public static bool R  { get => Input.GetKeyDown(KeyCode.R); }
        public static bool S  { get => Input.GetKeyDown(KeyCode.S); }
        public static bool T  { get => Input.GetKeyDown(KeyCode.T); }
        public static bool U  { get => Input.GetKeyDown(KeyCode.U); }
        public static bool V  { get => Input.GetKeyDown(KeyCode.V); }
        public static bool W  { get => Input.GetKeyDown(KeyCode.W); }
        public static bool X  { get => Input.GetKeyDown(KeyCode.X); }
        public static bool Y  { get => Input.GetKeyDown(KeyCode.Y); }
        public static bool Z  { get => Input.GetKeyDown(KeyCode.Z); }
        public static bool AU { get => Input.GetKeyDown(KeyCode.UpArrow);    }
        public static bool AD { get => Input.GetKeyDown(KeyCode.DownArrow);  }
        public static bool AR { get => Input.GetKeyDown(KeyCode.RightArrow); }
        public static bool AL { get => Input.GetKeyDown(KeyCode.LeftArrow);  }
        public static bool LS { get => Input.GetKeyDown(KeyCode.LeftShift);  }
        public static bool RS { get => Input.GetKeyDown(KeyCode.RightShift); }
        public static bool SP { get => Input.GetKeyDown(KeyCode.Space) ; }
        public static bool EN { get => Input.GetKeyDown(KeyCode.Return); }
        public static bool M0 { get => Input.GetMouseButtonDown(0); }
        public static bool M1 { get => Input.GetMouseButtonDown(1); }
        public static bool M2 { get => Input.GetMouseButtonDown(2); }

        public static bool Ad  { get => Input.GetKeyDown(KeyCode.A); }
        public static bool Bd  { get => Input.GetKeyDown(KeyCode.B); }
        public static bool Cd  { get => Input.GetKeyDown(KeyCode.C); }
        public static bool Dd  { get => Input.GetKeyDown(KeyCode.D); }
        public static bool Ed  { get => Input.GetKeyDown(KeyCode.E); }
        public static bool Fd  { get => Input.GetKeyDown(KeyCode.F); }
        public static bool Gd  { get => Input.GetKeyDown(KeyCode.G); }
        public static bool Hd  { get => Input.GetKeyDown(KeyCode.H); }
        public static bool Id  { get => Input.GetKeyDown(KeyCode.I); }
        public static bool Jd  { get => Input.GetKeyDown(KeyCode.J); }
        public static bool Kd  { get => Input.GetKeyDown(KeyCode.K); }
        public static bool Ld  { get => Input.GetKeyDown(KeyCode.L); }
        public static bool Md  { get => Input.GetKeyDown(KeyCode.M); }
        public static bool Nd  { get => Input.GetKeyDown(KeyCode.N); }
        public static bool Od  { get => Input.GetKeyDown(KeyCode.O); }
        public static bool Pd  { get => Input.GetKeyDown(KeyCode.P); }
        public static bool Qd  { get => Input.GetKeyDown(KeyCode.Q); }
        public static bool Rd  { get => Input.GetKeyDown(KeyCode.R); }
        public static bool Sd  { get => Input.GetKeyDown(KeyCode.S); }
        public static bool Td  { get => Input.GetKeyDown(KeyCode.T); }
        public static bool Ud  { get => Input.GetKeyDown(KeyCode.U); }
        public static bool Vd  { get => Input.GetKeyDown(KeyCode.V); }
        public static bool Wd  { get => Input.GetKeyDown(KeyCode.W); }
        public static bool Xd  { get => Input.GetKeyDown(KeyCode.X); }
        public static bool Yd  { get => Input.GetKeyDown(KeyCode.Y); }
        public static bool Zd  { get => Input.GetKeyDown(KeyCode.Z); }
        public static bool AUd { get => Input.GetKeyDown(KeyCode.UpArrow);    }
        public static bool ADd { get => Input.GetKeyDown(KeyCode.DownArrow);  }
        public static bool ARd { get => Input.GetKeyDown(KeyCode.RightArrow); }
        public static bool ALd { get => Input.GetKeyDown(KeyCode.LeftArrow);  }
        public static bool LSd { get => Input.GetKeyDown(KeyCode.LeftShift);  }
        public static bool RSd { get => Input.GetKeyDown(KeyCode.RightShift); }
        public static bool SPd { get => Input.GetKeyDown(KeyCode.Space) ; }
        public static bool ENd { get => Input.GetKeyDown(KeyCode.Return); }
        public static bool M0d { get => Input.GetMouseButtonDown(0); }
        public static bool M1d { get => Input.GetMouseButtonDown(1); }
        public static bool M2d { get => Input.GetMouseButtonDown(2); }

        public static bool As  { get => Input.GetKey(KeyCode.A); }
        public static bool Bs  { get => Input.GetKey(KeyCode.B); }
        public static bool Cs  { get => Input.GetKey(KeyCode.C); }
        public static bool Ds  { get => Input.GetKey(KeyCode.D); }
        public static bool Es  { get => Input.GetKey(KeyCode.E); }
        public static bool Fs  { get => Input.GetKey(KeyCode.F); }
        public static bool Gs  { get => Input.GetKey(KeyCode.G); }
        public static bool Hs  { get => Input.GetKey(KeyCode.H); }
        public static bool Is  { get => Input.GetKey(KeyCode.I); }
        public static bool Js  { get => Input.GetKey(KeyCode.J); }
        public static bool Ks  { get => Input.GetKey(KeyCode.K); }
        public static bool Ls  { get => Input.GetKey(KeyCode.L); }
        public static bool Ms  { get => Input.GetKey(KeyCode.M); }
        public static bool Ns  { get => Input.GetKey(KeyCode.N); }
        public static bool Os  { get => Input.GetKey(KeyCode.O); }
        public static bool Ps  { get => Input.GetKey(KeyCode.P); }
        public static bool Qs  { get => Input.GetKey(KeyCode.Q); }
        public static bool Rs  { get => Input.GetKey(KeyCode.R); }
        public static bool Ss  { get => Input.GetKey(KeyCode.S); }
        public static bool Ts  { get => Input.GetKey(KeyCode.T); }
        public static bool Us  { get => Input.GetKey(KeyCode.U); }
        public static bool Vs  { get => Input.GetKey(KeyCode.V); }
        public static bool Ws  { get => Input.GetKey(KeyCode.W); }
        public static bool Xs  { get => Input.GetKey(KeyCode.X); }
        public static bool Ys  { get => Input.GetKey(KeyCode.Y); }
        public static bool Zs  { get => Input.GetKey(KeyCode.Z); }
        public static bool AUs { get => Input.GetKey(KeyCode.UpArrow);    }
        public static bool ADs { get => Input.GetKey(KeyCode.DownArrow);  }
        public static bool ARs { get => Input.GetKey(KeyCode.RightArrow); }
        public static bool ALs { get => Input.GetKey(KeyCode.LeftArrow);  }
        public static bool LSs { get => Input.GetKey(KeyCode.LeftShift);  }
        public static bool RSs { get => Input.GetKey(KeyCode.RightShift); }
        public static bool SPs { get => Input.GetKey(KeyCode.Space) ; }
        public static bool ENs { get => Input.GetKey(KeyCode.Return); }
        public static bool M0s { get => Input.GetMouseButton(0); }
        public static bool M1s { get => Input.GetMouseButton(1); }
        public static bool M2s { get => Input.GetMouseButton(2); }

        public static bool Au  { get => Input.GetKeyUp(KeyCode.A); }
        public static bool Bu  { get => Input.GetKeyUp(KeyCode.B); }
        public static bool Cu  { get => Input.GetKeyUp(KeyCode.C); }
        public static bool Du  { get => Input.GetKeyUp(KeyCode.D); }
        public static bool Eu  { get => Input.GetKeyUp(KeyCode.E); }
        public static bool Fu  { get => Input.GetKeyUp(KeyCode.F); }
        public static bool Gu  { get => Input.GetKeyUp(KeyCode.G); }
        public static bool Hu  { get => Input.GetKeyUp(KeyCode.H); }
        public static bool Iu  { get => Input.GetKeyUp(KeyCode.I); }
        public static bool Ju  { get => Input.GetKeyUp(KeyCode.J); }
        public static bool Ku  { get => Input.GetKeyUp(KeyCode.K); }
        public static bool Lu  { get => Input.GetKeyUp(KeyCode.L); }
        public static bool Mu  { get => Input.GetKeyUp(KeyCode.M); }
        public static bool Nu  { get => Input.GetKeyUp(KeyCode.N); }
        public static bool Ou  { get => Input.GetKeyUp(KeyCode.O); }
        public static bool Pu  { get => Input.GetKeyUp(KeyCode.P); }
        public static bool Qu  { get => Input.GetKeyUp(KeyCode.Q); }
        public static bool Ru  { get => Input.GetKeyUp(KeyCode.R); }
        public static bool Su  { get => Input.GetKeyUp(KeyCode.S); }
        public static bool Tu  { get => Input.GetKeyUp(KeyCode.T); }
        public static bool Uu  { get => Input.GetKeyUp(KeyCode.U); }
        public static bool Vu  { get => Input.GetKeyUp(KeyCode.V); }
        public static bool Wu  { get => Input.GetKeyUp(KeyCode.W); }
        public static bool Xu  { get => Input.GetKeyUp(KeyCode.X); }
        public static bool Yu  { get => Input.GetKeyUp(KeyCode.Y); }
        public static bool Zu  { get => Input.GetKeyUp(KeyCode.Z); }
        public static bool AUu { get => Input.GetKeyUp(KeyCode.UpArrow);    }
        public static bool ADu { get => Input.GetKeyUp(KeyCode.DownArrow);  }
        public static bool ARu { get => Input.GetKeyUp(KeyCode.RightArrow); }
        public static bool ALu { get => Input.GetKeyUp(KeyCode.LeftArrow);  }
        public static bool LSu { get => Input.GetKeyUp(KeyCode.LeftShift);  }
        public static bool RSu { get => Input.GetKeyUp(KeyCode.RightShift); }
        public static bool SPu { get => Input.GetKeyUp(KeyCode.Space) ; }
        public static bool ENu { get => Input.GetKeyUp(KeyCode.Return); }
        public static bool M0u { get => Input.GetMouseButtonUp(0); }
        public static bool M1u { get => Input.GetMouseButtonUp(1); }
        public static bool M2u { get => Input.GetMouseButtonUp(2); }

        /// <summary> キーボードA横一列に作業を割り当てる </summary>
        public static void ASD(Action action, params Action[] actions) {
            Action[] fn = Params(action, actions);
            for (int i = 0; i < fn.Length; i++) {
                if (i < keyASD.Length) {
                    if (Input.GetKeyDown(keyASD[i])) fn[i]();
                }else break;
            }
        }

        /// <summary> KeyCodeとboolで指定 (Input.GetKey(指定Key);) 指定bool無:Down, true:Stay , false:Up  </summary>
        public static bool InputPush(KeyCode key, bool? flag = null) {
            if (flag == null) return Input.GetKeyDown(key);
            if ( flag.Value)  return Input.GetKey(key);
            else              return Input.GetKeyUp(key);
        }
        /// <summary> 数値とboolで指定 (Input.GetMouseButton(指定index);) 指定bool無:Down, true:Stay , false:Up </summary>
        public static bool InputPush(int mouseButton, bool? flag = null) {
            mouseButton = Clamp(mouseButton, 0, 2);
            if (flag == null) return Input.GetMouseButtonDown(mouseButton);
            if ( flag.Value)  return Input.GetMouseButton(mouseButton);
            else              return Input.GetMouseButtonUp(mouseButton);
        }

        /// <summary> 引数全てが true なら true を返す </summary>
        public static bool And(bool flag, params bool[] flags) {
            if (!flag) return false;
            return flags.All(f => f);
        }

        /// <summary> 引数どれかが true なら true を返す </summary>
        public static bool Or(bool flag, params bool[] flags) {
            if (flag) return true;
            return flags.Any(f => f);
        }

        /// <summary> for文の再現、(回数, i => { iを使える; })、クロージャ問題解決 </summary>
        public static void For<T>(this IReadOnlyList<T> list, Action<int> action, Func<bool> breakConditions = null) {
            if (list == null) return;
            For(list.Count, action, breakConditions);
        }
        /// <summary> for文の再現、(回数, i => { iを使える; })、クロージャ問題解決 </summary>
        public static void For(int count, Action<int> action, Func<bool> breakConditions = null) {
            if (count <= 0 || action == null) return;
            for (int i = 0; i < count; i++) {
                if (breakConditions != null && breakConditions()) break;
                int index = i;
                action(index);
            }
        }
        /// <summary> for文の再現、(回数, i => { iを使える; })、クロージャ問題解決 </summary>
        public static void ForBack<T>(this IReadOnlyList<T> list, Action<int> action, Func<bool> breakConditions = null) {
            if (list == null) return;
            ForBack(list.Count, action, breakConditions);
        }
        /// <summary> for文の再現、(回数, i => { iを使える; })、クロージャ問題解決 </summary>
        public static void ForBack(int count, Action<int> action, Func<bool> breakConditions = null) {
            if (count <= 0 || action == null) return;
            for (int i = (count - 1); 0 <= count; i--) {
                if (breakConditions != null && breakConditions()) break;
                int index = i;
                action(index);
            }
        }

        /// <summary> Instantiate の複数系 </summary>
        public static GameObject[] Instantiates(GameObject prefab, int num, Vector3? pos = null, Quaternion? qua = null, Transform par = null) {
            if (prefab == null || num < 1) return null;
            if (pos == null) pos = Vector3.zero;
            if (qua == null) qua = Quaternion.identity;
            GameObject[] objs = new GameObject[num];
            for(int i = 0; i < num; i++) objs[i] = Object.Instantiate(prefab, pos.Value, qua.Value, par);
            return objs;
        }

        /// <summary> リストのindexからすべて消す (true:前側, false:後側) </summary> <param name="index"> 指定場所も消す </param>
        public static bool RemoveRangeAll<T>(this List<T> list, int index, bool flag) {
            if (index < 0 || list.Count <= index) {
                Debug.LogWarning("配列外参照");
                return false;
            }
            if (list != null && 0 < list.Count) {
                if (flag) {
                    list.RemoveRange(0, index);
                }else {
                    list.RemoveRange(index, list.Count - index);
                }
            }return true;
        }

        /// <summary> new Vector2() の短縮形 (0f, 0f) </summary>
        public static Vector2 Vec2()                 { return new Vector2(); }
        /// <summary> new Vector2() の短縮形、(x,y,を同じ値に設定) </summary>
        public static Vector2 Vec2(float value)      { return new Vector2(value, value); }
        /// <summary> new Vector2() の短縮形 </summary>
        public static Vector2 Vec2(float x, float y) { return new Vector2(x, y); }
        
        /// <summary> new Vector3() の短縮形 (0f, 0f, 0f) </summary>
        public static Vector3 Vec3()                          { return new Vector3(); }
        /// <summary> new Vector3() の短縮形 (x, y, z, を同じ値に設定) </summary>
        public static Vector3 Vec3(float value)               { return new Vector3(value, value, value); }
        /// <summary> new Vector3() の短縮形 </summary>
        public static Vector3 Vec3(float x, float y, float z) { return new Vector3(x, y, z); }
    
        /// <summary> スクリーンサイズ取得 </summary>
        public static Vector2 GetScreenSize() { return Vec2(Screen.width, Screen.height); }
    
        /// <summary> 縦横比を返す (縦/横) </summary> <returns> 1080/1920(0.5625f), 1920/1080(1.7777f) </returns> <remarks> 横長 < 1f < 縦長 </remarks>
        public static float ScreenRatio() { return (float)Screen.height / Screen.width; }
    
        /// <summary> スクリーンサイズが違うならTrue </summary>
        public static bool IsScreenSizeDiffer(Vector2 screen) { return screen.x != Screen.width || screen.y != Screen.height; }
    
        /// <summary> CursorModeを変更する </summary> <param name="center"> true:画面中央固定非表示 , false:固定無し表示 </param> <param name="screen"> true:カーソル移動制限(スクリーン内) , false:移動制限無し </param>
        public static void CursorLock(bool center = false, bool screen = false) {
            Cursor.lockState = center ? CursorLockMode.Locked : screen ? CursorLockMode.Confined : CursorLockMode.None;
        }

        /// <summary> (ui) UIをスクリーンサイズに合わせて、アンカー位置と、比率でサイズを調整する </summary> <param name="ratio"> 比率を設定(0f〜1fまでがスクリーンの範囲内) </param> <param name="pos"> アンカー座標 </param> <remark> スクリーンサイズと比較し、割合でサイズを決定し、アンカー座標を設定する </remark>
        public static void ScreenAdjust(this RectTransform rect, Vector2 ratio, Vector3? pos = null) {
            rect.SetScale(Vec2(1f));
            if (pos == null) pos = rect.GetAnchorPos();
            rect.SetSize(Vec2(Screen.width * ratio.x, Screen.height * ratio.y));
            rect.SetAnchorPos(pos.Value);
        }
        /// <summary> (ui) UIをスクリーンサイズに合わせて、アンカー位置とスケールを調整する (初期比 1920f*1080f) </summary> <param name="ratio"> true:比率そのまま , false:変形可 </param> <param name="origin"> 前のピクセル比 </param> <remark> スクリーンサイズと比較し、X,Y,の小さい倍率で、スケールを等倍する </remark>
        public static void ScreenAdjust(this RectTransform rect, bool ratio, Vector3? origin = null) {
            if (origin == null) origin = Vec2(1920f, 1080f);
            float x = Screen.width  / origin.Value.x;
            float y = Screen.height / origin.Value.y;
            if (ratio) {
                float scale = x < y ? x : y;
                rect.SetAnchorPos(Vec3(rect.GetAnchorPos().x / rect.GetScale().x * scale, rect.GetAnchorPos().y / rect.GetScale().y * scale, rect.GetAnchorPos().z / rect.GetScale().z * scale));
                rect.SetScale(Vec3(scale));
            }else{
                rect.SetAnchorPos(Vec2(rect.GetAnchorPos().x / rect.GetScale().x * x, rect.GetAnchorPos().y / rect.GetScale().y * y));
                rect.SetScale(Vec2(x, y));
            }
        }
    
        /// <summary> スマホで実行中か？ </summary> <returns> true:スマホ, false:その他 </returns>
        public static bool IsPhone   { get => IsAndroid || IsIPhone; }
    
        /// <summary> Androidで実行中か？ </summary> <returns> true:Android, false:その他 </returns>
        public static bool IsAndroid { get => Application.platform == RuntimePlatform.Android; }
    
        /// <summary> IPhoneで実行中か？ </summary> <returns> true:IPhone, false:その他 </returns>
        public static bool IsIPhone  { get => Application.platform == RuntimePlatform.IPhonePlayer; }
    
        /// <summary> PCで実行中か？ </summary> <returns> true:PC, false:その他 </returns>
        public static bool IsPC      { get => IsWindows || IsMac || IsLinux; }
    
        /// <summary> Windowsで実行中か？ </summary> <returns> true:Windows, false:その他 </returns>
        public static bool IsWindows { get => Application.platform == RuntimePlatform.WindowsPlayer; }
        /// <summary> Macで実行中か？ </summary> <returns> true:Mac, false:その他 </returns>
        public static bool IsMac     { get => Application.platform == RuntimePlatform.OSXPlayer; }
        /// <summary> Linuxで実行中か？ </summary> <returns> true:Linux, false:その他 </returns>
        public static bool IsLinux   { get => Application.platform == RuntimePlatform.LinuxPlayer; }

        /// <summary> Editorで動いているか？ </summary> <returns> true:Editor, false:ビルド後 </returns>
        public static bool IsEditor  { get => Application.isEditor; }

        /// <summary> 実行中か？ </summary> <returns> true:実行中, false:実行してない </returns>
        public static bool IsPlay　  { get => Application.isPlaying; }

        /// <summary> ランダムな2択を返す </summary>
        public static bool RandomFlag { get => Random.Range(0, 2) != 0; }

        /// <summary> Boolで 1, -1 を返す </summary> <param name="flag"> 指定 > 有:(true:1 , false:-1), 無:ランダム(1, -1) </param> <returns> 1, -1 </returns>
        public static int IntFlag(bool? flag = null) { return flag != null ? (flag == true ? 1 : -1) : (Random.Range(0, 2) != 0 ? 1 : -1); }
    
        /// <summary> (s) intの桁数を返す(0 => 1) </summary> <param name="num"> 調べるint </param> <returns> 桁数 </returns>
        public static int Length(this int num) {
            // 0の場合は桁数が1となる
            if (num == 0) return 1;
            // 負の数も正の数に変換してから対数を取る
            return (int)Math.Log10(Math.Abs(num)) + 1;
        }
    
        /// <summary> 繰文字生成 </summary>
        public static string RepeatText(string text, int num) {
            if (!string.IsNullOrEmpty(text) && num > 0) {
                StringBuilder result = new StringBuilder(text.Length * num);
                for (int i = 0; i < num; i++) result.Append(text);
                return result.ToString();
            }return "";
        }

        /// <summary> 配列最初の｢null,!null｣のindexを返す </summary> <param name="list"> 探索コレクション </param> <param name="_null"> ｢null,!null｣どっち？ </param> <param name="index"> 探索開始index </param>
        public static int ArrayElementIndex<T>(this IReadOnlyList<T> list, bool _null = true, int index = 0) {
            for (int i = index; i < list.Count; i++) {
                if (_null && list[i] == null) return i;
                if (!_null && list[i] != null) return i;
            }return -1;
        }
    
        /// <summary> 配列の｢null,!null｣を数える </summary> <param name="list"> 探索配列 </param> <param name="_null"> ｢null,!null｣どっち？ </param> <param name="index"> 探索開始index </param>
        public static int ArrayElementCount<T>(this IReadOnlyList<T> list, bool _null = true, int index = 0) {
            int count = 0;
            for (int i = index; i < list.Count; i++) {
                if (_null && list[i] == null) count++;
                if (!_null && list[i] != null) count++;
            }return count;
        }
    
        /// <summary> ref で値を交換する </summary>
        public static void Swap<T>(ref T v1, ref T v2)  where T : IComparable<T> { T temp = v1; v1 = v2; v2 = temp; }

        /// <summary> 配列要素入れ替え </summary> <param name="list"> 対象 </param> <param name="v1"> index </param> <param name="v2"> index </param>
        public static void Swap<T>(this IList<T> list, int v1, int v2) where T : IComparable<T> {
            if (v1 < 0 || list.Count < v1 || v2 < 0 || list.Count < v2) {
                Log(null, "index_error");
                return;
            }
            T temp   = list[v1];
            list[v1] = list[v2];
            list[v2] = temp;
        }
    
        /// <summary> バブルソート </summary> <param name="sort"> true:昇順, false:降順 </param> <returns> ソート後配列 </returns>
        public static void Sort_Bubble<T>(this IList<T> list, bool sort) where T : IComparable<T> {
            for (int k = list.Count - 1; k > 0; k--) {
                for (int i = list.Count - 1; i > 0; i--) {
                    int result;
                    if (sort)  result = list[i].CompareTo(list[i - 1]);
                    else       result = list[i - 1].CompareTo(list[i]);
                    if (result < 0) list.Swap(i, i - 1);
                }
            }
        }
    
        /// <summary> 選択ソート </summary> <param name="sort"> true:昇順, false:降順 </param> <returns> ソート後配列 </returns>
        public static void Sort_Select<T>(this IList<T> list, bool sort) where T : IComparable<T> {
            for (int k = 0; k < list.Count; k++) {
                int min = k;
                for (int i = k; i < list.Count; i++) {
                    if (sort) if (list[min].CompareTo(list[i]) > 0) min = i;
                    else      if (list[min].CompareTo(list[i]) < 0) min = i;
                }list.Swap(k, min);
            }
        }
    
        /// <summary> (再起) マージソート </summary> <param name="sort"> true:昇順, false:降順 </param> <returns> ソート後配列 </returns>
        public static T[] Sort_Merge<T>(this T[] array, bool sort) where T : IComparable<T> {
            if (array.Length <= 1) return array;
            int m = array.Length / 2;
            T[] l = new T[m];
            T[] r = new T[array.Length - m];
            for (int i = 0; i < m; i++) l[i] = array[i];
            for (int i = m; i < array.Length; i++) r[i - m] = array[i];
            return Merge(Sort_Merge(l, sort), Sort_Merge(r, sort), sort);
        }
        static T[] Merge<T>(T[] l, T[] r, bool sort) where T : IComparable<T> {
            T[] res = new T[l.Length + r.Length];
            int i = 0, j = 0, k = 0;
            while (i < l.Length && j < r.Length) res[k++] = sort ? (l[i].CompareTo(r[j]) <= 0 ? l[i++] : r[j++]) : (l[i].CompareTo(r[j]) > 0 ? l[i++] : r[j++]);
            while (i < l.Length) res[k++] = l[i++];
            while (j < r.Length) res[k++] = r[j++];
            return res;
        }

        /// <summary> (再起) フィボナッチ数列生成 </summary>
        public static int Fibonacci(int n) {
            if (n <= 0) return 0;
            else if (n == 1) return 1;
            else return Fibonacci(n - 1) + Fibonacci(n - 2);
        }

        /// <summary> 四捨五入 </summary> <param name="digit"> 0(3.14f => 3f), 1(3.14f => 3.1f) </param>
        public static float Round   (this float num, int digit = 0) { return (float)Math.Round(num, digit); }
        /// <summary> 指定桁以下切取 </summary> <param name="digit"> 桁(10の位:1、0.1の位:-1) </param>
        public static float CutDigit(this float num, int digit)     { float pow = (float)Math.Pow(10, digit); return (float)Math.Truncate(num * pow) / pow; }
        /// <summary> 小数切上 (少数があるなら、+1, 少数切捨) </summary> <returns> 切上後値 </returns>
        public static float Ceiling (this float num) { return (float)Math.Ceiling(num); }
        /// <summary> 小数切捨 (少数があるなら、-1, 少数切捨) </summary> <returns> 切捨後値 </returns>
        public static float Floor   (this float num) { return (float)Math.Floor(num); }
    
        /// <summary> 数値を範囲内に調整 </summary>
        public static float Clamp(   float value, float min, float max) { return Mathf.Clamp(value, min, max); }
        /// <summary> 数値を範囲内に調整 </summary>
        public static int   Clamp(   int   value, int   min, int   max) { return (int)Clamp((float)value, (float)min, (float)max); }
        /// <summary> 数値をMin以上に調整 </summary>
        public static float ClampMin(float value, float min) { return Mathf.Max(value, min); }
        /// <summary> 数値をMin以上に調整 </summary>
        public static int   ClampMin(int   value, int   min) { return (int)ClampMin((float)value, (float)min); }
        /// <summary> 数値をMax以下に調整 </summary>
        public static float ClampMax(float value, float max) { return Mathf.Min(value, max); }
        /// <summary> 数値をMax以下に調整 </summary>
        public static int   ClampMax(int   value, int   max) { return (int)ClampMax((float)value, (float)max); }

        /// <summary> IDと確率の全体データを渡すと、結果IDを返す </summary>
        public static int Gacha(GachaValue gachaValue, params GachaValue[] gachaValues) {
            GachaValue[] value = Params(gachaValue, gachaValues);
            // 全体を計算
            float totalProb = 0f;
            foreach (GachaValue gv in value) totalProb += gv.probability;
            // 乱数を生成して確率を決定
            float rand = Random.Range(0f, totalProb);
            float re = 0f;
            foreach (GachaValue gv in value) {
                re += gv.probability;
                if (rand < re) return gv.id;
            }return value[Random.Range(0, value.Length)].id;
        }
        /// <summary> IDと確率の全体データを渡すと、結果IDを返す </summary>
        public static int Gacha(IReadOnlyList<GachaValue> gachaValues) {
            // 全体を計算
            float totalProb = 0f;
            foreach (GachaValue gv in gachaValues) totalProb += gv.probability;
            // 乱数を生成して確率を決定
            float rand = Random.Range(0f, totalProb);
            float re = 0f;
            foreach (GachaValue gv in gachaValues) {
                re += gv.probability;
                if (rand < re) return gv.id;
            }return gachaValues[Random.Range(0, gachaValues.Count)].id;
        }

        /// <summary> 文字列がnullまたは空でtrueを返す </summary> <returns> true:nullか空, false:1文字以上入ってる </returns>
        public static bool IsEmpty(this string text) { return String.IsNullOrEmpty(text); }
        /// <summary> 指定文字列が含まれてるか？を返す </summary>
        public static bool In(this string text, string check) { return text.Contains(check); }
        /// <summary> 指定文字列を全て｢ ｣に置換 </summary>
        public static string Delete(this string text, string delete) {
            while (text.In(delete)) {
                text = text.Replace(delete, string.Empty);
            }return text;
        }

        /// <summary> オブジェクト名から指定文字列を全て消す </summary>
        public static void NameTrimming(this Collision  collision,  string delete) { collision.collider.NameTrimming(delete); }
        /// <summary> オブジェクト名から指定文字列を全て消す </summary>
        public static void NameTrimming(this Component  component,  string delete) { component.gameObject.NameTrimming(delete); }
        /// <summary> オブジェクト名から指定文字列を全て消す </summary>
        public static void NameTrimming(this GameObject gameObject, string delete) {
            string text = gameObject.name;
            text = text.Delete(delete);
            gameObject.name = text;
        }

        /// <summary> (u) floatを｢00:00｣ので文字列で返す </summary> <param name="timer"> 時間変数 </param> <returns> ｢00:00｣形式文字列 </returns>
        public static string FormatTime(this float timer) {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    
        /// <summary> byte  0〜255[4] => Color </summary>
        public static Color ToColor(byte  r, byte  g, byte  b, byte  a) {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
        /// <summary> int   0〜255[4] => Color </summary>
        public static Color ToColor(int   r, int   g, int   b, int   a) {
            r = Clamp(r, 0, 255);
            g = Clamp(g, 0, 255);
            b = Clamp(b, 0, 255);
            a = Clamp(a, 0, 255);
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
        /// <summary> float 0〜255[4] => Color </summary>
        public static Color ToColor(float r, float g, float b, float a) {
            r = Clamp(r, 0f, 1f);
            g = Clamp(g, 0f, 1f);
            b = Clamp(b, 0f, 1f);
            a = Clamp(a, 0f, 1f);
            return new Color(r, g, b, a);
        }
        /// <summary> "#ColorCode"    => Color </summary>
        public static Color ToColor(string hex) {
            hex = hex.Replace("#", "").ToUpper();
            if (hex.Length != 6 && hex.Length != 8) { Debug.LogError("LengthError"); return Color.white; }
            if (!System.Text.RegularExpressions.Regex.IsMatch(hex, @"\A\b[0-9A-F]+\b\Z")) { Debug.LogError("FormatError"); return Color.white; }
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255;
            if (hex.Length == 8) a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary> コレクションをカンマ空白(, )区切りで１つの文字列にする </summary>
        public static string Join<T>(this IReadOnlyList<T> text, string sp = ", ") { return string.Join(sp, text); }

        #if UNITY_EDITOR

        /// <summary> 呼び出す度に値を変える </summary>
        public static void      Log(Color? color = null) { taskLog++; Log(null, $"タスク {taskLog}  ...完了"); }

        #else

        /// <summary> 何もしない </summary>
        public static void      Log() { }

        #endif

        /// <summary> Debug.Log()、値には干渉しない </summary>
        public static T         Log<T>(     T       text,  Color? color = null                     ) { Debug.Log($"{(color != null ? $"<color=#{RGBA(color.Value)}>" : "")}{                            text                    }{$"{(color != null ? "</color>" : "")}"}"); return text;  }
        /// <summary> 出力物につけるLog、値には干渉しない、引数入力で[logText : text]となる </summary>
        public static T         Log<T>(this T       text,  Color? color = null, string logText = "") { Debug.Log($"{(color != null ? $"<color=#{RGBA(color.Value)}>" : "")}{(logText == "" ? "" : $"{logText} : ")}{    text    }{$"{(color != null ? "</color>" : "")}"}"); return text;  }
        /// <summary> 配列をカンマ区切りで Debug.Log() </summary>
        public static T[]       Log<T>(this T[]     texts, Color? color = null, string logText = "") { Debug.Log($"{(color != null ? $"<color=#{RGBA(color.Value)}>" : "")}{(logText == "" ? "" : $"{logText} : ")}{texts.Join()}{$"{(color != null ? "</color>" : "")}"}"); return texts; }
        /// <summary> リストをカンマ区切りで Debug.Log() </summary>
        public static List<T>   Log<T>(this List<T> texts, Color? color = null, string logText = "") { Debug.Log($"{(color != null ? $"<color=#{RGBA(color.Value)}>" : "")}{(logText == "" ? "" : $"{logText} : ")}{texts.Join()}{$"{(color != null ? "</color>" : "")}"}"); return texts; }
        /// <summary> 配列をカンマ区切りで Debug.Log() </summary>
        public static T[]       Log<T>(T       text0,  T       text1,  Color? color = null, params T[]       texts) { 
            T[] re = new T[(texts != null ? texts.Length : 0) + 2];
            re[0] = text0;
            re[1] = text1;
            if (texts != null) for(int i = 2; i < re.Length; i++) re[i] = texts[i - 2];
            return re.Log(color);
        }
        /// <summary> 配列をカンマ区切りで Debug.Log() </summary>
        public static T[][]     Log<T>(T[]     texts0, T[]     texts1, Color? color = null, params T[][]     texts) { 
            T[][] re = new T[(texts != null ? texts.Length : 0) + 2][];
            re[0] = texts0;
            re[1] = texts1;
            if (texts != null) for(int i = 2; i < re.Length; i++) re[i] = texts[i - 2];
            for(int i = 0; i < re.Length; i++) re[i].Log(color);
            return re;
        }
        /// <summary> リストをカンマ区切りで Debug.Log() </summary>
        public static List<T>[] Log<T>(List<T> texts0, List<T> texts1, Color? color = null, params List<T>[] texts) {
            List<T>[] re = new List<T>[((texts != null ? texts.Length : 0) + 2)];
            re[0] = texts0;
            re[1] = texts1;
            if (texts != null) for(int i = 2; i < re.Length; i++) re[i] = texts[i - 2];
            for(int i = 0; i < re.Length; i++) re[i].Log(color);
            return re;
        }
        
        /// <summary> 色をRGBA表記にする </summary>
        public static string RGBA(Color color) => ColorUtility.ToHtmlStringRGBA(color);

        /// <summary> スクタブを1つのstring(Json)に書換 </summary> <param name="input"> ScriptableObject </param> <returns> Json化した文字列 </returns>
        public static string ToJson<T>(T input) where T : ScriptableObject { return JsonUtility.ToJson(input); }

        /// <summary> string(Json)からスクタブに書込 </summary> <param name="input"> ScriptableObject </param> <param name="json"> Json化した文字列 </param>
        public static void InJson<T>(T input, string json) where T : ScriptableObject { JsonUtility.FromJsonOverwrite(json, input); }

        /// <summary> セーブ(Json) </summary> <param name="fileName"> File名 </param> <param name="input"> ScriptableObject </param> <returns> true:成功, false:失敗 </returns>
        public static bool Save<T>(T input, string fileName = "SaveData") where T : ScriptableObject {
            try {
                string path = Path.Combine(IsEditor ? Application.dataPath : Application.persistentDataPath, $"{fileName}{(fileName.In(".json") ? "" : ".json")}");
                string json = ToJson(input);
                File.WriteAllText(path, json);
                Debug.Log($"セーブしました。{path}");
                return true;
            }
            catch (Exception e) {
                Debug.LogWarning($"セーブに失敗しました: {e.Message}");
                return false;
            }
        }

        /// <summary> ロード(Json) </summary> <param name="fileName"> File名 </param> <param name="input"> ScriptableObject </param> <returns> true:成功, false:失敗 </returns>
        public static bool Load<T>(T input, string fileName = "SaveData") where T : ScriptableObject {
            string path = Path.Combine(IsEditor ? Application.dataPath : Application.persistentDataPath, $"{fileName}{(fileName.EndsWith(".json") ? "" : ".json")}");
            if (File.Exists(path)) {
                try {
                    string json = File.ReadAllText(path);
                    InJson(input, json);
                    return true;
                }
                catch (Exception e) {
                    Debug.LogWarning("ロードエラー: " + e.ToString());
                    return false;
                }
            }else {
                Debug.LogWarning("ファイルが存在しません: " + path);
                return false;
            }

        }


        /// <summary> (2) 物理移動軸固定 </summary> <param name="x"> true:固定, false:解除 </param> <param name="y"> true:固定, false:解除 </param>
        public static void ConstMove(this Rigidbody2D rb, bool? x = null, bool? y = null) {
            if (rb != null) {
                if (x != null) { if (x.Value) rb.constraints |= RigidbodyConstraints2D.FreezePositionX; else rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX; }
                if (y != null) { if (y.Value) rb.constraints |= RigidbodyConstraints2D.FreezePositionY; else rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY; }
            }
        }
        /// <summary> (2) 物理回転軸固定 </summary> <param name="x"> true:固定, false:解除 </param> <param name="y"> true:固定, false:解除 </param>
        public static void ConstRotation(this Rigidbody2D rb, bool? z = null) {
            if (rb != null && z != null) if (z.Value) rb.constraints |=  RigidbodyConstraints2D.FreezeRotation; else rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
        }

        /// <summary> (3) 物理移動軸固定 </summary> <param name="x"> true:固定, false解除: </param> <param name="y"> true:固定, false:解除 </param> <param name="z"> true:固定, false:解除 </param>
        public static void ConstMove(this Rigidbody rb, bool? x = null, bool? y = null, bool? z = null) {
            if (rb != null) {
                if (x != null) { if (x.Value) rb.constraints |= RigidbodyConstraints.FreezePositionX; else rb.constraints &= ~RigidbodyConstraints.FreezePositionX; }
                if (y != null) { if (y.Value) rb.constraints |= RigidbodyConstraints.FreezePositionY; else rb.constraints &= ~RigidbodyConstraints.FreezePositionY; }
                if (z != null) { if (z.Value) rb.constraints |= RigidbodyConstraints.FreezePositionZ; else rb.constraints &= ~RigidbodyConstraints.FreezePositionZ; }
            }
        }
        /// <summary> (3) 物理回転軸固定 </summary> <param name="x"> true:固定, false解除: </param> <param name="y"> true:固定, false:解除 </param> <param name="z"> true:固定, false:解除 </param>
        public static void ConstRotation(this Rigidbody rb, bool? x = null, bool? y = null, bool? z = null) {
            if (rb != null) {
                if (x != null) { if (x.Value) rb.constraints |= RigidbodyConstraints.FreezeRotationX; else rb.constraints &= ~RigidbodyConstraints.FreezeRotationX; }
                if (y != null) { if (y.Value) rb.constraints |= RigidbodyConstraints.FreezeRotationY; else rb.constraints &= ~RigidbodyConstraints.FreezeRotationY; }
                if (z != null) { if (z.Value) rb.constraints |= RigidbodyConstraints.FreezeRotationZ; else rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ; }
                
            }
        }
    
        /// <summary> (2ui) 物理2Dジャンプ </summary> <param name="rb"> Rigid参照 </param> <param name="height"> ジャンプ高 </param> 
        public static void ForceJump(this Rigidbody2D rb, float height) {
            float force = Mathf.Sqrt(2 * Physics2D.gravity.magnitude * rb.gravityScale * height);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
        /// <summary> (3ui) 物理3Dジャンプ </summary> <param name="rb"> Rigid参照 </param> <param name="height"> ジャンプ高 </param> 
        public static void ForceJump(this Rigidbody rb, float height) {
            float force = Mathf.Sqrt(2 * Physics.gravity.magnitude * height);
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        }
    
        /// <summary> KeyCodeリストのDownのOrBoolを返す </summary> <returns> 配列内にtrueになるKeyCodeがあればtrue </returns>
        public static bool GetKeyDownList(IReadOnlyList<KeyCode> keys) {
            foreach (KeyCode key in keys) {
                if (Input.GetKeyDown(key)) return true;
            }return false;
        }
        /// <summary> KeyCodeリストのStayのOrBoolを返す </summary> <returns> 配列内にtrueになるKeyCodeがあればtrue </returns>
        public static bool GetKeyList(IReadOnlyList<KeyCode> keys) {
            foreach (KeyCode key in keys) {
                if (Input.GetKey(key)) return true;
            }return false;
        }
        /// <summary> KeyCodeリストのUpのOrBoolを返す </summary> <returns> 配列内にtrueになるKeyCodeがあればtrue </returns>
        public static bool GetKeyUpList(IReadOnlyList<KeyCode> keys) {
            foreach (KeyCode key in keys) {
                if (Input.GetKeyUp(key)) return true;
            }return false;
        }
    
        /// <summary> シーンの切替 </summary> <param name="sceneName"> 転移希望シーン名 </param> <returns> true:完了 , false:Error </returns>
        public static bool LoadScene(string sceneName) {
            if (!string.IsNullOrEmpty(sceneName)) {
                SceneManager.LoadScene(sceneName);
                return true;
            }
            Debug.LogWarning("Sceneが使用不可");
            return false;
        }

        /// <summary> Object生成 </summary>
        public static GameObject GenerateObject(string name = "GameObject", GameObject parent = null) {
            GameObject obj = new GameObject(name);
            if (parent != null) obj.SetParent(parent);
            obj.SetLocalPos(Vec3(0f));
            return obj;
        }
        /// <summary> EventSystem生成 </summary>
        public static EventSystem GenerateEventSystem(string name = "EventSystem", GameObject parent = null) {
            GameObject  obj = GenerateObject(name, parent);
            EventSystem eve = obj.AddComponent<EventSystem>();
            obj.AddComponent<StandaloneInputModule>();
            return eve;
        }
        /// <summary> Canvas生成 </summary>
        public static Canvas GenerateCanvas(string name = "Canvas", int layer = 0, GameObject parent = null) {
            GameObject obj      = GenerateObject(name, parent);
            Canvas canvas       = obj.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = layer;
            obj.AddComponent<CanvasScaler>();
            obj.AddComponent<GraphicRaycaster>();
            return canvas;
        }
        /// <summary> Image生成 </summary>
        public static Image GenerateImage(string name = "Image", GameObject parent = null, Vector2? size = null, Color? color = null, Sprite sprite = null) {
            GameObject obj = GenerateObject(name, parent);
            Image image    = obj.AddComponent<Image>();
            if (size != null) image.rectTransform.SetSize(size.Value);
            image.color    = color  ?? Color.white;
            image.sprite   = sprite ?? GenerateSprite();
            return image;
        }

        /// <summary> Sprite(単色)生成 </summary> <remarks> ランタイムのみ参照可 </remarks>
        public static Sprite GenerateSprite(Color? color = null, int width = 128, int height = 128) {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color ?? Color.white;
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, width, height), new Vector2(0.5f, 0.5f));
        }

        /// <summary> Button生成 </summary>
        public static Button GenerateButton(string name = "Button", GameObject parent = null, Vector2? size = null, Color? color = null, Sprite sprite = null) {
            Image  image  = GenerateImage(name, parent, size, color, sprite);
            Button button = image.gameObject.AddComponent<Button>();
            return button;
        }

        /// <summary> TextMeshProUGUI生成 </summary>
        public static TextMeshProUGUI GenerateTMPro(string name = "Text", GameObject parent = null, Color? color = null, float fontSize = 50, TMP_FontAsset font = null) {
            GameObject      obj = GenerateObject(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text     = "";
            tmp.color    = color ?? Color.black;
            if (font != null) tmp.font = font;  // ?? Resources.GetBuiltinResource<TMP_FontAsset>("Default Font.asset");
            tmp.fontSize = fontSize;
            return tmp;
        }

        /// <summary> Object種類を入れ替える (Transform <=> RectTransform) </summary>
        public static void TransformChange(this GameObject obj) {
            // RectTransformがあるか？
            RectTransform rect = obj.GetComponent<RectTransform>();
            // RectTransform => Transform
            if (rect != null) {
                // 新しいゲームオブジェクトを作成
                GameObject newObject = new GameObject("ReplacedObject");
                // 元のオブジェクトの親を新しいオブジェクトの親に設定
                newObject.transform.SetParent(rect.parent, false);
                // Transformのプロパティをコピー
                newObject.SetLocalPos(GetLocalPos(rect));
                newObject.transform.localRotation = rect.localRotation;
                newObject.transform.localScale = rect.localScale;
                // 元のオブジェクトの子を新しいオブジェクトに移動
                while (rect.childCount > 0) {
                    rect.GetChild(0).SetParent(newObject.transform, false);
                }
               // 元のrectオブジェクトを削除（必要に応じて）
               // Destroy(gameObject);
            }
            // Transform => RectTransform
            else{
                obj.AddComponent<RectTransform>();
                obj.AddComponent<CanvasRenderer>();
            }
        }

        /// <summary> GetかAddしてComponentを必ず取得 </summary>
        public static T ReferenceComponent<T>(this Component component) where T : Component {
            return component.gameObject.ReferenceComponent<T>();
        }
        /// <summary> GetかAddしてComponentを必ず取得 </summary>
        public static T ReferenceComponent<T>(this GameObject gameObject) where T : Component {
            T re = gameObject.GetComponent<T>();
            if (re == null) re = gameObject.AddComponent<T>();
            return re;
        }
        /// <summary> Component名を指定して削除、なれけば無視 </summary>
        public static void RemoveComponent<T>(this GameObject obj) where T : Component {
            T component = obj.GetComponent<T>();
            if (component != null) {
                if (IsEditor) GameObject.DestroyImmediate(component);
                else GameObject.Destroy(component);
            }
        }

        /// <summary> 何かの配列を文字列配列にする </summary>
        public static string[]     ToStrings<T>(this T[] objs) { return objs.Select(obj => obj.ToString()).ToArray(); }
        /// <summary> 何かのリストを文字列リストにする </summary>
        public static List<string> ToStrings<T>(this List<T> objs) { return objs.Select(obj => obj.ToString()).ToList(); }

        /// <summary> 文字列をchar配列に分解する </summary>
        public static char[] ToChar(this string text) { return text.ToCharArray(); }
        /// <summary> char配列を文字列にする </summary>
        public static string AsString(this char[] chars) { return new string(chars); }

        /// <summary> クォータニオンからVector3を作成する </summary>
        public static Vector3 ToVector3(this Quaternion quaternion) { return new Vector3(quaternion.x, quaternion.y, quaternion.z); }

        /// <summary> オブジェクト名を返す </summary>
        public static string       ToName( this Collision       collision  ) { return collision.gameObject.name; }
        /// <summary> オブジェクト名を返す </summary>
        public static string[]     ToNames(this Collision[]     collisions ) { return collisions.ToGameObject().ToNames(); }
        /// <summary> オブジェクト名を返す </summary>
        public static List<string> ToNames(this List<Collision> collisions ) { return collisions.ToGameObject().ToNames(); }
        /// <summary> オブジェクト名を返す </summary>
        public static string       ToName( this Component       component  ) { return component.gameObject.name; }
        /// <summary> オブジェクト名を返す </summary>
        public static string[]     ToNames(this Component[]     components ) { return components.ToGameObject().ToNames(); }
        /// <summary> オブジェクト名を返す </summary>
        public static List<string> ToNames(this List<Component> components ) { return components.ToGameObject().ToNames(); }
        /// <summary> オブジェクト名を返す </summary>
        public static string       ToName( this GameObject      gameObject ) { return gameObject.name; }
        /// <summary> オブジェクト名を返す </summary>
        public static string[]     ToNames(this GameObject[]    gameObjects) { return gameObjects.Select(obj => obj.name).ToArray(); }
        /// <summary> オブジェクト名を返す </summary>
        public static List<string> ToNames(this List<GameObject> gameObjects) { return gameObjects.Select(obj => obj.name).ToList(); }

        /// <summary> コンポーネント配列をGameObject配列にして返す </summary>
        public static GameObject[]     ToGameObject(this Collision[] collisions) { return collisions.Select(col => col.gameObject).ToArray(); }
        /// <summary> コンポーネント配列をGameObject配列にして返す </summary>
        public static List<GameObject> ToGameObject(this List<Collision> collisions) { return collisions.Select(col => col.gameObject).ToList(); }
        /// <summary> コンポーネント配列をGameObject配列にして返す </summary>
        public static GameObject[]     ToGameObject(this Component[] components) { return components.Select(com => com.gameObject).ToArray(); }
        /// <summary> コンポーネント配列をGameObject配列にして返す </summary>
        public static List<GameObject> ToGameObject(this List<Component> components) { return components.Select(com => com.gameObject).ToList(); }
        /// <summary> GameObject配列をコンポーネント配列にして返す </summary>
        public static T[]     ToComponent<T>(this GameObject[] gameObjects) where T : Component { return gameObjects.Select(obj => obj.ReferenceComponent<T>()).ToArray(); }
        /// <summary> GameObject配列をコンポーネント配列にして返す </summary>
        public static List<T> ToComponent<T>(this List<GameObject> gameObjects) where T : Component { return gameObjects.Select(obj => obj.ReferenceComponent<T>()).ToList(); }

        /// <summary> シーン内の全ての見つけたGameObject参照を返す </summary> <returns> T:参照, null:ない </returns>
        [Obsolete]
        public static GameObject[] FindGameObjects() {
            GameObject[] matchObj = Object.FindObjectsOfType<GameObject>();
            return 0 < matchObj.Length ? matchObj : null;
        }

        /// <summary> シーン内の最初に見つけたコンポネ一致の参照を返す </summary> <returns> T:参照, null:ない </returns>
        [Obsolete]
        public static T FindComponent<T>() where T : Component {
            return Object.FindObjectOfType<T>();
        }

        /// <summary> シーン内の全ての見つけたコンポネ一致の参照を返す </summary> <returns> T:参照, null:ない </returns>
        [Obsolete]
        public static T[] FindComponents<T>() where T : Component {
            T[] matchObj = Object.FindObjectsOfType<T>();
            return 0 < matchObj.Length ? matchObj : null;
        }
        /// <summary> シーン内の最初に見つけたTag一致の参照を返す </summary> <returns> T:参照, null:ない </returns>
        public static GameObject FindTag(string tagName) {
            return GameObject.FindGameObjectWithTag(tagName);
        }
        /// <summary> シーン内の全ての見つけたTag一致の参照を返す </summary> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindTags(string tagName) {
            GameObject[] matchObj = GameObject.FindGameObjectsWithTag(tagName);
            return 0 < matchObj.Length ? matchObj : null;
        }

        /// <summary> シーン内の最初に見つけた名前一致の参照を返す </summary> <returns> T:参照, null:ない </returns>
        [Obsolete]
        public static GameObject FindName(string objName) {
            GameObject[] allObj = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObj) {
                if (obj.IsName(objName)) return obj;
            }return null;
        }

        /// <summary> シーン内の全ての見つけた名前一致の参照を返す </summary> <returns> T:参照, null:ない </returns>
        [Obsolete]
        public static GameObject[] FindNames(string objName) {
            List<GameObject> matchObj = new List<GameObject>();
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects) {
                if (obj.IsName(objName)) matchObj.Add(obj);
            }return 0 < matchObj.Count ? matchObj.ToArray() : null;
        }

        /// <summary> 子や子孫の数を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> 子や子孫の数 </returns>
        public static int ChildCount(this Collision  parent, bool descendants = false) { return parent.collider.ChildCount(descendants); }
        /// <summary> 子や子孫の数を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> 子や子孫の数 </returns>
        public static int ChildCount(this Component  parent, bool descendants = false) { return parent.gameObject.ChildCount(descendants); }
        /// <summary> 子や子孫の数を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> 子や子孫の数 </returns>
        public static int ChildCount(this GameObject parent, bool descendants = false) {
            if (parent != null) {
                int count = parent.transform.childCount;
                if (descendants) {
                    foreach (Transform child in parent.transform) {
                        // 再帰的に子孫を検索
                        count += child.gameObject.ChildCount(true);
                    }
                }return count;
            }return -1;
        }

        /// <summary> 子オブジェクトをすべて消す </summary> </summary> <param name="childs"> true:第1世代のみ残す , false:全ての子孫を消す </param>
        public static void ChildDestroy(this Collision  parent, bool childs = false) { parent.collider.ChildDestroy(childs); }
        /// <summary> 子オブジェクトをすべて消す </summary> </summary> <param name="childs"> true:第1世代のみ残す , false:全ての子孫を消す </param>
        public static void ChildDestroy(this Component  parent, bool childs = false) { parent.gameObject.ChildDestroy(childs); }
        /// <summary> 子オブジェクトをすべて消す </summary> </summary> <param name="childs"> true:第1世代のみ残す , false:全ての子孫を消す </param>
        public static void ChildDestroy(this GameObject parent, bool childs = false) {
            if (parent == null) return;
            if (childs) {
                foreach (Transform child in parent.transform) {
                    for (int i = child.childCount - 1; i >= 0; i--) {
                        if (IsEditor) GameObject.DestroyImmediate(child.GetChild(i).gameObject, true);
                        else GameObject.Destroy(child.GetChild(i).gameObject);
                    }
                }
            }else {
                for (int i = parent.transform.childCount - 1; i >= 0; i--) {
                    if (IsEditor) GameObject.DestroyImmediate(parent.transform.GetChild(i).gameObject, true);
                    else GameObject.Destroy(parent.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary> 子の最初に見つけたコンポネ一致の参照を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static T GetChild<T>(this Collision  parent, bool descendants = false) where T : Component { return parent.collider.GetChild<T>(descendants); }
        /// <summary> 子の最初に見つけたコンポネ一致の参照を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static T GetChild<T>(this Component  parent, bool descendants = false) where T : Component { return parent.gameObject.GetChild<T>(descendants); }
        /// <summary> 子の最初に見つけたコンポネ一致の参照を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static T GetChild<T>(this GameObject parent, bool descendants = false) where T : Component {
            if (parent != null) {
                foreach (Transform child in parent.transform) {
                    T component = child.GetComponent<T>();
                    if (component != null) return component;
                    if (descendants) {
                        // 再帰的に子孫を検索
                        T descendantComponent = child.gameObject.GetChild<T>(true);
                        if (descendantComponent != null) return descendantComponent;
                    }
                }return null;
            }return null;
        }
        /// <summary> 子の全ての見つけたコンポネ一致の参照を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static T[] GetChilds<T>(this Collision  parent, bool descendants = false) where T : Component { return parent.collider.GetChilds<T>(descendants); }
        /// <summary> 子の全ての見つけたコンポネ一致の参照を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static T[] GetChilds<T>(this Component  parent, bool descendants = false) where T : Component { return parent.gameObject.GetChilds<T>(descendants); }
        /// <summary> 子の全ての見つけたコンポネ一致の参照を返す </summary> </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static T[] GetChilds<T>(this GameObject parent, bool descendants = false) where T : Component {
            List<T> matchComponent = new List<T>();
            if (parent != null) {
                foreach (Transform child in parent.transform) {
                    T component = child.GetComponent<T>();
                    if (component != null) matchComponent.Add(component);
                    if (descendants) {
                        // 再帰的に子孫を検索
                        T[] descendantComponent = child.gameObject.GetChilds<T>(true);
                        if (descendantComponent != null && 0 < descendantComponent.Length) matchComponent.AddRange(descendantComponent);
                    }
                }
            }
            return 0 < matchComponent.Count ? matchComponent.ToArray() : null;
        }

        /// <summary> 子の最初に見つけたTag一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject FindTag(this Collision  parent, string tagName, bool descendants = false) { return parent.collider.FindTag(tagName, descendants); }
        /// <summary> 子の最初に見つけたTag一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject FindTag(this Component  parent, string tagName, bool descendants = false) { return parent.gameObject.FindTag(tagName, descendants); }
        /// <summary> 子の最初に見つけたTag一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject FindTag(this GameObject parent, string tagName, bool descendants = false) {
            if (parent != null) {
                foreach (Transform child in parent.transform) {
                    if (child.IsTag(tagName)) return child.gameObject;
                    if (descendants) {
                        // 再帰的に子孫を検索
                        GameObject descendantObj = child.gameObject.FindTag(tagName, true);
                        if (descendantObj != null) return descendantObj;
                    }
                }
            }return null;
        }
        /// <summary> 子の最初に見つけたTag一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindTags(this Collision  parent, string tagName, bool descendants = false) { return parent.collider.FindTags(tagName, descendants); }
        /// <summary> 子の最初に見つけたTag一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindTags(this Component  parent, string tagName, bool descendants = false) { return parent.gameObject.FindTags(tagName, descendants); }
        /// <summary> 子の最初に見つけたTag一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindTags(this GameObject parent, string tagName, bool descendants = false) {
            List<GameObject> matchObj = new List<GameObject>();
            if (parent != null) {
                foreach (Transform child in parent.transform) {
                    if (child.IsTag(tagName)) matchObj.Add(child.gameObject);
                    if (descendants) {
                        // 再帰的に子孫を検索
                        GameObject[] descendantObj = child.gameObject.FindTags(tagName, true);
                        if (descendantObj != null && 0 < descendantObj.Length) matchObj.AddRange(descendantObj);
                    }
                }
            }
            return matchObj.Count > 0 ? matchObj.ToArray() : null;
        }

        /// <summary> 子の最初に見つけた名前一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject FindName(this Collision  parent, string objName, bool descendants = false) { return parent.collider.FindName(objName, descendants); }
        /// <summary> 子の最初に見つけた名前一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject FindName(this Component  parent, string objName, bool descendants = false) { return parent.gameObject.FindName(objName, descendants); }
        /// <summary> 子の最初に見つけた名前一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject FindName(this GameObject parent, string objName, bool descendants = false) {
            if (parent != null) {
                foreach (Transform child in parent.transform) {
                    if (child.IsName(objName)) return child.gameObject;
                    if (descendants) {
                        // 再帰的に子孫を検索
                        GameObject descendantObj = child.gameObject.FindName(objName, true);
                        if (descendantObj != null) return descendantObj;
                    }
                }
            }return null;
        }
        /// <summary> 子の最初に見つけた名前一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindNames(this Collision  parent, string objName, bool descendants = false) { return parent.collider.FindNames(objName, descendants); }
        /// <summary> 子の最初に見つけた名前一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindNames(this Component  parent, string objName, bool descendants = false) { return parent.gameObject.FindNames(objName, descendants); }
        /// <summary> 子の最初に見つけた名前一致の参照を返す </summary> <param name="descendants"> true:全ての子孫 , false:第1世代のみ </param> <returns> T:参照, null:ない </returns>
        public static GameObject[] FindNames(this GameObject parent, string objName, bool descendants = false) {
            List<GameObject> matchObj = new List<GameObject>();
            if (parent != null) {
                foreach (Transform child in parent.transform) {
                    if (child.IsName(objName)) matchObj.Add(child.gameObject);
                    if (descendants) {
                        // 再帰的に子孫を検索
                        GameObject[] descendantObj = child.gameObject.FindNames(objName, true);
                        if (descendantObj != null && 0 < descendantObj.Length) matchObj.AddRange(descendantObj);
                    }
                }
            }
            return matchObj.Count > 0 ? matchObj.ToArray() : null;
        }

        /// <summary> コレクションからランダム取得 </summary>
        public static T GetRandom<T>(this IReadOnlyList<T> list) {
            if (list == null || list.Count == 0) return default(T);
            return list[Random.Range(0, list.Count)];
        }

        /// <summary> Active状態を変更 </summary>
        public static void Active(this Collision  collision,  bool flag) { collision.collider.gameObject.Active(flag); }
        /// <summary> Active状態を変更 </summary>
        public static void Active(this Component  component,  bool flag) { component.gameObject.Active(flag); }
        /// <summary> Active状態を変更 </summary>
        public static void Active(this GameObject gameObject, bool flag) { gameObject.SetActive(flag); }
    
        /// <summary> Activeチェックする </summary> <returns> true:Active , false:非Active </returns>
        public static bool IsActive(this Component  component)  { return component.gameObject.activeSelf; }
        /// <summary> Activeチェックする </summary> <returns> true:Active , false:非Active </returns>
        public static bool IsActive(this GameObject gameObject) { return gameObject.activeSelf; }
    
        /// <summary> Tagチェックする </summary> <returns> true:一致 , false:不一致 </returns>
        public static bool IsTag(this Collision  collision,  string tagName) { return collision.collider.IsTag(tagName); }
        /// <summary> Tagチェックする </summary> <returns> true:一致 , false:不一致 </returns>
        public static bool IsTag(this Component  component,  string tagName) { return component.gameObject.IsTag(tagName); }
        /// <summary> Tagチェックする </summary> <returns> true:一致 , false:不一致 </returns>
        public static bool IsTag(this GameObject gameObject, string tagName) { return gameObject.CompareTag(tagName); }

        /// <summary> Nameチェックする </summary> <returns> true:一致 , false:不一致 </returns>
        public static bool IsName(this Collision  collision,  string objName) { return collision.collider.IsName(objName); }
        /// <summary> Nameチェックする </summary> <returns> true:一致 , false:不一致 </returns>
        public static bool IsName(this Component  component,  string objName) { return component.gameObject.IsName(objName); }
        /// <summary> Nameチェックする </summary> <returns> true:一致 , false:不一致 </returns>
        public static bool IsName(this GameObject gameObject, string objName) { return gameObject.name == objName; }

        /// <summary> コンポーネントがついてるか確認するだけ </summary> <returns> true:ある, false:ない </returns>
        public static bool IsComponent<T>(this Collision  collision) { return collision.collider.IsComponent<T>(); }
        /// <summary> コンポーネントがついてるか確認するだけ </summary> <returns> true:ある, false:ない </returns>
        public static bool IsComponent<T>(this Component  component) { return component.gameObject.IsComponent<T>(); }
        /// <summary> コンポーネントがついてるか確認するだけ </summary> <returns> true:ある, false:ない </returns>
        public static bool IsComponent<T>(this GameObject gameObject) { T re = gameObject.GetComponent<T>(); return re != null; }

        /// <summary> (3) 自身が対象の方を向く </summary>
        public static void LookAt(this GameObject gameObject, Collision  target) { gameObject.LookAt(target.collider); }
        /// <summary> (3) 自身が対象の方を向く </summary>
        public static void LookAt(this GameObject gameObject, Component  target) { gameObject.LookAt(target.gameObject); }
        /// <summary> (3) 自身が対象の方を向く </summary>
        public static void LookAt(this GameObject gameObject, GameObject target) { gameObject.LookAt(target.transform); }
        /// <summary> (3) 自身が対象の方を向く </summary>
        public static void LookAt(this GameObject gameObject, Transform  target) { gameObject.transform.LookAt(target); }
    
        /// <summary> (u) マウスScroll値にspeedをかけて取得 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> 上:正の値, 下:負の値 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static float GetAxisMouseScroll(float speed = 1f, bool delta = true) { return Input.GetAxis("Mouse ScrollWheel") * speed * (delta ? Time.deltaTime : 1f); }
        /// <summary> (u) マウス移動量Xにspeedをかけて取得 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> 右:正の値, 左:負の値 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static float GetAxisMouseX(float speed = 1f, bool delta = true)  { return Input.GetAxis("Mouse X") * speed * (delta ? Time.deltaTime : 1f); }
        /// <summary> (u) マウス移動量Yにspeedをかけて取得 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> 上:正の値, 下:負の値 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static float GetAxisMouseY(float speed = 1f, bool delta = true)  { return Input.GetAxis("Mouse Y") * speed * (delta ? Time.deltaTime : 1f); }


        /// <summary> (u) 横移動の逆同時押しをしているか？ (A,D,←,→) </summary>
        public static bool GetAxisHorStay { get => (Ds || ARs) && (As || ALs); }
        /// <summary> (u) 奥移動の逆同時押しをしているか？ (W,S,↑,↓) </summary>
        public static bool GetAxisVerStay { get => (Ws || AUs) && (Ss || ADs); }

        /// <summary> (u) 横移動 (A,D,←,→) </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> -1f〜1f </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static float GetAxisHor(float speed = 1f, bool delta = true) { return GetAxisHorStay ? 0f : Input.GetAxis("Horizontal") * speed * (delta ? Time.deltaTime : 1f); }
        /// <summary> (u) 奥移動 (W,S,↑,↓) </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> -1f〜1f </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static float GetAxisVer(float speed = 1f, bool delta = true) { return GetAxisVerStay ? 0f : Input.GetAxis("Vertical") * speed * (delta ? Time.deltaTime : 1f); }
        /// <summary> (u) ジャンプ (Space) </summary> <returns> true:押された瞬間, false:それ以外 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static bool  IsJump() { return Input.GetButtonDown("Jump"); }

        /// <summary> (u) Axisの力を返す </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> -1f〜1f </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static float GetAxisPower(bool normalize = true, float speed = 1f, bool delta = false) {
            float re = Mathf.Sqrt(Mathf.Pow(GetAxisHor(speed, delta), 2f) + Mathf.Pow(GetAxisVer(speed, delta), 2f));
            return (normalize && GetAxisHor() != 0f && GetAxisVer() != 0f) ? re / Mathf.Sqrt(2f) : re;
        }

        /// <summary> (u) 攻撃１ (左Click,左Ctrl) </summary> <returns> true:押された瞬間, false:それ以外 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static bool GetFire1() { return Input.GetButtonDown("Fire1"); }
        /// <summary> (u) 攻撃２ (右Click,Alt) </summary> <returns> true:押された瞬間, false:それ以外 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static bool GetFire2() { return Input.GetButtonDown("Fire2"); }
        /// <summary> (u) 攻撃３ (LeftShift) </summary> <returns> true:押された瞬間, false:それ以外 </returns> <remarks> 「Edit」>「Project Settings」>「Input Manager」から変更可 </remarks>
        public static bool GetFire3() { return Input.GetButtonDown("Fire3"); }
    
        /// <summary> (3u) 入力で移動する </summary> <param name="moveSpeed"> 移動速度 </param> <param name="rotateSpeed"> Y軸回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void InputMove(this GameObject gameObject, float moveSpeed, float rotateSpeed, float dash = 1f, bool delta = true) {
            Vector3 movement = Vec3(GetAxisHor(), 0f, GetAxisVer()).normalized;
            Camera  camera   = Camera.main;
            camera.WorldAngleX(0f);
            movement = camera.Direction(movement);
            gameObject.transform.position += movement * (LSs ? moveSpeed * dash : moveSpeed) * (delta ? Time.deltaTime : 1f);
            if (movement != Vector3.zero) {
                Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
                gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, toRotation, rotateSpeed * (delta ? Time.deltaTime : 1f));
            }
        }
        /// <summary> (3u) 入力で移動する </summary> <param name="AxisX"> 横移動 </param> <param name="AxisZ"> 奥移動 </param> <param name="moveSpeed"> 移動速度 </param> <param name="rotateSpeed"> Y軸回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void InputMove(this GameObject gameObject, float AxisX, float AxisZ, float moveSpeed, float rotateSpeed, float dash = 1f, bool delta = true) {
            AxisX = Clamp(AxisX, -1f, 1f);
            AxisZ = Clamp(AxisZ, -1f, 1f);
            Vector3 movement = Vec3(AxisX, 0f, AxisZ).normalized;
            Camera  camera   = Camera.main;
            camera.WorldAngleX(0f);
            movement = camera.Direction(movement);
            gameObject.transform.position += movement * (LSs ? moveSpeed * dash : moveSpeed) * (delta ? Time.deltaTime : 1f);
            if (movement != Vector3.zero) {
                Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
                gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, toRotation, rotateSpeed * (delta ? Time.deltaTime : 1f));
            }
        }

        /// <summary> アニメーション操作 </summary>
        public static void Anime(this Animator ani, string animeName, bool? flag = null) {
            if (flag == null) {
                ani.SetTrigger(animeName);
            }else{
                ani.SetBool(animeName, flag.Value);
            }
        }
        /// <summary> アニメーション操作 </summary>
        public static void Anime(this Animator ani, string animeName, int value) => ani.SetInteger(animeName, value);
        /// <summary> アニメーション操作 </summary>
        public static void Anime(this Animator ani, string animeName, float value) => ani.SetFloat(animeName, value);

        /// <summary> (u) マウスのWorld座標取得 </summary> <param name="offset"> ズレ </param> <returns> マウスWorld座標 </returns>
        public static Vector3 MouseTracking(Vector3? offset = null) {
            Vector3 mousePos = Input.mousePosition;
            mousePos += offset ?? Vec3(0f, 0f, 10f);
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        /// <summary> マウスから出る、光線を取得 </summary>
        public static Ray GetMouseRay { get => Camera.main.ScreenPointToRay(Input.mousePosition); }

        /// <summary> (u) 光線に当たった最初のcastを返す </summary> <param name="distance"> 探索距離 </param> <returns> true:衝突した false:衝突無し </returns> <remark> 【変数.collider】collider、【変数.point】交差点World座標、【変数.distance】衝突までの直線距離 </remark>
        public static bool RayCast(Ray ray, out RaycastHit hit, float distance = 1f) {
            if (IsEditor) Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.red);
            return Physics.Raycast(ray, out hit, distance);
        }
        /// <summary> (u) 光線に当たった全てのcastを配列で返す </summary> <param name="distance"> 探索距離 </param> <returns> true:衝突した false:衝突無し </returns> <remark> 【変数.collider】collider、【変数.point】交差点World座標、【変数.distance】衝突までの直線距離 </remark>
        public static bool RayCast(Ray ray, out RaycastHit[] hits, float distance = 1f) {
            if (IsEditor) Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.red);
            hits = Physics.RaycastAll(ray, distance);
            return 0 < hits.Length;
        }
    
        /// <summary> (3u) 空から見たターゲット座標をMap座標に変換 </summary> <returns> Vector2座標 </returns>
        public static Vector2 PosToMap(GameObject target) {
            return PosToMap(target.GetWorldPos());
        }
        /// <summary> (3u) 空から見たターゲット座標をMap座標に変換 </summary> <returns> Vector2座標 </returns>
        public static Vector2 PosToMap(Vector3 target) {
            return Vec2(target.x, target.z);
        }
    
        /// <summary> 角度を１周単位に直す </summary> <param name="angle"> -180f～180f </param>
        public static float AngleNormalized(this float angle) {
            while (angle >  180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
        /// <summary> 角度を１周単位に直す </summary> <param name="angle"> -180f～180f </param>
        public static Vector3 AngleNormalized(this Vector3 angle) {
            angle.x = angle.x.AngleNormalized();
            angle.y = angle.y.AngleNormalized();
            angle.z = angle.z.AngleNormalized();
            return angle;
        }
        /// <summary> 角度を１周単位に直す </summary> <param name="angle"> -180f～180f </param>
        public static Quaternion AngleNormalized(this Quaternion angle) {
            angle.x = angle.x.AngleNormalized();
            angle.y = angle.y.AngleNormalized();
            angle.z = angle.z.AngleNormalized();
            return angle;
        }

        /// <summary> 範囲内でループ演算 (min == max とし、minを優先する) </summary> <returns> 演算後の値 </returns>
        public static int Normalized(int value, int v1, int v2) {
            // 引数のチェック、必要なら入れ替え
            if (v2 < v1) Swap(ref v1, ref v2);
            // 範囲が一点のみの場合
            if (v1 == v2) return v1;
            // 範囲の長さを計算
            int range = v2 - v1;
            // 範囲内になるまでループ
            while (value < v1)  value += range;
            while (v2 <= value) value -= range;
            // 範囲内になった値を返す
            return value;
        }
        /// <summary> 範囲内でループ演算 (min == max とし、minを優先する) </summary> <returns> 演算後の値 </returns>
        public static float Normalized(float value, float v1, float v2) {
            // 引数のチェック、必要なら入れ替え
            if (v2 < v1) Swap(ref v1, ref v2);
            // 範囲が一点のみの場合
            if (v1 == v2) return v1;
            // 範囲の長さを計算
            float range = v2 - v1;
            // 範囲内になるまでループ
            while (value < v1)  value += range;
            while (v2 <= value) value -= range;
            // 範囲内になった値を返す
            return value;
        }

        /// <summary> (u) floatをターゲットに向けてspeedで移動 </summary> <param name="from"> 開始float </param> <param name="target"> 目標float </param> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> speedでターゲットに近づいた値 </returns>
        public static float TransFloat(float from, float target, float speed, bool delta = true) { return Mathf.MoveTowards(from, target, speed * (delta ? Time.deltaTime : 1f)); }
    
        /// <summary> 回転, 目標度までに必要な最短回転度を返す </summary> <param name="from"> 仮の原点0度 </param> <param name="target"> 目標度 </param> <returns> 最短距離 (-180f～180f) </returns>
        public static float AngleShort(float from, float target) { return Mathf.DeltaAngle(from, target); }
        /// <summary> (u) 回転, 最短で目標度に向けてspeedで移動 </summary> <param name="from"> 仮の原点0度 </param> <param name="target"> 目標度 </param> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> speedでターゲットに近づいた度 </returns>
        public static float AngleTracking(float from, float target, float speed, bool delta = true) {
            float shortest = AngleShort(from, target);
            float angle    = TransFloat(from, from + shortest, speed, delta);
            return angle;
        }
    
        /// <summary> (2) 原点->対象、ベクトルの長さを返す </summary> <param name="target"> 対象座標 </param> <param name="from"> 原点座標 </param> <returns> ベクトルの長さ </returns>
        public static float DirectionToTarget(Vector2 target, Vector2? from = null) {
            from = from ?? Vector2.zero;
            Vector2 direction = target - (Vector2)from;
            float vector = direction.magnitude;
            return vector;
        }
        /// <summary> (3) 原点->対象、ベクトルの長さを返す </summary> <param name="target"> 対象座標 </param> <param name="from"> 原点座標 </param> <returns> ベクトルの長さ </returns>
        public static float DirectionToTarget(Vector3 target, Vector3? from = null) {
            from = from ?? Vector3.zero;
            Vector3 direction = target - (Vector3)from;
            float vector = direction.magnitude;
            return vector;
        }
    
        /// <summary> (2) 原点->対象、角度を返す </summary> <param name="target"> 対象座標 </param> <param name="from"> 原点座標 </param> <returns> 原点から見た角度 </returns>
        public static float AngleToTarget(Vector2 target, Vector2? from = null) {
            from = from ?? Vector2.zero;
            Vector2 direction = target - (Vector2)from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return angle;
        }
        /// <summary> (3) 原点->対象、角度を返す </summary> <param name="target"> 対象座標 </param> <param name="from"> 原点座標 </param> <returns> 原点から見た角度 </returns>
        public static float AngleToTarget(Vector3 target, Vector3? from = null) {
            from = from ?? Vector3.zero;
            Vector3 direction = target - (Vector3)from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return angle;
        }
    
        /// <summary> 移動ベクトルをLocalから対象のWorldに変換 </summary> <param name="localVector"> Localベクトル </param>
        public static Vector3 Direction(this Collision  collision,  Vector3 localVector) { return collision.collider.Direction(localVector); }
        /// <summary> 移動ベクトルをLocalから対象のWorldに変換 </summary> <param name="localVector"> Localベクトル </param>
        public static Vector3 Direction(this Component  component,  Vector3 localVector) { return component.gameObject.Direction(localVector); }
        /// <summary> 移動ベクトルをLocalから対象のWorldに変換 </summary> <param name="localVector"> Localベクトル </param>
        public static Vector3 Direction(this GameObject gameObject, Vector3 localVector) { return gameObject.transform.TransformDirection(localVector); }
    
        /// <summary> 線形補間 (float) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static float Lerp(float from, float to, float value) {
            value = Clamp(value, 0f, 1f);
            return Mathf.Lerp(from, to, value);
        }  
        /// <summary> 線形補間 (Color) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static Color Lerp(Color from, Color to, float value) {
            value = Clamp(value, 0f, 1f);
            return Color.Lerp(from, to, value);
        }
        /// <summary> 線形補間 (Vector2) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static Vector2 Lerp(Vector2 from, Vector2 to, float value) {
            value = Clamp(value, 0f, 1f);
            return Vector2.Lerp(from, to, value);
        }
        /// <summary> 線形補間 (Vector3) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static Vector3 Lerp(Vector3 from, Vector3 to, float value) {
            value = Clamp(value, 0f, 1f);
            return Vector3.Lerp(from, to, value);
        }
        /// <summary> 回転方向を示す </summary>
        public enum LerpMode { Short, Long, Right, Left, }
        /// <summary> 角度補間 (float) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static float LerpAngle(float from, float to, float value, LerpMode lerpMode) {
            from  = from.AngleNormalized();
            to    = to.AngleNormalized();
            if (Mathf.Abs(to - from) < 0.00001f) return from;
            value = Clamp(value, 0f, 1f);
            switch (lerpMode) {
                case LerpMode.Short:
                    return Mathf.LerpAngle(from, to, value);
                case LerpMode.Long:

                    // 内回りの差を求める
                    float abs        = Mathf.Abs(to - from);
                    // 最短回転量を求める
                    float shortAngle = abs <= 180f ? abs : 360f - abs;
                    // 最長回転量を求める
                    float longAngle  = 360f - shortAngle;

                    // 最短が内回りの時
                    if (abs <= 180f) {
                        // 最短が＋回転の時
                        if (from < to) return (from - (longAngle * value)).AngleNormalized();
                        // 最短がー回転の時
                        else return (from + (longAngle * value)).AngleNormalized();
                    }
                    // 最短が外回りの時
                    else {
                        // 最短がー回転の時
                        if (from < to) return (from + (longAngle * value)).AngleNormalized();
                        // 最短が＋回転の時
                        else return (from - (longAngle * value)).AngleNormalized();
                    }

                case LerpMode.Right:
                    if (from < to) {
                        float re = to - from;
                        return from + (re * value);
                    }else {
                        float re = (180f - from) + (to + 180f);
                        re = from + (re * value);
                        if (re <= 180f) return re;
                        return re -360f; 
                    }
                case LerpMode.Left:
                    if (to < from) {
                        float re = from - to;
                        return from - (re * value);
                    }else {
                        float re = (from + 180f) + (180f - to);
                        re = from - (re * value);
                        if (-180f < re) return re;
                        return re + 360f; 
                    }
            }return default;
        }
        /// <summary> 角度補間 (Vector3) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static Vector3 LerpAngle(Vector3 from, Vector3 to, float value, LerpMode lerpMode) {
            Vector3 re = new Vector3();
            re.x = LerpAngle(from.x, to.x, value, lerpMode);
            re.y = LerpAngle(from.y, to.y, value, lerpMode);
            re.z = LerpAngle(from.z, to.z, value, lerpMode);
            return re;
        }
        /// <summary> 角度補間 (Vector3) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static Vector3 LerpAngle(Vector3 from, Vector3 to, float value, (LerpMode, LerpMode, LerpMode) lerpMode) {
            Vector3 re = new Vector3();
            re.x = LerpAngle(from.x, to.x, value, lerpMode.Item1);
            re.y = LerpAngle(from.y, to.y, value, lerpMode.Item2);
            re.z = LerpAngle(from.z, to.z, value, lerpMode.Item3);
            return re;
        }

        /// <summary> 球形補間 (Quaternion) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static Quaternion Slerp(Quaternion from, Quaternion to, float value) {
            value = Clamp(value, 0f, 1f);
            return Quaternion.Slerp(from, to, value);
        }

        /// <summary> 空間補間 (TransValue) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static TransValue Lerp(TransValue from, TransValue to, float value, LerpMode lerpMode = default) {
            value      = Clamp(value, 0f, 1f);
            TransValue result = new TransValue();
            result.pos   = Lerp(from.pos, to.pos, value);
            result.angle = LerpAngle(from.angle, to.angle, value, lerpMode);
            result.scale = Lerp(from.scale, to.scale, value);
            result.size  = Lerp(from.size, to.size, value);
            return result;
        }
        /// <summary> 空間補間 (TransValue) </summary> <remarks> value(0f〜1f)で値間を推移する </remarks>
        public static TransValue Lerp(TransValue from, TransValue to, float value, (LerpMode, LerpMode, LerpMode) lerpMode) {
            value      = Clamp(value, 0f, 1f);
            TransValue result = new TransValue();
            result.pos   = Lerp(from.pos, to.pos, value);
            result.angle = LerpAngle(from.angle, to.angle, value, lerpMode);
            result.scale = Lerp(from.scale, to.scale, value);
            result.size  = Lerp(from.size, to.size, value);
            return result;
        }

        /// <summary> RectTransform に TransValue をローカルで、適用する </summary>
        public static void ToTransform(this RectTransform transform, TransValue value) {
            transform.SetLocalPos(value.pos);
            transform.SetLocalAngle(value.angle);
            transform.SetScale(value.scale);
            transform.SetSize(value.size);
        }
        /// <summary> Transform に TransValue をローカルで、適用する </summary>
        public static void ToTransform(this Transform transform, TransValue value) {
            transform.SetLocalPos(value.pos);
            transform.SetLocalAngle(value.angle);
            transform.SetScale(value.scale);
        }

        /// <summary> RectTransform に TransValue を補間しながら、適用する </summary>
        public static void ToLerp(this RectTransform  rect, TransValue from, TransValue to, float value, LerpMode lerpMode = default) { rect.ToTransform(Lerp(from, to, value, lerpMode)); }
        /// <summary> RectTransform に TransValue を補間しながら、適用する </summary>
        public static void ToLerp(this RectTransform  rect, TransValue from, TransValue to, float value, (LerpMode, LerpMode, LerpMode) lerpMode) { rect.ToTransform(Lerp(from, to, value, lerpMode)); }
        /// <summary> Transform に TransValue を補間しながら、適用する </summary>
        public static void ToLerp(this Transform transform, TransValue from, TransValue to, float value, LerpMode lerpMode = default) { transform.ToTransform(Lerp(from, to, value, lerpMode)); }
        /// <summary> Transform に TransValue を補間しながら、適用する </summary>
        public static void ToLerp(this Transform transform, TransValue from, TransValue to, float value, (LerpMode, LerpMode, LerpMode) lerpMode) { transform.ToTransform(Lerp(from, to, value, lerpMode)); }

        #region Transform　位置 (UIGetL(1), UISetL(2), バラUIsetL(3), 定期(9), GetW(3), SetW(6), バラSetW(9), GetL(4), SetL(8), バラSetL(12))

        /// <summary> UIアンカーの相対座標を取得 </summary>
        public static Vector3 GetAnchorPos(this RectTransform rect, Vector3? speed = null, bool delta = true) {
            Vector3 pos = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, rect.gameObject.GetLocalPos().z);
            if (speed != null) pos += speed.Value * (delta ? Time.deltaTime : 1f);
            return pos; 
        }
        /// <summary> UIアンカーの相対座標を設定 </summary>
        public static void    SetAnchorPos(this RectTransform rect, Vector2 pos) { rect.anchoredPosition = pos; }
        /// <summary> UIのアンカーからの相対座標を設定 </summary>
        public static void    SetAnchorPos(this RectTransform rect, Vector3 pos) { rect.anchoredPosition = pos; }

        /// <summary> UIのアンカーからの相対座標を設定 </summary>
        public static void    AnchorPosX(this RectTransform rect, float x) { rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y); }
        /// <summary> UIのアンカーからの相対座標を設定 </summary>
        public static void    AnchorPosY(this RectTransform rect, float y) { rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y); }
        /// <summary> UIのアンカーからの相対座標を設定 </summary>
        public static void    AnchorPosZ(this RectTransform rect, float z) { rect.gameObject.LocalPosZ(z); }

        /// <summary> (u+) X軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateX(this Collision  collision,  float speed, bool delta = true, bool worldSpace = false) { collision.collider.TranslateX(speed, delta, worldSpace); }
        /// <summary> (u+) X軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateX(this Component  component,  float speed, bool delta = true, bool worldSpace = false) { component.gameObject.TranslateX(speed, delta, worldSpace); }
        /// <summary> (u+) X軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateX(this GameObject gameObject, float speed, bool delta = true, bool worldSpace = false) { if (worldSpace) gameObject.transform.Translate(Vector3.right * speed * (delta ? Time.deltaTime : 1f)); else gameObject.transform.Translate(Vector3.right * speed * (delta ? Time.deltaTime : 1f), Space.World);  }
        /// <summary> (u+) Y軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateY(this Collision  collision,  float speed, bool delta = true, bool worldSpace = false) { collision.collider.TranslateY(speed, delta, worldSpace); }
        /// <summary> (u+) Y軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateY(this Component  component,  float speed, bool delta = true, bool worldSpace = false) { component.gameObject.TranslateY(speed, delta, worldSpace); }
        /// <summary> (u+) Y軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateY(this GameObject gameObject, float speed, bool delta = true, bool worldSpace = false) { if (worldSpace) gameObject.transform.Translate(Vector3.up    * speed * (delta ? Time.deltaTime : 1f)); else gameObject.transform.Translate(Vector3.up * speed * (delta ? Time.deltaTime : 1f), Space.World); }
        /// <summary> (3u+) Z軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateZ(this Collision  collision,  float speed, bool delta = true, bool worldSpace = false) { collision.collider.TranslateZ(speed, delta, worldSpace); }
        /// <summary> (3u+) Z軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateZ(this Component  component,  float speed, bool delta = true, bool worldSpace = false) { component.gameObject.TranslateZ(speed, delta, worldSpace); }
        /// <summary> (3u+) Z軸をspeedで移動 </summary> <param name="speed"> 移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void TranslateZ(this GameObject gameObject, float speed, bool delta = true, bool worldSpace = false) { if (worldSpace) gameObject.transform.Translate(Vector3.forward * speed * (delta ? Time.deltaTime : 1f)); else gameObject.transform.Translate(Vector3.forward * speed * (delta ? Time.deltaTime : 1f), Space.World); }

        /// <summary> ワールド座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetWorldPos(this Collision  collision,  Vector3? speed = null, bool delta = true) { return GetWorldPos(collision.collider, speed, delta); }
        /// <summary> ワールド座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetWorldPos(this Component  component,  Vector3? speed = null, bool delta = true) { return GetWorldPos(component.gameObject, speed, delta); }
        /// <summary> ワールド座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetWorldPos(this GameObject gameObject, Vector3? speed = null, bool delta = true) { 
            Vector3 pos = gameObject.transform.position;
            if (speed != null) pos += speed.Value * (delta ? Time.deltaTime : 1f);
            return pos;
        }
        
        /// <summary> ワールド座標を設定 </summary>
        public static void    SetWorldPos(this Collision  collision,  Vector2 pos) { collision.collider.SetWorldPos(pos); }
        /// <summary> ワールド座標を設定 </summary>
        public static void    SetWorldPos(this Component  component,  Vector2 pos) { component.gameObject.SetWorldPos(pos); }
        /// <summary> ワールド座標を設定 </summary>
        public static void    SetWorldPos(this GameObject gameObject, Vector2 pos) { gameObject.SetWorldPos(new Vector3(pos.x, pos.y, gameObject.transform.position.z)); }
        /// <summary> ワールド座標を設定 </summary>
        public static void    SetWorldPos(this Collision  collision,  Vector3 pos) { collision.collider.SetWorldPos(pos); }
        /// <summary> ワールド座標を設定 </summary>
        public static void    SetWorldPos(this Component  component,  Vector3 pos) { component.gameObject.SetWorldPos(pos); }
        /// <summary> ワールド座標を設定 </summary>
        public static void    SetWorldPos(this GameObject gameObject, Vector3 pos) { gameObject.transform.position = pos; }
    
        /// <summary> ワールドX座標を設定 </summary>
        public static void WorldPosX(this Collision  collision,  float x) { collision.collider.WorldPosX(x); }
        /// <summary> ワールドX座標を設定 </summary>
        public static void WorldPosX(this Component  component,  float x) { component.gameObject.WorldPosX(x); }
        /// <summary> ワールドX座標を設定 </summary>
        public static void WorldPosX(this GameObject gameObject, float x) { gameObject.transform.position = new Vector3(x, gameObject.transform.position.y, gameObject.transform.position.z); }
        /// <summary> ワールドY座標を設定 </summary>
        public static void WorldPosY(this Collision  collision,  float y) { collision.collider.WorldPosY(y); }
        /// <summary> ワールドY座標を設定 </summary>
        public static void WorldPosY(this Component  component,  float y) { component.gameObject.WorldPosY(y); }
        /// <summary> ワールドY座標を設定 </summary>
        public static void WorldPosY(this GameObject gameObject, float y) { gameObject.transform.position = new Vector3(gameObject.transform.position.x, y, gameObject.transform.position.z); }
        /// <summary> ワールドZ座標を設定 </summary>
        public static void WorldPosZ(this Collision  collision,  float z) { collision.collider.WorldPosZ(z); }
        /// <summary> ワールドZ座標を設定 </summary>
        public static void WorldPosZ(this Component  component,  float z) { component.gameObject.WorldPosZ(z); }
        /// <summary> ワールドZ座標を設定 </summary>
        public static void WorldPosZ(this GameObject gameObject, float z) { gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, z); }

        /// <summary> ローカル座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <param name="anchor"> アンカーを考慮するか？ </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalPos(this RectTransform   rect,  Vector3? speed = null, bool delta = true, bool anchor = true) { if (anchor) return rect.GetAnchorPos(speed, delta); else return rect.gameObject.GetLocalPos(speed, delta); }
        /// <summary> ローカル座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalPos(this Collision  collision,  Vector3? speed = null, bool delta = true) { return collision.collider.GetLocalPos(speed, delta); }
        /// <summary> ローカル座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalPos(this Component  component,  Vector3? speed = null, bool delta = true) { return component.gameObject.GetLocalPos(speed, delta); }
        /// <summary> ローカル座標を取得 </summary> <param name="speed"> 仮想の移動速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalPos(this GameObject gameObject, Vector3? speed = null, bool delta = true) {
            Vector3 pos = gameObject.transform.localPosition;
            if (speed != null) pos += speed.Value * (delta ? Time.deltaTime : 1f);
            return pos;
        }
        /// <summary> ローカル座標を設定 </summary> <param name="anchor"> アンカーを考慮するか？ </param> 
        public static void    SetLocalPos(this RectTransform   rect,  Vector2 pos, bool anchor = true) { if (anchor) rect.SetAnchorPos(pos); else rect.gameObject.SetLocalPos(pos); }
        /// <summary> ローカル座標を設定 </summary>
        public static void    SetLocalPos(this Collision  collision,  Vector2 pos) { collision.collider.SetLocalPos(pos); }
        /// <summary> ローカル座標を設定 </summary>
        public static void    SetLocalPos(this Component  component,  Vector2 pos) { component.gameObject.SetLocalPos(pos); }
        /// <summary> ローカル座標を設定 </summary>
        public static void    SetLocalPos(this GameObject gameObject, Vector2 pos) { gameObject.SetLocalPos(new Vector3(pos.x, pos.y, gameObject.transform.localPosition.z)); }
        /// <summary> ローカル座標を設定 </summary> <param name="anchor"> アンカーを考慮するか？ </param> 
        public static void    SetLocalPos(this RectTransform   rect,  Vector3 pos, bool anchor = true) { if (anchor) { rect.SetAnchorPos(pos); rect.LocalPosZ(pos.z); } else rect.gameObject.SetLocalPos(pos); }
        /// <summary> ローカル座標を設定 </summary>
        public static void    SetLocalPos(this Collision  collision,  Vector3 pos) { collision.collider.SetLocalPos(pos); }
        /// <summary> ローカル座標を設定 </summary>
        public static void    SetLocalPos(this Component  component,  Vector3 pos) { component.gameObject.SetLocalPos(pos); }
        /// <summary> ローカル座標を設定 </summary>
        public static void    SetLocalPos(this GameObject gameObject, Vector3 pos) { gameObject.transform.localPosition = pos; }

        /// <summary> ローカルX座標を設定 </summary> <param name="anchor"> アンカーを考慮するか？ </param> 
        public static void LocalPosX(this RectTransform   rect,  float x, bool anchor = true) { if (anchor) rect.AnchorPosX(x); else rect.gameObject.LocalPosX(x); }
        /// <summary> ローカルX座標を設定 </summary>
        public static void LocalPosX(this Collision  collision,  float x) { collision.collider.LocalPosX(x); }
        /// <summary> ローカルX座標を設定 </summary>
        public static void LocalPosX(this Component  component,  float x) { component.gameObject.LocalPosX(x); }
        /// <summary> ローカルX座標を設定 </summary>
        public static void LocalPosX(this GameObject gameObject, float x) { gameObject.transform.localPosition = new Vector3(x, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z); }

        /// <summary> ローカルY座標を設定 </summary> <param name="anchor"> アンカーを考慮するか？ </param> 
        public static void LocalPosY(this RectTransform   rect,  float y, bool anchor = true) { if (anchor) rect.AnchorPosY(y); else rect.gameObject.LocalPosY(y); }
        /// <summary> ローカルY座標を設定 </summary>
        public static void LocalPosY(this Collision  collision,  float y) { collision.collider.LocalPosY(y); }
        /// <summary> ローカルY座標を設定 </summary>
        public static void LocalPosY(this Component  component,  float y) { component.gameObject.LocalPosY(y); }
        /// <summary> ローカルY座標を設定 </summary>
        public static void LocalPosY(this GameObject gameObject, float y) { gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, y, gameObject.transform.localPosition.z); }

        /// <summary> ローカルZ座標を設定 </summary> <param name="anchor"> アンカーを考慮するか？ </param> 
        public static void LocalPosZ(this RectTransform   rect,  float z, bool anchor = true) { if (anchor) rect.AnchorPosZ(z); else rect.gameObject.LocalPosZ(z); }
        /// <summary> ローカルZ座標を設定 </summary>
        public static void LocalPosZ(this Collision  collision,  float z) { collision.collider.LocalPosZ(z); }
        /// <summary> ローカルZ座標を設定 </summary>
        public static void LocalPosZ(this Component  component,  float z) { component.gameObject.LocalPosZ(z); }
        /// <summary> ローカルZ座標を設定 </summary>
        public static void LocalPosZ(this GameObject gameObject, float z) { gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, z); }

        #endregion

        #region Transform　回転 (定期(9), GetW(3), SetW(6), バラSetW(9), GetL(4), SetL(8), バラSetL(12))

        /// <summary> (u+) X軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateX(this Collision  collision,  float speed, bool delta = true, bool worldSpace = false) { collision.collider.RotateX(speed, delta, worldSpace); }
        /// <summary> (u+) X軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateX(this Component  component,  float speed, bool delta = true, bool worldSpace = false) { component.gameObject.RotateX(speed, delta, worldSpace); }
        /// <summary> (u+) X軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateX(this GameObject gameObject, float speed, bool delta = true, bool worldSpace = false) { if (worldSpace) gameObject.transform.Rotate(Vector3.right * speed * (delta ? Time.deltaTime : 1f)); else gameObject.transform.Rotate(Vector3.right * speed * (delta ? Time.deltaTime : 1f), Space.World); }
        /// <summary> (u+) Y軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateY(this Collision  collision,  float speed, bool delta = true, bool worldSpace = false) { collision.collider.RotateY(speed, delta, worldSpace); }
        /// <summary> (u+) Y軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateY(this Component  component,  float speed, bool delta = true, bool worldSpace = false) { component.gameObject.RotateY(speed, delta, worldSpace); }
        /// <summary> (u+) Y軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateY(this GameObject gameObject, float speed, bool delta = true, bool worldSpace = false) { if (worldSpace) gameObject.transform.Rotate(Vector3.up    * speed * (delta ? Time.deltaTime : 1f)); else gameObject.transform.Rotate(Vector3.up    * speed * (delta ? Time.deltaTime : 1f), Space.World); }
        /// <summary> (u+) Z軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateZ(this Collision  collision,  float speed, bool delta = true, bool worldSpace = false) { collision.collider.RotateZ(speed, delta, worldSpace); }
        /// <summary> (u+) Z軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateZ(this Component  component,  float speed, bool delta = true, bool worldSpace = false) { component.gameObject.RotateZ(speed, delta, worldSpace); }
        /// <summary> (u+) Z軸をspeedで回転 </summary> <param name="speed"> 回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void RotateZ(this GameObject gameObject, float speed, bool delta = true, bool worldSpace = false) { if (worldSpace) gameObject.transform.Rotate(Vector3.forward * speed * (delta ? Time.deltaTime : 1f)); else gameObject.transform.Rotate(Vector3.forward * speed * (delta ? Time.deltaTime : 1f), Space.World); }
    
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetWorldAngle(this Collision  collision,  Vector3? speed = null, bool delta = true) { return collision.collider.GetWorldAngle(speed, delta); }
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetWorldAngle(this Component  component,  Vector3? speed = null, bool delta = true) { return component.gameObject.GetWorldAngle(speed, delta); }
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の回転を取得 </remarks>
        public static Vector3 GetWorldAngle(this GameObject gameObject, Vector3? speed = null, bool delta = true) {
            Vector3 angle = gameObject.transform.eulerAngles;
            if (speed != null) angle += speed.Value * (delta ? Time.deltaTime : 1f);
            angle = AngleNormalized(angle);
            return angle;
        }
        /// <summary> 回転を設定 </summary>
        public static void    SetWorldAngle(this Collision  collision,  Vector2 angle) { collision.collider.SetWorldAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetWorldAngle(this Component  component,  Vector2 angle) { component.gameObject.SetWorldAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetWorldAngle(this GameObject gameObject, Vector2 angle) { gameObject.SetWorldAngle(AngleNormalized(new Vector3(angle.x, angle.y, gameObject.transform.eulerAngles.z))); }
        /// <summary> 回転を設定 </summary>
        public static void    SetWorldAngle(this Collision  collision,  Vector3 angle) { collision.collider.SetWorldAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetWorldAngle(this Component  component,  Vector3 angle) { component.gameObject.SetWorldAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetWorldAngle(this GameObject gameObject, Vector3 angle) { gameObject.transform.rotation = Quaternion.Euler(angle).AngleNormalized(); }
    
        /// <summary> 回転X軸を設定 </summary>
        public static void WorldAngleX(this Collision  collision,  float x) { collision.collider.WorldAngleX(x); }
        /// <summary> 回転X軸を設定 </summary>
        public static void WorldAngleX(this Component  component,  float x) { component.gameObject.WorldAngleX(x); }
        /// <summary> 回転X軸を設定 </summary>
        public static void WorldAngleX(this GameObject gameObject, float x) { gameObject.transform.rotation = Quaternion.Euler(x, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z).AngleNormalized(); }
        /// <summary> 回転Y軸を設定 </summary>
        public static void WorldAngleY(this Collision  collision,  float y) { collision.collider.WorldAngleY(y); }
        /// <summary> 回転Y軸を設定 </summary>
        public static void WorldAngleY(this Component  component,  float y) { component.gameObject.WorldAngleY(y); }
        /// <summary> 回転Y軸を設定 </summary>
        public static void WorldAngleY(this GameObject gameObject, float y) { gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.eulerAngles.x, y, gameObject.transform.eulerAngles.z).AngleNormalized(); }
        /// <summary> 回転Z軸を設定 </summary>
        public static void WorldAngleZ(this Collision  collision,  float z) { collision.collider.WorldAngleZ(z); }
        /// <summary> 回転Z軸を設定 </summary>
        public static void WorldAngleZ(this Component  component,  float z) { component.gameObject.WorldAngleZ(z); }
        /// <summary> 回転Z軸を設定 </summary>
        public static void WorldAngleZ(this GameObject gameObject, float z) { gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y,z).AngleNormalized(); }
         
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalAngle(this RectTransform rect, Vector3? speed = null, bool delta = true) {
            Vector3 angle = rect.localEulerAngles;
            if (speed != null) angle += speed.Value * (delta ? Time.deltaTime : 1f);
            angle = AngleNormalized(angle);
            return angle;
        }
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalAngle(this Collision  collision,  Vector3? speed = null, bool delta = true) { return collision.collider.GetLocalAngle(speed, delta); }
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetLocalAngle(this Component  component,  Vector3? speed = null, bool delta = true) { return component.gameObject.GetLocalAngle(speed, delta); }
        /// <summary> 回転を取得 </summary> <param name="speed"> 仮想の回転速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の回転を取得 </remarks>
        public static Vector3 GetLocalAngle(this GameObject gameObject, Vector3? speed = null, bool delta = true) {
            Vector3 angle = gameObject.transform.localEulerAngles;
            if (speed != null) angle += speed.Value * (delta ? Time.deltaTime : 1f);
            angle = AngleNormalized(angle);
            return angle;
        }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this RectTransform   rect,  Vector2 angle) { rect.gameObject.SetLocalAngle(new Vector3(angle.x, angle.y, rect.localEulerAngles.z)); }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this Collision  collision,  Vector2 angle) { collision.collider.SetLocalAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this Component  component,  Vector2 angle) { component.gameObject.SetLocalAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this GameObject gameObject, Vector2 angle) { gameObject.SetLocalAngle(new Vector3(angle.x, angle.y, gameObject.transform.localEulerAngles.z)); }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this RectTransform   rect,  Vector3 angle) { rect.localEulerAngles = angle; }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this Collision  collision,  Vector3 angle) { collision.collider.SetLocalAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this Component  component,  Vector3 angle) { component.gameObject.SetLocalAngle(angle); }
        /// <summary> 回転を設定 </summary>
        public static void    SetLocalAngle(this GameObject gameObject, Vector3 angle) { gameObject.transform.localRotation = Quaternion.Euler(angle).AngleNormalized(); }

        /// <summary> 回転X軸を設定 </summary>
        public static void LocalAngleX(this RectTransform   rect,  float x) { rect.localEulerAngles = new Vector3(x.AngleNormalized(), rect.localEulerAngles.y, rect.localEulerAngles.z); }
        /// <summary> 回転X軸を設定 </summary>
        public static void LocalAngleX(this Collision  collision,  float x) { collision.collider.LocalAngleX(x); }
        /// <summary> 回転X軸を設定 </summary>
        public static void LocalAngleX(this Component  component,  float x) { component.gameObject.LocalAngleX(x); }
        /// <summary> 回転X軸を設定 </summary>
        public static void LocalAngleX(this GameObject gameObject, float x) { gameObject.transform.localRotation = Quaternion.Euler(x, gameObject.transform.localEulerAngles.y, gameObject.transform.localEulerAngles.z).AngleNormalized(); }

        /// <summary> 回転Y軸を設定 </summary>
        public static void LocalAngleY(this RectTransform   rect,  float y) { rect.localEulerAngles = new Vector3(rect.localEulerAngles.x, y.AngleNormalized(), rect.localEulerAngles.z); }
        /// <summary> 回転Y軸を設定 </summary>
        public static void LocalAngleY(this Collision  collision,  float y) { collision.collider.LocalAngleY(y); }
        /// <summary> 回転Y軸を設定 </summary>
        public static void LocalAngleY(this Component  component,  float y) { component.gameObject.LocalAngleY(y); }
        /// <summary> 回転Y軸を設定 </summary>
        public static void LocalAngleY(this GameObject gameObject, float y) { gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localEulerAngles.x, y, gameObject.transform.localEulerAngles.z).AngleNormalized(); }

        /// <summary> 回転Z軸を設定 </summary>
        public static void LocalAngleZ(this RectTransform   rect,  float z) { rect.localEulerAngles = new Vector3(rect.localEulerAngles.x, rect.localEulerAngles.y, z.AngleNormalized()); }
        /// <summary> 回転Z軸を設定 </summary>
        public static void LocalAngleZ(this Collision  collision,  float z) { collision.collider.LocalAngleZ(z); }
        /// <summary> 回転Z軸を設定 </summary>
        public static void LocalAngleZ(this Component  component,  float z) { component.gameObject.LocalAngleZ(z); }
        /// <summary> 回転Z軸を設定 </summary>
        public static void LocalAngleZ(this GameObject gameObject, float z) { gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localEulerAngles.x, gameObject.transform.localEulerAngles.y, z).AngleNormalized(); }

        #endregion

        #region Transform　スケール

        /// <summary> (u) 拡大率をspeedで変更 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <returns> PosでいうTranslate </returns>
        public static void Scalete(this Transform transform, Vector3 speed, bool delta = true) { transform.localScale += speed * (delta ? Time.deltaTime : 1f); }
    
        /// <summary> (u+) X軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteX(this Collision  collision,  float speed, bool delta = true) { collision.collider.ScaleteX(speed, delta); }
        /// <summary> (u+) X軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteX(this Component  component,  float speed, bool delta = true) { component.gameObject.ScaleteX(speed, delta); }
        /// <summary> (u+) X軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteX(this GameObject gameObject, float speed, bool delta = true) { gameObject.transform.Scalete(Vector3.right * speed * (delta ? Time.deltaTime : 1f)); }
        /// <summary> (u+) Y軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteY(this Collision  collision,  float speed, bool delta = true) { collision.collider.ScaleteY(speed, delta); }
        /// <summary> (u+) Y軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteY(this Component  component,  float speed, bool delta = true) { component.gameObject.ScaleteY(speed, delta); }
        /// <summary> (u+) Y軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteY(this GameObject gameObject, float speed, bool delta = true) { gameObject.transform.Scalete(Vector3.up    * speed * (delta ? Time.deltaTime : 1f)); }
        /// <summary> (3u+) Z軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteZ(this Collision  collision,  float speed, bool delta = true) { collision.collider.ScaleteZ(speed, delta); }
        /// <summary> (3u+) Z軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteZ(this Component  component,  float speed, bool delta = true) { component.gameObject.ScaleteZ(speed, delta); }
        /// <summary> (3u+) Z軸をspeedで拡大縮小 </summary> <param name="speed"> 拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param>
        public static void ScaleteZ(this GameObject gameObject, float speed, bool delta = true) { gameObject.transform.Scalete(Vector3.forward * speed * (delta ? Time.deltaTime : 1f)); }
    
        /// <summary> 拡大率を取得 </summary> <param name="speed"> 仮想の拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetScale(this Collision  collision,  Vector3? speed = null, bool delta = true) { return GetScale(collision.collider, speed, delta); }
        /// <summary> 拡大率を取得 </summary> <param name="speed"> 仮想の拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetScale(this Component  component,  Vector3? speed = null, bool delta = true) { return GetScale(component.gameObject, speed, delta); }
        /// <summary> 拡大率を取得 </summary> <param name="speed"> 仮想の拡大率変更速度 </param> <param name="delta"> true: * delta , false: * 1f </param> <remarks> 引数を入れると1フレーム先の位置を取得 </remarks>
        public static Vector3 GetScale(this GameObject gameObject, Vector3? speed = null, bool delta = true) {
            Vector3 scale = gameObject.transform.localScale;
            if (speed != null) scale += speed.Value * (delta ? Time.deltaTime : 1f);
            return scale;
        }
        /// <summary> 拡大率を設定 </summary>
        public static void    SetScale(this Collision  collision,  Vector2 scale) { collision.collider.SetScale(scale); }
        /// <summary> 拡大率を設定 </summary>
        public static void    SetScale(this Component  component,  Vector2 scale) { component.gameObject.SetScale(scale); }
        /// <summary> 拡大率を設定 </summary>
        public static void    SetScale(this GameObject gameObject, Vector2 scale) { gameObject.SetScale(new Vector3(scale.x, scale.y, gameObject.transform.localScale.z)); }
        /// <summary> 拡大率を設定 </summary>
        public static void    SetScale(this Collision  collision,  Vector3 scale) { collision.collider.SetScale(scale); }
        /// <summary> 拡大率を設定 </summary>
        public static void    SetScale(this Component  component,  Vector3 scale) { component.gameObject.SetScale(scale); }
        /// <summary> 拡大率を設定 </summary>
        public static void    SetScale(this GameObject gameObject, Vector3 scale) { gameObject.transform.localScale = scale; }
    
        /// <summary> 拡大率X軸を設定 </summary>
        public static void ScaleX(this Collision  collision,  float x) { collision.collider.ScaleX(x); }
        /// <summary> 拡大率X軸を設定 </summary>
        public static void ScaleX(this Component  component,  float x) { component.gameObject.ScaleX(x); }
        /// <summary> 拡大率X軸を設定 </summary>
        public static void ScaleX(this GameObject gameObject, float x) { gameObject.transform.localScale = new Vector3(x, gameObject.transform.localScale.y, gameObject.transform.localScale.z); }
        /// <summary> 拡大率Y軸を設定 </summary>
        public static void ScaleY(this Collision  collision,  float y) { collision.collider.ScaleY(y); }
        /// <summary> 拡大率Y軸を設定 </summary>
        public static void ScaleY(this Component  component,  float y) { component.gameObject.ScaleY(y); }
        /// <summary> 拡大率Y軸を設定 </summary>
        public static void ScaleY(this GameObject gameObject, float y) { gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, y, gameObject.transform.localScale.z); }
        /// <summary> 拡大率Z軸を設定 </summary>
        public static void ScaleZ(this Collision  collision,  float z) { collision.collider.ScaleZ(z); }
        /// <summary> 拡大率Z軸を設定 </summary>
        public static void ScaleZ(this Component  component,  float z) { component.gameObject.ScaleZ(z); }
        /// <summary> 拡大率Z軸を設定 </summary>
        public static void ScaleZ(this GameObject gameObject, float z) { gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, z); }
    
        /// <summary> UIのサイズを取得 </summary>
        public static Vector2 GetSize( this RectTransform rect) { return rect.sizeDelta; }
        /// <summary> UIのサイズを設定 </summary>
        public static void    SetSize( this RectTransform rect, Vector2 size) { rect.sizeDelta = size; }
        /// <summary> UIのX軸サイズを設定 </summary>
        public static void    SetSizeX(this RectTransform rect, float x) { rect.sizeDelta = new Vector2(x, rect.sizeDelta.y); }
        /// <summary> UIのY軸サイズを設定 </summary>
        public static void    SetSizeY(this RectTransform rect, float y) { rect.sizeDelta = new Vector2(rect.sizeDelta.x, y); }

        #endregion

        /// <summary> UIの サイズ * スケール を取得 </summary>
        public static Vector2 GetRatioUI(this RectTransform rect) { return rect.sizeDelta * rect.GetScale(); }

        /// <summary> 親Object取得 </summary>
        public static GameObject GetParent(this Collision child) { return child.collider.gameObject.GetParent(); }
        /// <summary> 親Object取得 </summary>
        public static GameObject GetParent(this Component child) { return child.gameObject.GetParent(); }
        /// <summary> 親Object取得 </summary>
        public static GameObject GetParent(this GameObject child) {
            if (child.transform.parent == null) return null;
            return child.transform.parent.gameObject;
        }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this Collision  child, Collision  parent) { child.collider.gameObject.transform.SetParent(parent.collider.gameObject.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this Collision  child, Component  parent) { child.collider.gameObject.transform.SetParent(parent.gameObject.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this Collision  child, GameObject parent) { child.collider.gameObject.transform.SetParent(parent.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this Component  child, Collision  parent) { child.gameObject.transform.SetParent(parent.collider.gameObject.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this Component  child, Component  parent) { child.gameObject.transform.SetParent(parent.gameObject.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this Component  child, GameObject parent) { child.gameObject.transform.SetParent(parent.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this GameObject child, Collision  parent) { child.transform.SetParent(parent.collider.gameObject.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this GameObject child, Component  parent) { child.transform.SetParent(parent.gameObject.transform); }
        /// <summary> 親子関係を設定する </summary> <param name="parent"> 親要素 </param>
        public static void SetParent(this GameObject child, GameObject parent) { child.transform.SetParent(parent.transform); }

        /// <summary> リスト内の順序を変更する </summary>
        public static void Order<T>(this List<T> list, int oldIndex, int? newIndex = null) {
            int count = list.Count;
            // nullなら最後尾を適応
            if (newIndex == null) { newIndex = count; }
            // リスト外参照チェック
            if (oldIndex < 0 || oldIndex >= count || newIndex < 0 || newIndex > count) {
                Debug.LogWarning("リスト外参照");
                return;
            }
            T temp = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (oldIndex < newIndex) newIndex -= 1;
            list.Insert(newIndex.Value, temp);
        }

        /// <summary> 子オブジェクトの順列を変更する </summary> <param name="oldIndex"> 今の順列 (index) </param> <param name="newIndex"> 新しい順列 (index) </param>
        public static void SetChildOrder(this Transform parent, int oldIndex, int? newIndex = null) {
            int count = parent.childCount;
            // nullなら最後尾を適応
            if (newIndex == null) { newIndex = count; }
            // 配列外参照チェック
            if (oldIndex < 0 || oldIndex >= count || newIndex < 0 || newIndex > count) {
                Debug.LogWarning("配列外参照 (子オブジェクト指定)");
                return;
            }
            Transform child = parent.GetChild(oldIndex);
            child.SetSiblingIndex(newIndex.Value);
            // SetAsLastSibling()でも最後尾はできる
        }

        /// <summary> 要素達を新しい1つの配列にまとめて返す </summary>
        public static T[] Params<T>(T first, params T[] array) {
            T[] re = new T[(array != null ? (array.Length + 1) : 1)];
            for(int i = 0; i < re.Length; i++) {
                if (i == 0) re[i] = first;
                else re[i] = array[i - 1];
            }return re;
        }
        /// <summary> 要素達を新しい1つの配列にまとめて返す </summary>
        public static T[] Params<T>(T[] first, params T[][] array) {
            List<T> list = new List<T>();
            for(int i = 0; i < first.Length; i++) list.Add(first[i]);
            for(int i = 0; i < array.Length; i++) for(int j = 0; j < array[i].Length; j++) list.Add(array[i][j]);
            return list.ToArray();
        }

        /// <summary> UIのAlpha値をいじる </summary> <param name="alpha"> 透明度0f〜1f </param>
        public static void Alpha(this Graphic graphic, float alpha) {
            alpha = Clamp(alpha, 0f, 1f);
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
        }
    
        /// <summary> 画像をFill </summary>
        public static void Fill(this Image image, float percent) { image.fillAmount = percent; }
        /// <summary> 画像をFill </summary>
        public static void Fill(this Image image, float range, float now) { Fill(image, now / range); }
    
        /// <summary> トグル押下実行を設定(新規) </summary>
        public static void ClickAction(this Toggle toggle, UnityAction<bool> action) {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(delegate(bool value) { action(value); ClickExit(); });
        }
        /// <summary> ボタン押下実行を設定(新規) </summary>
        public static void ClickAction(this Button button, UnityAction action, bool taskRemove = true) {
            if (taskRemove) button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { action(); ClickExit(); });
        }
        /// <summary> 入力欄書換時実行を設定(新規) </summary>
        public static void InputAction(this TMP_InputField inputField, UnityAction action) {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(delegate (string text) { inputField.TextFilter(); action(); });
        }
        /// <summary> クリック後フォーカスを外す </summary>
        static void ClickExit() { EventSystem.current.SetSelectedGameObject(null); }
        /// <summary> ボタンの有効化,無効化 </summary>
        public static void Set(this Button button, bool flag) { button.interactable = flag; }
        /// <summary> ボタンの文字を変更 </summary> <returns> true:完了 , false:文字枠がない </returns>
        public static bool Text(this Button button, string text = "") {
            TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) {
                tmp.text = text;
                return true;
            }else {
                Debug.LogWarning("ボタンの文字枠がない");
                return false;
            }
        }
        /// <summary> 入力欄内の文字数を返す </summary>
        public static int Length(this TMP_InputField inputField) { return inputField.text.Length; }
        /// <summary> 入力欄NGワードフィルター </summary>
        static void TextFilter(this TMP_InputField inputField) {
            string[] ngWord = { "\r", "\n", " " };
            string input = inputField.text;
            foreach (string ng in ngWord) {
                input = input.Delete(ng);
            }inputField.text = input;
        }

        /// <summary> 複数のコルーチン同時実行 </summary>
        public static Coroutine[] StartCoroutines(this MonoBehaviour mono, IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            Coroutine[]   re = new Coroutine[cor.Length];
            for (int i = 0; i < re.Length; i++) {
                re[i] = mono.StartCoroutine(cor[i]);
            }return re;
        }
        /// <summary> 複数のコルーチン同時実行 </summary>
        public static Coroutine[] StartCoroutines(this MonoBehaviour mono, IEnumerator[] coroutines) {
            Coroutine[]   cor = new Coroutine[coroutines.Length];
            for (int i = 0; i < cor.Length; i++) {
                cor[i] = mono.StartCoroutine(coroutines[i]);
            }return cor;
        }

        /// <summary> コルーチンの後実行処理を追加 </summary>
        public static IEnumerator StuckCoroutine(IEnumerator coroutine, Action action) { if (coroutine != null) yield return coroutine; yield return null; if (action != null) action(); }

        /// <summary> 引数のコルーチンを1つずつ終わらせる </summary>
        public static IEnumerator QueueCoroutine(IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            for (int i = 0; i < cor.Length; i++) {
                if (cor[i] != null) yield return cor[i];
            }
        }

        /// <summary> Coroutineを同時に実行し、どれか終了するまで待機 </summary>
        public static IEnumerator AnyWait(this MonoBehaviour mono, IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            bool[]      isEnd = new bool[cor.Length];
            cor.For(i => {
                mono.StartCoroutine(StuckCoroutine(cor[i], () => isEnd[i] = true));
            });
            yield return new WaitUntil(() => isEnd.Contains(true));
        }
        /// <summary> Coroutineを同時に実行し、どれか終了するまで待機 </summary>
        public static IEnumerator AnyWait(this MonoBehaviour mono, IEnumerator[] coroutines) {
            bool[] isEnd = new bool[coroutines.Length];
            coroutines.For(i => { mono.StartCoroutine(StuckCoroutine(coroutines[i], () => isEnd[i] = true)); });
            yield return new WaitUntil(() => isEnd.Contains(true));
        }

        /// <summary> Coroutineを同時に実行し、全て終了するまで待機 </summary>
        public static IEnumerator AllWait(this MonoBehaviour mono, IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            bool[]      isEnd = new bool[cor.Length];
            cor.For(i => {
                mono.StartCoroutine(StuckCoroutine(cor[i], () => isEnd[i] = true));
            });
            yield return new WaitUntil(() => !isEnd.Contains(false));
        }
        /// <summary> Coroutineを同時に実行し、全て終了するまで待機 </summary>
        public static IEnumerator AllWait(this MonoBehaviour mono, IEnumerator[] coroutines) {
            bool[] isEnd = new bool[coroutines.Length];
            coroutines.For(i => { mono.StartCoroutine(StuckCoroutine(coroutines[i], () => isEnd[i] = true)); });
            yield return new WaitUntil(() => !isEnd.Contains(false));
        }

        /// <summary> Lerp補間を時間で管理する </summary> <param name="LerpAction"> ラムダ式の引数はvalue </param> <param name="time"> 完了までの時間、delta必要なし </param> <param name="from"> 開始位置 </param> <param name="to"> 終了位置 </param>
        public static IEnumerator TimeLerp(Action<float> LerpAction, float time = 1f, float from = 0f, float to = 1f) {
            if (LerpAction == null) yield break;
            if (time <= 0f) {
                LerpAction(to);
                yield break;
            }
            float timer = 0f;
            while (timer < time) {
                LerpAction(Lerp(from, to, timer / time));
                timer += Time.deltaTime;
                yield return null;
            }LerpAction(to);
        }

        /// <summary> (c) Fade(透明度) </summary> <param name="component"> 対象 </param> <param name="x"> 開始alpha(0f-1f) </param> <param name="y"> 終了alpha(0f-1f) </param> <param name="fadeTime"> 一連の所要時間 </param>
        public static IEnumerator Fade(Component component, float startAlpha, float endAlpha, float fadeTime) {
    
            // 適正値に抑える
            startAlpha = Mathf.Clamp(startAlpha, 0f, 1f);
            endAlpha   = Mathf.Clamp(endAlpha,   0f, 1f);
    
            // いじって代入する用の変数
            Color targetCopy;
            // それぞれの色を戻すメソッドの格納
            Action<Color> setColor = null;
    
            // 型に合わせた処理
            if (component is Graphic graphic) {
                targetCopy = graphic.color;
                setColor = newColor => graphic.color = newColor;
            }else if (component is SpriteRenderer spriteRenderer) {
                targetCopy = spriteRenderer.color;
                setColor = newColor => spriteRenderer.color = newColor;
            }else {
                Debug.LogError("対象はフェード不可");
                yield break;
            }
    
            // 所要時間が0秒以下なら即変更
            if (fadeTime <= 0f) {
                targetCopy.a  = endAlpha;
                setColor?.Invoke(targetCopy);
                yield break;
            }
    
            // Fade処理の共通ロジック
            float counter = 0;
            while (counter < fadeTime) {
                if (component == null) yield break;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, counter / fadeTime);
                targetCopy.a = alpha;
                setColor?.Invoke(targetCopy);
                counter += Time.deltaTime;
                yield return null;
            }
    
            // 確実に終了値に設定
            if (component != null) {
                targetCopy.a = endAlpha;
                setColor?.Invoke(targetCopy);
            }
            
        }
    
        /// <summary> 条件の後に実行 </summary>
        public static IEnumerator WaitCor(float seconds, Action action) {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
        /// <summary> 指定秒後に実行 </summary>
        public static IEnumerator WaitCor(Func<bool> condition, Action action) {
            yield return new WaitUntil(condition);
            action?.Invoke();
        }

        /// <summary> Resourcesフォルダからの相対パスによりAssets内を参照する </summary>
        public static T GetResources<T>(string path) where T : Object {
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/Resources")) {
                Debug.LogWarning("Resourcesフォルダが無い為、作成します");
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
#endif
            T reso = Resources.Load<T>(path);
            if (reso == null) Debug.LogWarning("参照Error");
            return reso;
        }

#if UNITY_EDITOR

        /// <summary> コンソールをまっさらにする </summary>
        public static void ConsoleClear() {
            EditorApplication.ExecuteMenuItem("Window/General/Console");
            EditorApplication.ExecuteMenuItem("Edit/Clear");
        }

        /// <summary> Hierarchyにて最後に選んだ選択中のObjectを返す </summary>
        public static GameObject   HierarchySelect  { get => Selection.activeGameObject; }
    
        /// <summary> Hierarchyにて選択中の全てのObjectを返す </summary>
        public static GameObject[] HierarchySelects { get => Selection.gameObjects; }
    
        /// <summary> Hierarchyの選択状況を返す (true:単数 or なし , false:複数) </summary>
        public static bool IsHierarchySelection { get => HierarchySelects.Length <= 1; }

        /// <summary> GUIラベル作成 </summary>
        public static GUIStyle CreateStyle(GUIStyle def = null, bool center = false, bool bold = false, Color? color = null) {
            GUIStyle style = def != null ? def : new GUIStyle();
            if (    center    ) style.alignment        = TextAnchor.MiddleCenter;
            if (     bold     ) style.fontStyle        = FontStyle.Bold;
            if (color.HasValue) style.normal.textColor = color.Value;
            return style;
        }

        // === EditorField_overload ===
        public static bool          Button(string text,                       GUIStyle style = null     ) { if (style == null) return GUILayout.           Button(text       ); else return GUILayout.           Button(text,        style); }
        public static void           Field(string text,                       GUIStyle style = null     ) { if (style == null)        GUILayout.            Label(text       ); else        GUILayout.            Label(text,        style); }
        public static void           Field(Rect   rect, string         text,  GUIStyle style = null     ) { if (style == null)        EditorGUI.       LabelField(rect, text ); else        EditorGUI.       LabelField(rect, text,  style); }
        public static int            Field(string text, int            value, GUIStyle style = null     ) { if (style == null) return EditorGUILayout.   IntField(text, value); else return EditorGUILayout.   IntField(text, value, style); }
        public static float          Field(string text, float          value, GUIStyle style = null     ) { if (style == null) return EditorGUILayout. FloatField(text, value); else return EditorGUILayout. FloatField(text, value, style); }
        public static double         Field(string text, double         value, GUIStyle style = null     ) { if (style == null) return EditorGUILayout.DoubleField(text, value); else return EditorGUILayout.DoubleField(text, value, style); }
        public static string         Field(string text, string         value, GUIStyle style = null     ) { if (style == null) return EditorGUILayout.  TextField(text, value); else return EditorGUILayout.  TextField(text, value, style); }
        public static void           Field(                                                             ) {        EditorGUILayout.          Space(                             ); }
        public static bool           Field(string text, bool           value                            ) { return EditorGUILayout.         Toggle(text, value                  ); }
        public static Color          Field(string text, Color          value                            ) { return EditorGUILayout.     ColorField(text, value                  ); }
        public static Vector2        Field(string text, Vector2        value                            ) { return EditorGUILayout.   Vector2Field(text, value                  ); }
        public static Vector2Int     Field(string text, Vector2Int     value                            ) { return EditorGUILayout.Vector2IntField(text, value                  ); }
        public static Vector3        Field(string text, Vector3        value                            ) { return EditorGUILayout.   Vector3Field(text, value                  ); }
        public static Vector3Int     Field(string text, Vector3Int     value                            ) { return EditorGUILayout.Vector3IntField(text, value                  ); }
        public static Vector4        Field(string text, Vector4        value                            ) { return EditorGUILayout.   Vector4Field(text, value                  ); }
        public static LayerMask      Field(string text, LayerMask      value                            ) { return EditorGUILayout.     LayerField(text, value                  ); }
        public static Enum           Field(string text, Enum           value                            ) { return EditorGUILayout.      EnumPopup(text, value                  ); }
        public static Object         Field(string text, Object         value, Type type, bool allowScene) { return EditorGUILayout.    ObjectField(text, value, type, allowScene); }
        public static AnimationCurve Field(string text, AnimationCurve value                            ) { return EditorGUILayout.     CurveField(text, value                  ); }
        public static Bounds         Field(string text, Bounds         value                            ) { return EditorGUILayout.    BoundsField(text, value                  ); }
        public static Rect           Field(string text, Rect           value                            ) { return EditorGUILayout.      RectField(text, value                  ); }
        public static long           Field(string text, long           value                            ) { return EditorGUILayout.      LongField(text, value                  ); }
        public static Gradient       Field(string text, Gradient       value                            ) { return EditorGUILayout.  GradientField(text, value                  ); }
        public static void           Field(Rect   rect, SerializedProperty property, GUIContent label, bool includeChildren = true) { EditorGUI.PropertyField(rect, property, label,                 includeChildren); }
        public static void           Field(Rect   rect, SerializedProperty property, string     label, bool includeChildren = true) { EditorGUI.PropertyField(rect, property, new GUIContent(label), includeChildren); }

#endif

#if Pun2

        /// <summary> サーバーに接続 </summary>
        public static void ServerLogin() { PhotonNetwork.ConnectUsingSettings(); }

        /// <summary> 今ロビーにいるか？を返す </summary>
        public static bool InLobby { get => PhotonNetwork.InLobby; }

        /// <summary> ロビーにログイン </summary>
        public static void LobbyLogin() { PhotonNetwork.JoinLobby(); }

        /// <summary> ロビーから抜ける </summary>
        public static void OutLobby() { PhotonNetwork.LeaveLobby(); }

        /// <summary> 今ルームにいるか？を返す </summary>
        public static bool InRoom { get => PhotonNetwork.InRoom; }

        /// <summary> ルームに参加または作成 </summary>
        public static void RoomLogin(int roomID, Room newRoom) {

            string roomName = $"{newRoom.name}({roomID})";

            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = newRoom.maxPlayer;

            TypedLobby  lobby   = TypedLobby.Default;
            
            PhotonNetwork.JoinOrCreateRoom(roomName, options, lobby);
        }

        /// <summary> 接続中のルーム名を取得 (接続なしで空文字を返す) </summary>
        public static string RoomName { get => InRoom ? PhotonNetwork.CurrentRoom.Name : ""; }

        /// <summary> 接続中のルームのMax人数を取得 (接続なしで-1を返す) </summary>
        public static int MaxPlayer { get => InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : -1; }

        /// <summary> 接続中のルームNow人数を取得 (接続なしで-1を返す) </summary>
        public static int PlayerCount { get => InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : -1; }

        /// <summary> 接続中のルームの何番目か？を取得 １～カウント (接続なしで-1を返す) </summary>
        public static int ActorNumber { get => InRoom ? PhotonNetwork.LocalPlayer.ActorNumber : -1; }

        /// <summary> ルームから抜ける </summary>
        public static void OutRoom() { if (InRoom) PhotonNetwork.LeaveRoom(); }
    
#endif

    }

    // ===== GameData =====       // Scriptable:ゲームデータ
    // [CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObject/GameData", order = 1)]
    public class GameData : ScriptableObject {
    
    }

#if UNITY_EDITOR

    // ===== EditorWindow =====   // 新規Windowカスタム
    public class BaseEW : EditorWindow {
    
        // ラベルスタイル (OnEnable使用)
        protected List<GUIStyle> LabelStyle;

        // スクロールバー調整用
        protected Vector2 scroll;

        // === 新規Window作成 ===
        [MenuItem("(´・ω・)/Support")]
        public static void BaseWindow() {
            GetWindow<BaseEW>("Support").Show();
        }
    
        // === 追加Window ===
        protected void AddWindowButton<T>(string text = null, GUIStyle style = null) where T : EditorWindow {
            text = text ?? typeof(T).Name;
            style = style ?? LabelStyle[0];
            if (Button(text, style)) AddWindow<T>();
        }
        protected void AddWindow<T>() where T : EditorWindow {
            GetWindow<T>(typeof(T).Name).Show();
        }
    
        // === Start ===
        protected virtual void OnEnable() {
            // ラベルスタイルの設定
            LabelStyle = new List<GUIStyle>();
            // ラベル生成
            LabelStyle.Add(CreateStyle(null, true, true, Color.red));
            LabelStyle.Add(CreateStyle(null, true,false));
            // タブサイズ固定
            minSize = Vec2(300f);
            maxSize = Vec2(400f);
        }
        // === Update === 描画
        protected virtual void OnGUI() {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            using (new EditorGUI.IndentLevelScope()) { /* ここだけ自動改行 */ }
            Field();
            Field("作業効率化", LabelStyle[0]);
            if (Button("コンポネ全て折りたたむ")) ComponentsClose();
            Field();
            Field("Objの子削除", LabelStyle[0]);
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Obj削除(子は残す)")) {
                    if (IsHierarchySelection) HierarchySelect.ChildDestroy(true);
                    else HierarchySelects.For(i => { HierarchySelects[i].ChildDestroy(true); });
                }
                if (Button("Obj削除(子孫全て)"　)) {
                    if (IsHierarchySelection) HierarchySelect.ChildDestroy();
                    else HierarchySelects.For(i => { HierarchySelects[i].ChildDestroy(); });
                }
            }
            Field();
            Field("生成関連", LabelStyle[0]);
            if (Button("Canvas生成"            )) {
                if (IsHierarchySelection) GenerateCanvas("Canvas", 0, HierarchySelect);
                else HierarchySelects.For(i => { GenerateCanvas("Canvas", 0, HierarchySelects[i]); });
            }
            if (Button("Object(0, 0, 0)生成"   )) {
                if (IsHierarchySelection) GenerateObject("Object", HierarchySelect);
                else HierarchySelects.For(i => { GenerateObject("Object", HierarchySelects[i]); });
            }
            if (Button("Image(1920 * 1080)生成")) {
                if (IsHierarchySelection) GenerateImage( "Image" , HierarchySelect, Vec2(1920f, 1080f));
                else HierarchySelects.For(i => { GenerateImage( "Image" , HierarchySelects[i], Vec2(1920f, 1080f)); });
            }
            if (Button("Button(250 * 250)生成" )) {
                if (IsHierarchySelection) GenerateButton("Button", HierarchySelect, Vec2( 250f,  250f));
                else HierarchySelects.For(i => { GenerateButton("Button", HierarchySelects[i], Vec2( 250f,  250f)); });
            }
            Field();
            Field("Pos移動(120)", LabelStyle[0]);
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("X-")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosX(HierarchySelect.GetLocalPos().x - 120f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosX(HierarchySelects[i].GetLocalPos().x - 120f); });
                }
                if (Button("X0")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosX(0f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosX(0f); });
                }
                if (Button("X+")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosX(HierarchySelect.GetLocalPos().x + 120f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosX(HierarchySelects[i].GetLocalPos().x + 120f); });
                }
            }
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Y-")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosY(HierarchySelect.GetLocalPos().y - 120f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosY(HierarchySelects[i].GetLocalPos().y - 120f); });
                }
                if (Button("Y0")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosY(0f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosY(0f); });
                }
                if (Button("Y+")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosY(HierarchySelect.GetLocalPos().y + 120f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosY(HierarchySelects[i].GetLocalPos().y + 120f); });
                }
            }
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Z-")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosZ(HierarchySelect.GetLocalPos().z - 120f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosZ(HierarchySelects[i].GetLocalPos().z - 120f); });
                }
                if (Button("Z0")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosZ(0f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosZ(0f); });
                }
                if (Button("Z+")) {
                    if (IsHierarchySelection) HierarchySelect.LocalPosZ(HierarchySelect.GetLocalPos().z + 120f);
                    else HierarchySelects.For(i => { HierarchySelects[i].LocalPosZ(HierarchySelects[i].GetLocalPos().z + 120f); });
                }
            }
            Field();
            Field("コライダー付与", LabelStyle[0]);
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Mesh")) {
                    if (IsHierarchySelection) {
                        MeshCollider mesh = HierarchySelect.ReferenceComponent<MeshCollider>();
                        mesh.convex = true;
                    }
                    else HierarchySelects.For(i => {
                        MeshCollider mesh = HierarchySelects[i].ReferenceComponent<MeshCollider>();
                        mesh.convex = true;
                    });
                }
                if (Button("Box")) {
                    if (IsHierarchySelection) HierarchySelect.ReferenceComponent<BoxCollider>();
                    else HierarchySelects.For(i => { HierarchySelects[i].ReferenceComponent<BoxCollider>(); });
                }
                if (Button("Box2D")) {
                    if (IsHierarchySelection) HierarchySelect.ReferenceComponent<BoxCollider2D>();
                    else HierarchySelects.For(i => { HierarchySelects[i].ReferenceComponent<BoxCollider2D>(); });
                }
            }
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Circle2D")) {
                    if (IsHierarchySelection) HierarchySelect.ReferenceComponent<CircleCollider2D>();
                    else HierarchySelects.For(i => { HierarchySelects[i].ReferenceComponent<CircleCollider2D>(); });
                }
                if (Button("Capsule")) {
                    if (IsHierarchySelection) HierarchySelect.ReferenceComponent<CapsuleCollider>();
                    else HierarchySelects.For(i => { HierarchySelects[i].ReferenceComponent<CapsuleCollider>(); });
                }
                if (Button("Capsule2D")) {
                    if (IsHierarchySelection) HierarchySelect.ReferenceComponent<CapsuleCollider2D>();
                    else HierarchySelects.For(i => { HierarchySelects[i].ReferenceComponent<CapsuleCollider2D>(); });
                }
                
            }
            Field();
            Field("コライダー削除", LabelStyle[0]);
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Mesh")) {
                    if (IsHierarchySelection) HierarchySelect.RemoveComponent<MeshCollider>();
                    else HierarchySelects.For(i => { HierarchySelects[i].RemoveComponent<MeshCollider>(); });
                }
                if (Button("Box")) {
                    if (IsHierarchySelection) HierarchySelect.RemoveComponent<BoxCollider>();
                    else HierarchySelects.For(i => { HierarchySelects[i].RemoveComponent<BoxCollider>(); });
                }
                if (Button("Box2D")) {
                    if (IsHierarchySelection) HierarchySelect.RemoveComponent<BoxCollider2D>();
                    else HierarchySelects.For(i => { HierarchySelects[i].RemoveComponent<BoxCollider2D>(); });
                }
            }
            using (new EditorGUILayout.HorizontalScope()) { /* ここだけ横並び */
                if (Button("Circle2D")) {
                    if (IsHierarchySelection) HierarchySelect.RemoveComponent<CircleCollider2D>();
                    else HierarchySelects.For(i => { HierarchySelects[i].RemoveComponent<CircleCollider2D>(); });
                }
                if (Button("Capsule")) {
                    if (IsHierarchySelection) HierarchySelect.RemoveComponent<CapsuleCollider>();
                    else HierarchySelects.For(i => { HierarchySelects[i].RemoveComponent<CapsuleCollider>(); });
                }
                if (Button("Capsule2D")) {
                    if (IsHierarchySelection) HierarchySelect.RemoveComponent<CapsuleCollider2D>();
                    else HierarchySelects.For(i => { HierarchySelects[i].RemoveComponent<CapsuleCollider2D>(); });
                }
                
            }
            EditorGUILayout.EndScrollView();
        }
        // === End ===
        protected virtual void OnDisable() {
            
        }

        // === 生成 ===
        protected void ScriptGenerator(string scriptName, string contents = "") {
            // パスに指定フォルダが無いなら作成
            if (!Directory.Exists(Application.dataPath + "/Generate")) AssetDatabase.CreateFolder("Assets", "Generate");
            // 作成先パスを設定 (ダブりは名前変更)
            string scriptPath = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/Generate/" + scriptName + ".cs");
            // 生成
            File.WriteAllText(scriptPath, contents);
            // 変更のインポート
            AssetDatabase.Refresh();
        }

        void ComponentsClose() {
            // 選択中のすべてのゲームオブジェクトに対して操作
            foreach (GameObject obj in Selection.gameObjects) {
                // ゲームオブジェクトの全コンポーネントを取得
                Component[] components = obj.GetComponents<Component>();
                foreach (Component component in components) {
                    // コンポーネントを折りたたむ
                    // 注：'InternalEditorUtility'は公式のドキュメントには記載されていない内部APIです
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(component, false);
                }
            }
        }

    }

    // ===== Editor =====         // 既存Windowカスタム
    public class BaseE<T> : Editor where T : MonoBehaviour {
    
        /* 使い方 (編集クラスと, いじるクラス, で作成)
#if UNITY_EDITOR
        using UnityEditor;
#endif
        [CustomEditor(typeof(CustomClass))]
        public class EditorClass : BaseE<CustomClass> {
            protected override void Inspector() { }
        }
        */
    
        // 何のスクリプト表示をいじるか？
        protected T script;

        // 元々のインスペクターを重ねて表示するか？
        public bool baseInspectorGUI = false;

        // === Start ===
        protected virtual void OnEnable() {
            script = (T)target;
        }
        // === Refresh === 描画(書換時に実行)
        public override void OnInspectorGUI() {
            // 描画切替ボタン
            if (Button($"インスペクターの描画切替  現在：{(baseInspectorGUI ? "オリジナル (Original)" : "カスタム (Custom)")}")) { baseInspectorGUI = !baseInspectorGUI; }
            // 描画
            if (baseInspectorGUI) {
                // 初期のInspector
                base.OnInspectorGUI();
            }else {
                // Inspectorの情報を取得
                serializedObject.Update();
                // 追加処理
                Inspector();
                // Inspectorの情報を上記で書換
                serializedObject.ApplyModifiedProperties();
            }
        }
        // === override用 ===
        protected virtual void Inspector() {
            
        }
        // === End ===
        protected virtual void OnDisable() {
            
        }
    
    }

    /// <summary> カスタム属性 (エディタ上の名付け) </summary>
    [Serializable]
    public class Name : PropertyAttribute {
        public string[] names;
        public Name(params string[] names) { this.names = names; }
    }

    /// <summary> カスタムドロワー (エディタ上の名付け) </summary>
    [CustomPropertyDrawer(typeof(Name))]
    public class Drawer : PropertyDrawer {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
            Name name = (Name)attribute;
            int pos = -1;
            string[] pathParts = property.propertyPath.Split('[', ']');

            if (1 < pathParts.Length) {
                if (int.TryParse(pathParts[1], out pos) && -1 < pos && pos < name.names.Length) {
                    Field(rect, property, name.names[pos]);
                }else{
                    Field(rect, "使用しない枠です。  ( don't use )");
                }
            }else{
                Field(rect, property, name.names[0]);
            }

        }
    }
    
#endif
    
    // ===== MonoBehaviour =====  // Mono:Base (Start、Update、Fixed、を通常で使う用)
    public class Base : MoBe {

        // 関数セット
        protected Admin admin;
    
        // ゲームデータ
        protected ScriptableObject scob;
    
        // サウンド
        protected SoundSystem sound;

        // === 起動時の最初に1回実行 ===
        [Obsolete]
        protected virtual void Awake() {
            Application.targetFrameRate = 60;
            admin = FindObjectOfType<Admin>();
            if (admin != null) scob  = admin.data;
            if (admin != null) sound = admin.sound;
        }
    
        /// <summary> 存在ログ </summary>
        protected virtual bool ScriptLog() {
            if (admin == null) { Debug.LogWarning("Admin？");            return false; }
            if (scob  == null) { Debug.LogWarning("ScriptableObject？"); return false; }
            if (sound == null) { Debug.LogWarning("SoundSystem？");      return false; }
            return true;
        }

        /// <summary> 複数のコルーチン同時実行 </summary>
        public Coroutine[] StartCoroutines(IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            Coroutine[]   re  = new Coroutine[cor.Length];
            for (int i = 0; i < re.Length; i++) {
                re[i] = StartCoroutine(cor[i]);
            }return re;
        }
        /// <summary> 複数のコルーチン同時実行 </summary>
        public Coroutine[] StartCoroutines(IEnumerator[] coroutines) {
            Coroutine[]   cor = new Coroutine[coroutines.Length];
            for (int i = 0; i < cor.Length; i++) {
                cor[i] = StartCoroutine(coroutines[i]);
            }return cor;
        }
    
        /// <summary> 秒待機後、実行 </summary>
        public Coroutine Wait(float seconds, Action action) {
            return StartCoroutine(WaitCor(seconds, action));
        }
    
        /// <summary> 条件待機後、実行 </summary>
        public Coroutine Wait(Func<bool> condition, Action action) {
            return StartCoroutine(WaitCor(condition, action));
        }

        /// <summary> Coroutineを同時に実行し、どれか終了するまで待機 </summary>
        public IEnumerator AnyWait(IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            bool[]      isEnd = new bool[cor.Length];
            cor.For(i => {
                StartCoroutine(StuckCoroutine(cor[i], () => isEnd[i] = true));
            });
            yield return new WaitUntil(() => isEnd.Contains(true));
        }
        /// <summary> Coroutineを同時に実行し、どれか終了するまで待機 </summary>
        public IEnumerator AnyWait(IEnumerator[] coroutines) {
            bool[] isEnd = new bool[coroutines.Length];
            coroutines.For(i => { StartCoroutine(StuckCoroutine(coroutines[i], () => isEnd[i] = true)); });
            yield return new WaitUntil(() => isEnd.Contains(true));
        }

        /// <summary> Coroutineを同時に実行し、全て終了するまで待機 </summary>
        public IEnumerator AllWait(IEnumerator coroutine, params IEnumerator[] coroutines) {
            IEnumerator[] cor = Params(coroutine, coroutines);
            bool[]      isEnd = new bool[cor.Length];
            cor.For(i => {
                StartCoroutine(StuckCoroutine(cor[i], () => isEnd[i] = true));
            });
            yield return new WaitUntil(() => !isEnd.Contains(false));
        }
        /// <summary> Coroutineを同時に実行し、全て終了するまで待機 </summary>
        public IEnumerator AllWait(IEnumerator[] coroutines) {
            bool[] isEnd = new bool[coroutines.Length];
            coroutines.For(i => { StartCoroutine(StuckCoroutine(coroutines[i], () => isEnd[i] = true)); });
            yield return new WaitUntil(() => !isEnd.Contains(false));
        }
    
    }
    
    // ===== MonoBehaviour =====  // Base:Super
    public class Super : Base {

        // === 起動時に始めの1回実行 ===
        protected virtual void Start() {
            
        }
    
        // === 繰り返し実行(fps依存) ===
        protected virtual void Update() {
            
        }
    
        // === 繰り返し実行(1/50fps) ===
        protected virtual void FixedUpdate() {
            
        }
    
    }
    
    // ===== Bace =====
    public class DataBase : Base {

        protected GameData data;

        // === 起動時の最初に1回実行 ===
        [Obsolete]
        protected override void Awake() {
            base.Awake();
            data = admin.data as GameData;
        }

    }
    
    // ===== DataBase =====       // DataBase:キャラベース
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Animator))]
    public class CharacterBase : DataBase {

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public Animator  ani;

        // キャラクター情報
        public BattleChara chara;

        [Obsolete]
        protected override void Awake() {
            base.Awake();
            if (this.IsTag("Untagged")) Log("キャラタグあってる？");
            rb  = this.ReferenceComponent<Rigidbody>();
            ani = this.ReferenceComponent<Animator>();
        }

    }
    
    // ===== DataBase =====       // DataBase:カメラ
    public class BaseCamera : DataBase {
    
        // 追跡するか？
        [SerializeField] bool isTracking = false;
        protected bool IsTracking { get { return isTracking; } set { isTracking = value; } }
    
        // 追跡するターゲット
        [SerializeField] GameObject target;
        protected GameObject Target { get { return target; } }
    
        // 追跡する軸どれ？
        [SerializeField] Flag3 axis = new Flag3();
        protected Flag3 Axis { get { return axis; } }
    
        // 追跡ターゲットを中心として、カメラ中心をずらす幅
        [SerializeField] Vector3 offset = new Vector3();
        protected Vector3 Offset { get { return offset; } set { offset = value; } }
    
        // offsetの四方制限
        [SerializeField] Limit4 limit = new Limit4();
        protected Limit4 Limit { get { return limit; } set { limit = value; } }
    
        // === 追跡に使うUpdate ===
        protected virtual void LateUpdate() {
            if (isTracking) {
                Vector3 trackingPos = new Vector3();
                trackingPos.x = axis.X ? target.transform.position.x + offset.x : gameObject.transform.position.x;
                trackingPos.y = axis.Y ? target.transform.position.y + offset.y : gameObject.transform.position.y;
                trackingPos.z = axis.Z ? target.transform.position.z + offset.z : gameObject.transform.position.z;
                gameObject.transform.position = trackingPos;
            }
        }
    
        // === 追跡を開始する時に外部から呼び出し (追跡物, 追跡軸, ずらし幅, 初期位置) ===
        public virtual void Tracking(GameObject target, Flag3 axis, Vector3? offset = null, Limit4 newLimit = null, Vector3? initial = null) {
            IsTracking = true;
            Offset = offset ?? new Vector3(0f, 0f, -10f);
            Limit = newLimit ?? new Limit4(Offset, -Offset);
            gameObject.transform.position = initial ?? target.transform.position;
            this.target = target;
            this.axis = axis;
        }
    
        // === 条件下でoffsetをずらす ===
        public virtual void OffsetShift(Vector3 shift, Limit4 newLimit = null) {
            limit = newLimit ?? limit;
            if (limit.LimitCheck(offset + shift)) {
                offset += shift;
            }
            else {
                offset = limit.LimitPoint(offset + shift);
            }
        }
    
        // === 追跡をしない時に呼び出し (定位置) ===
        public virtual void Tracking_null(GameObject target, Vector3 offset) {
            isTracking = false;
            gameObject.transform.position = target.transform.position + offset;
        }
        public virtual void Tracking_null(Vector3 initial) {
            isTracking = false;
            gameObject.transform.position = initial;
        }
    
    }
    
    // ===== DataBase =====       // DataBase:マネージャー
    public class Manager : DataBase {
    
        // ﾃﾃﾃﾃ会話速度
        [SerializeField, Min(0.1f)] float textSpeed = 0.3f;
        protected float TextSpeed { get { return textSpeed; } set { textSpeed = ClampMin(value, 0.1f); } }

        // === 起動時に始めの1回実行 ===
        [Obsolete]
        protected override void Awake() {
            base.Awake();
            ScriptLog();
        }
    
        // === 物語風にテキスト表示 ===
        protected Coroutine textWriter;
        protected bool DrawText(TextMeshProUGUI talkText, string text) {
            if (textWriter == null) return false;
            textWriter = StartCoroutine(TextWriter(talkText, text));
            return true;
        }
        protected bool DrawText(TextMeshProUGUI nameText, TextMeshProUGUI talkText, string name, string text) {
            if (textWriter == null) return false;
            nameText.text = name + "\n「";
            textWriter = StartCoroutine(TextWriter(talkText, "\n  " + text + "」"));
            return true;
        }
        // === Textをぬるぬる表示する関数 ===
        protected IEnumerator TextWriter(TextMeshProUGUI textBox, string text) {
            float time = 0;  //経過時間を表す
            while (true) {
                yield return null;
                time += Time.deltaTime;
                //経過時間で、今表示されるべきの文字の長さを表す
                int len = Mathf.FloorToInt(time / TextSpeed);
                //クリックさるか、全文表示されたら、1文字ずつ表示を終わる
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)) { break; }
                if (len > text.Length) { break; }
                //経過時間で、今表示されるべきの文字の長さを表示する
                textBox.text = text.Substring(0, len);
            }
            textBox.text = text;  //ループを終わると文章全文表示する
            yield return null;  //コルーチンが適切に終了するまで待機
            textWriter = null;
        }
    
        // === TextWriterのクリックを待つ関数 ===
        protected virtual IEnumerator IsTextWriter() {
            yield return new WaitUntil(() => (textWriter == null));
        }
        protected virtual IEnumerator Skip() {
            yield return new WaitUntil(() => (textWriter == null));
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0));
        }
    
    }
    
    // ===== Chara (Base) =====   // Chara
    [Serializable]
    public class Chara {
    
        // 名前
        [SerializeField] string name;
        public string Name       { get { return name;        } set { name        = value;               } }

        // 動く速さ、回転速度、ダッシュ倍率
        [SerializeField, Min(0)] float moveSpeed = 5f, rotateSpeed = 500f, dash = 1.5f;
        public float MoveSpeed   { get { return moveSpeed;   } set { moveSpeed   = ClampMin(value, 0f); } }
        public float RotateSpeed { get { return rotateSpeed; } set { rotateSpeed = ClampMin(value, 0f); } }
        public float Dash        { get { return dash;        } set { dash        = ClampMin(value, 0f); } }
    
        [SerializeField, Min(0f)] float jumpPower = 2f;
        public float JumpPower    { get { return jumpPower;   } set { jumpPower   = ClampMin(value, 0f); } }
    
        // === コンストラクタ ===
        public Chara() {
            Name  = "";
            MoveSpeed   =    3f;
            RotateSpeed = 1000f;
        }
    
    }
    
    // ===== Chara (1) =====      // Chara:BattleCharacter
    [Serializable]
    public class BattleChara : Chara {
    
        // HP, SP, 割合, 攻撃力, 防御力
        [SerializeField, Min(0)] int hp_max = 0, hp_now = 0, sp_max = 0, sp_now = 0, atk = 0, def = 0;
        public int    HP_MAX { get => hp_max; set => hp_max = ClampMin(value, 0);      }
        public int    HP_NOW { get => hp_now; set => hp_now = Clamp(value, 0, HP_MAX); }
        public int    SP_MAX { get => sp_max; set => sp_max = ClampMin(value, 0);      }
        public int    SP_NOW { get => sp_now; set => sp_now = Clamp(value, 0, SP_MAX); }
        public float  HP_PER { get => (float)HP_NOW / HP_MAX; }
        public float  SP_PER { get => (float)SP_NOW / SP_MAX; }
        public int    ATK    { get => atk;    set => atk    = ClampMin(value, 0);      }
        public int    DEF    { get => def;    set => def    = ClampMin(value, 0);      }
    
        // === コンストラクタ ===
        public BattleChara() {
            HP_MAX = 0;
            HP_NOW = 0;
            SP_MAX = 0;
            SP_NOW = 0;
        }
    
        /// <summary> 回復する </summary> <param name="heal"> 回復量 </param>
        public virtual void Heal(int heal) {
            HP_NOW = HP_MAX < HP_NOW + heal ? HP_MAX : HP_NOW + heal;
        }

        /// <summary> ダメージを受ける </summary> <param name="heal"> ダメージ量 </param>
        public virtual void Damage(int damage) {
            damage = ClampMin(damage - DEF, 0);
            HP_NOW = HP_NOW >= damage ? HP_NOW - damage : 0;
        }
    
    }
    
    // ===== Chara (2) =====      // Chara:BattleCharacter:LevelCharacter
    [Serializable]
    public class LevelChara : BattleChara {
    
        // 経験値
        [SerializeField, Min(0)] int exp = 0;
        public int    Exp    { get { return exp;    } set { exp    = ClampMin(value, 0); } }
    
        // Level
        [SerializeField, Range(0, 99)] int level = 0;
        public int    Level  { get { return level;  } set { level  = Clamp(value, 0, 99); } }
    
        // === コンストラクタ ===
        public LevelChara() {
            Exp   = 0;
            Level = 0;
        }
    
    }

    // ===== LoopPanel =====      // UI Imageをリールで表示し、拡大などもできる
    [Serializable]
    public class ViewLoopPanel {

               MonoBehaviour mono;

        public RectTransform expansion,  relay , front , back ;
               TransValue    expansionV, relayV, frontV, backV;
        public Transform     panelParent;
        [HideInInspector]
        public List<Image>   panel       = new List<Image>();
        public float         time        =  0.3f;
               float         space;
               bool          IsExpansion = false;
               Coroutine     task        =  null;

        /// <summary> 初期化 </summary>
        public void Initialize(MonoBehaviour mono, Sprite[] sprites = null) {
            this.mono  = mono;
            if (expansion != null) expansionV = expansion;
            relayV     = relay;
            frontV     = front;
            backV      = back;
            Reset(sprites);
        }

        /// <summary> 表示リセット </summary>
        public void Reset(Sprite[] sprites = null) {
            if (task != null) {
                mono.StopCoroutine(task);
                task = null;
            }
            if (sprites != null  && sprites.Length != 0 && panelParent.ChildCount() != sprites.Length) {
                panel.Clear();
                panelParent.ChildDestroy();
                for(int i = 0; i < sprites.Length; i++) {
                    Image im = GenerateImage("Panel", panelParent.gameObject);
                    panel.Add(im);
                }
            }
            panel = panelParent.GetChilds<Image>().ToList();
            panel.Reverse();
            space = panel.Count <= 1 ? 0f : (1f / (panel.Count - 1));
            panel.For(i => {
                if (sprites != null && sprites.Length != 0 && i < sprites.Length) panel[i].sprite = sprites[i];
                panel[i].rectTransform.ToLerp(frontV, backV, space * i);
            });
            IsExpansion = false;
        }

        /// <summary> 手前のパネルを拡大縮小する </summary>
        public void Scaling() {
            if (task != null || expansion == null) return;
            task = mono.StartCoroutine(ScalingC());
        }
        IEnumerator ScalingC() {
            yield return TimeLerp(
                range => { panel[0].rectTransform.ToLerp(frontV, expansionV, range); },
                time,
                IsExpansion ? 1f : 0f,
                IsExpansion ? 0f : 1f
            );
            IsExpansion = !IsExpansion;
            task = null;
        }
       
        /// <summary> パネルを中継位置経由で移動させる、ヒエラルキー順列も変更する </summary>
        public void Looping(bool dir = true) {
            if (task != null || panel.Count <= 1) return;
            if (IsExpansion) {
                task = mono.StartCoroutine(ScalingC());
            }else{
                task = mono.StartCoroutine(LoopingC(mono, dir));
            }
        }
        IEnumerator LoopingC(MonoBehaviour mono, bool dir) {
            float half = time / 2;
            IEnumerator[] cor = new IEnumerator[panel.Count];
            if (dir) {
                cor[0] = TimeLerp(range => { panel[0].rectTransform.ToLerp(frontV, relayV, range); }, half);
                panel.For(i => {
                    if (i != 0) {
                        cor[i] = TimeLerp(
                            range => { panel[i].rectTransform.ToLerp(frontV, backV, range); },
                            half,
                            space * i,
                            space * (i - 1)
                        );
                    }
                });
                yield return mono.AllWait(cor);
                panelParent.SetChildOrder(panelParent.ChildCount() - 1, 0);
                yield return TimeLerp(range => { panel[0].rectTransform.ToLerp(relayV, backV, range); }, half);
                panel.Order(0);
            }else{
                int end = panel.Count - 1;
                panel.For(i => {
                    if (i != end) {
                        cor[i] = TimeLerp(
                            range => { panel[i].rectTransform.ToLerp(frontV, backV, range); },
                            half,
                            space * i,
                            space * (i + 1)
                        );
                    }
                });
                cor[end] = TimeLerp(range => { panel[end].rectTransform.ToLerp(relayV, frontV, range); }, half);
                yield return TimeLerp(range => { panel[end].rectTransform.ToLerp(backV, relayV, range); }, half); 
                panelParent.SetChildOrder(0);
                yield return mono.AllWait(cor);
                panel.Order(end, 0);
            }
            task = null;
        }

    }

    // ===== ガチャ値 =====
    [Serializable]
    public struct GachaValue {
        // ID
        [SerializeField] int _id;
        public int   id { get => _id; set => _id = value; }
        // 確率
        [SerializeField, Min(0f)] float _probability;
        public float probability { get => _probability; set => _probability = value; }
        // === コンストラクタ ===
        public GachaValue(int _id, float _probability) {
            this._id          = _id;
            this._probability = _probability;
        }
    }

    // ===== int型を参照として持つ =====
    [Serializable]
    public class intR {
        [SerializeField, Min(0)] int _value = 0;
        public int value { get => _value; set => _value = value; }
    }

    // ===== ガチャ画面 =====
    [Serializable]
    public class GachaPanel {

        // panelの親 と 10枚のpanel と 見せ位置 と 開始位置 と めくり中継位置 と 終了位置
        public RectTransform panelParent;
        Image[] panel;
        TransValue[] panelV, panelS, panelR, panelE;

        // 時間 と 管理Coroutine
        public float time = 0.2f;
        MonoBehaviour mono;
        Coroutine task = null;
        public bool IsRun { get => task != null; }

        // プレイヤーアクションを待機
        Func<bool> _playerAction;
        public Func<bool> playerAction { set => _playerAction = value; }

        // 動くたびの実行、コストが足りない時の実行
        Action _each, _notCost;
        public Action each    { set => _each    = value; }
        public Action notCost { set => _notCost = value; }

        // 全アイテムデータ
        List<Item> items;

        // 手持ちのコスト
        intR       costValue;

        // がちゃリスト
        [Serializable]
        public class GachaList {
            // がちゃラインナップSprite
            public Sprite sprite;
            // コスト
            [Min(0)]
            public int    cost;
            // 排出率
            public List<GachaValue> gachaValue = new List<GachaValue>();
            // ボタン
            [Min(1), Tooltip("○連の数値を２つまで設定できます。")]
            public int[] buttonRunNum = { 1, 10 };
        }

        // 表示パネル と がちゃ種類 と 選択ボタン と 記録 と 記録テキスト
        public GameObject      selectPanel;
        public ViewLoopPanel   loopPanel;
        public List<GachaList> gachaList = new List<GachaList>();
        public Button          left, right;
               int             select    = 0;
        public int             Select { get => select; set => select = Normalized(value, 0, gachaList.Count); }
        public TextMeshProUGUI selectText;

        public Button          run1,  run2;
               RectTransform   run1R, run2R;
        　　　 TransValue      run1V, run2V;

        // レア表示、範囲設定
        [Serializable]
        public struct RareSprite {
            [Min(1)]
            public int    num;
            public Sprite sprire;
        }
        [Tooltip("numは範囲の長さ、配列最後尾は長さ関係ない")]
        public List<RareSprite> rareSprites;
        Sprite RareView(int rarity) {
            int count = 0;
            for (int i = 0; i < rareSprites.Count; i++) {
                count += rareSprites[i].num;
                if (rarity <= count) return rareSprites[i].sprire;
            }return rareSprites[rareSprites.Count - 1].sprire;
        }

        // 前回のガチャ結果
        int[] _log;
        public int[] log { get => _log; }

        /// <summary> 初期化 </summary>
        public void Initialize(MonoBehaviour mono, List<Item> items, intR CostValue, Action Each = null, Action NotCost = null, Func<bool> PlayerAction = null) {
            // 実行中なら無視
            if (task != null) return;
            // レア表示エラーチェック
            if (rareSprites.Count < 1) {
                Log("RareSprite は１つ以上");
                return;
            }
            // 実行MoBe と Itemデータ と プレイヤーの反応を待つ関数 を取得
            this.mono 　 = mono;
            this.items   = items;
            costValue    = CostValue;
            each         = Each;
            notCost      = NotCost;
            playerAction = PlayerAction;
            // 10枚のpanel参照取得
            panel = panelParent.GetChilds<Image>();
            if (panel.Length != 10) { Log("Parent内のPanelは10枚にして"); return; }
            // panelの座標記録(見せ位置 と 開始位置)、透明化、初期位置に移動
            panelV = new TransValue[panel.Length];
            panelS = new TransValue[panel.Length];
            panelR = new TransValue[panel.Length];
            panelE = new TransValue[panel.Length];
            panelV[0] = panel[0];
            panelV[panelV.Length - 1] = panel[panel.Length - 1];
            panel.For(i => {
                panel[i].Alpha(0f);
                panelV[i] = Lerp(panelV[0], panelV[panelV.Length - 1], i % 5 * 0.25f);
                panelV[i].pos_y = i < 5 ? panelV[0].pos_y : panelV[panelV.Length - 1].pos_y;
                panelS[i] = panelV[i];
                panelS[i].scale = panelS[i].scale * 3;
                panelR[i] = panelV[i];
                panelR[i].angle_z = panelR[i].angle_z / 2;
                panelE[i] = panelV[i];
                panelE[i].pos_y = panelE[i].pos_y + panelParent.GetSize().y;
                panel[i].rectTransform.ToTransform(panelS[i]);
            });
            // 表示パネルの初期化
            Sprite[] lineup = new Sprite[gachaList.Count];
            gachaList.For(i => { lineup[i] = gachaList[i].sprite; });
            loopPanel.Initialize(mono, lineup);
            // RunボタンのTransform取得 と 座標記録
            run1R = run1.GetComponent<RectTransform>();
            run1V = run1R;
            run2R = run2.GetComponent<RectTransform>();
            run2V = run2R;
            Selection();
            // ボタンに役割付与
            left. ClickAction(() => { loopPanel.Looping(false); Select--; Selection(); _each?.Invoke(); });
            right.ClickAction(() => { loopPanel.Looping( true); Select++; Selection(); _each?.Invoke(); });
            run1. ClickAction(() => { Run(gachaList[Select].buttonRunNum[0]); });
            run2. ClickAction(() => { Run(gachaList[Select].buttonRunNum[1]); });
        }

        /// <summary> がちゃラインナップ選択ボタンの実行処理 </summary>
        public void Selection() {
            selectText.text = $"{Select + 1} / {gachaList.Count}";
            int buttonCount = gachaList[Select].buttonRunNum.Length;
            if (1 <= buttonCount) run1.Text($"{gachaList[Select].buttonRunNum[0]}連");
            if (2 <= buttonCount) run2.Text($"{gachaList[Select].buttonRunNum[1]}連");
            run1R.ToTransform(2 <= buttonCount ? run1V : (Lerp(run1V, run2V, 0.5f)));
            run1.Active(1 <= buttonCount);
            run2.Active(2 <= buttonCount);
        }

        /// <summary> 与えられたIDと確率でがちゃし 結果を表示 </summary>
        public void Run(int num = 1) {
            // 実行中、排出無、回数無効、なら無視
            if (task != null || gachaList[Select].gachaValue.Count == 0 || num < 1) {
                Log("がちゃの排出設定がされていない可能性があります");
                return;
            }
            if (costValue.value < (gachaList[Select].cost * num)) {
                Log("コストが足りていません");
                _notCost?.Invoke();
                return;
            }
            // 選択パネルを非表示
            selectPanel.Active(false);
            // ガチャを回し結果idを記録し、バッグにいれる
            _log = new int[num];
            _log.For(i => {
                _log[i] = Gacha(gachaList[Select].gachaValue);
                items[_log[i]].quantity++;
            });
            costValue.value -= (gachaList[Select].cost * num);
            _log.Log(null, "結果");
            // 結果表示の演出実行
            task = mono.StartCoroutine(RunC());
        }
        IEnumerator RunC() {
            // 一度に最大10枚ずつ表示するループ
            for (int p = 0; p < ((log.Length / 10) + 1); p++) {
                // 今回ループで表示する枚数計算
                int display = log.Length < 10 * (p + 1) ? log.Length % 10 : 10;
                // 画面にパネルを順に表示
                for (int i = 0; i < display; i++) {
                    panel[i].sprite = RareView(items[_log[p * 10 + i]].rarity);
                    panel[i].Alpha(1f);
                    _each?.Invoke();
                    yield return TimeLerp(value => { panel[i].rectTransform.ToLerp(panelS[i], panelV[i], value); }, M0s ? time / 3f : time);
                }
                // Playerのアクションを待機
                if (_playerAction != null) yield return new WaitUntil(() => _playerAction());
                // パネルを順にめくる
                for (int i = 0; i < display; i++) {
                    // 前半めくり
                    _each?.Invoke();
                    panelR[i].angle_x =  12.5f;
                    panelR[i].angle_y = -90f;
                    yield return TimeLerp(value => { panel[i].rectTransform.ToLerp(panelV[i], panelR[i], value); }, M0s ? time / 3f : time);
                    // 中間作業
                    panel[i].sprite = items[_log[p * 10 + i]].sprite;
                    // 後半めくり
                    panelR[i].angle_x = -12.5f;
                    panelR[i].angle_y =  90f;
                    yield return TimeLerp(value => { panel[i].rectTransform.ToLerp(panelV[i], panelR[i], value); }, M0s ? time / 3f : time, 1f, 0f);
                }
                // Playerのアクションを待機
                if (_playerAction != null) yield return new WaitUntil(() => _playerAction());
                // パネルを順に上に逃がして、初期位置へ
                for (int i = 0; i < display; i++) {
                    yield return TimeLerp(value => { panel[i].rectTransform.ToLerp(panelV[i], panelE[i], value); }, 0.1f);
                    panel[i].Alpha(0f);
                    panel[i].rectTransform.ToTransform(panelS[i]);
                }
            }
            yield return new WaitForSeconds(0.5f);
            // 選択パネルを表示
            selectPanel.Active(true);
            task = null;
        }
    }

    // ===== アイテム =====
    [Serializable]
    public class Item {

        // レアリティ
        [SerializeField, Min(1)]  int         _rarity;
        // 名前
        [SerializeField]          string      _name;
        // 2D画像
        [SerializeField]          Sprite      _sprite;
        // 所持数
        [SerializeField, Min(0)]  int         _quantity;
        // 効力
        public                    UnityAction action;
        // 使用間隔、使用制限管理Coroutine
        [SerializeField, Min(0f)] float       _interval, timer;
                                  Coroutine   useWait;

        // プロパティ
        public int    rarity     { get => _rarity;   set => _rarity   = ClampMin(value, 1); }
        public string name       { get => _name;                                            }
        public Sprite sprite     { get => _sprite;                                          }
        public int    quantity   { get => _quantity; set => _quantity = ClampMin(value, 0); }
        public float  interval   { get => _interval;                                        }
        public float  per        { get => timer / _interval; }
        public bool   UseWait    { get => useWait != null; }

        // 入手した事があるか？
        public bool   IsObtained { get; set; } = false;

        /// <summary> コンストラクタ </summary>
        public Item(int _rarity, string _name, Sprite _sprite, int _quantity, float _interval) {
            this._rarity   = ClampMin(_rarity, 1);
            this._name     = _name;
            this._sprite   = _sprite;
            this._quantity = ClampMin(_quantity, 0); ;
            this._interval = _interval;
            useWait  = null;
        }

        /// <summary> アイテム使用 </summary>
        public bool Use(MonoBehaviour mono) {
            if (useWait != null || quantity <= 0) return false;
            Log($"{_name}を使った");
            useWait = mono.StartCoroutine(UseC());
            return true;
        }
        IEnumerator UseC() {
            quantity--;
            action?.Invoke();
            if (0 < _quantity) {
                timer = _interval;
                while (0f < timer) {
                    timer -= Time.deltaTime;
                    yield return null;
                }
                timer = 0f;
            }
            useWait   = null;
        }

        /// <summary> 間隔再開 </summary>
        public void UseSync(MonoBehaviour mono) {
            useWait = mono.StartCoroutine(UseSyncC(mono));
        }
        IEnumerator UseSyncC(MonoBehaviour mono) {
            while (0f < timer) {
                timer -= Time.deltaTime;
                yield return null;
            }
            timer = 0f;
            useWait = null;
        }

    }

    // ===== アイテムリスト =====
    [Serializable]
    public class ItemList {

        MonoBehaviour     mono;

        List<Item>        items;
        List<Item>        have;

        [SerializeField]
        Transform         displayParent;
        [SerializeField]
        TextMeshProUGUI   pageText;
        int               page = 0;
        public int        Page { get => page; set => page = Normalized(value, 0, ((have.Count - 1) / 10) + 1); }

        Image[]           display, fill;
        TextMeshProUGUI[] quantity;
        Button[]          useButton;

        [SerializeField]
        Button            backButton, nextButton;

        // 動くたびの実行
        Action _each;
        public Action each { set => _each = value; }

        // Fill管理
        Coroutine fillTask = null;

        /// <summary> 初期化 </summary>
        public void Initialize(MonoBehaviour mono, List<Item> items, Action Each = null) {
            this.mono  = mono;
            this.items = items;
            _each      = Each;
            display    = displayParent.GetChilds<Image>();
            if (display.Length != 10) Log("Imageは１ページ分の10枚にしてね");
            fill       = new Image [display.Length];
            quantity   = new TextMeshProUGUI[display.Length];
            useButton  = new Button[display.Length];
            display.For(i => {
                fill[i]      = display[i].GetChild<Image>();
                fill[i].Fill(0f);
                if (fill[i] == null) Log("10枚のImageの子にそれぞれFill用Imageをつけてね");
                quantity[i]  = display[i].GetChild<TextMeshProUGUI>();
                if (quantity[i] == null) Log("10枚のImageの子にそれぞれTMPをつけてね");
                useButton[i] = display[i].GetChild<Button>(true);
                if (useButton[i] == null) Log("10枚のImageの子にそれぞれButtonをつけてね");
                useButton[i].ClickAction(() => { Use(i); });
            });
            DisplayUpdate();
            backButton.ClickAction(() => { Page--; DisplayUpdate(); _each?.Invoke(); });
            nextButton.ClickAction(() => { Page++; DisplayUpdate(); _each?.Invoke(); });
        }

        /// <summary> アイテム使用 </summary>
        public void Use(int useID) {
            int re = (Page * 10) + useID;
            if (have.Count <= re) {
                Log("ボタンに対応するアイテムが見つからない");
                return;
            }
            _each?.Invoke();
            have[re].Use(mono);
            DisplayUpdate();
        }

        /// <summary> 表示アイテムの更新 </summary>
        public void DisplayUpdate() {

            have = items != null ? items.Where(item => 0 < item.quantity).ToList() : new List<Item>();
            if ((((have.Count - 1) / 10) + 1) < (Page + 1)) Page--;
            pageText.text = $"{Page + 1} / {((have.Count - 1) / 10) + 1}";

            display.For(i => {
                int re = (Page * 10) + i;
                if (re < have.Count) {
                    display[i].sprite = have[re].sprite;
                    quantity[i].text  = $"×{have[re].quantity}";
                    display[i].Active(true);
                }else {
                    display[i].Active(false);
                }
            });
            
            if (fillTask == null && mono != null) mono.StartCoroutine(UseWait());

        }

        IEnumerator UseWait() {
            while(true) {
                fill.For(i => {
                    if (fill[i].IsActive()) fill[i].Fill(have[(Page * 10) + i].per);
                });
                yield return null;
            }
        }

    }

    // ===== EnumUtility =====    // Enum
    [Serializable]
    public class EnumUtility<T> where T : Enum {
    
        // 列挙値の配列
        T[] values;
    
        // === コンストラクタ ===
        public EnumUtility() {
            // 列挙型要素を列挙型配列で取得
            values = (T[])Enum.GetValues(typeof(T));
        }
    
        // === 列挙のすべての名前を取得 ===
        public string[] Names => values.Select(e => e.ToString()).ToArray();
    
        // === 列挙の要素数を取得 ===
        public int Length => values.Length;
    
        // === ランダムな列挙値を取得 ===
        public T Random => values[UnityEngine.Random.Range(0, Length)];
    
        // === Indexで列挙値を取得 ===
        public T SetIndex(int index) {
            if (index < 0 || index >= Length) {
                Debug.LogWarning("範囲外");
                return default;
            }
            return values[index];
        }
    
        // === 文字列で列挙値を取得 ===
        public T FromString(string name) {
            foreach (var value in values) {
                if (value.ToString() == name) {
                    return value;
                }
            }
            Debug.LogWarning($"{name}は含まれてない");
            return default;
        }
    
    }
    
    // ===== タイマー =====       // Timer
    [Serializable]
    public class Timer {
    
        // 開始時間と計測変数
        float start, timer;
        public float StartTime { get { return start; } set { start = ClampMin(value, 0f); } }
        // ポーズ機能 (true:実行中 , false:一時停止)
        bool  play;
    
        // === コンストラクタ ===
        public Timer() : this(10f) { }
        public Timer(float start) {
            this.start = start;
            this.timer = start;
            this.play  = false;
        }
    
        /// <summary> タイマーを開始する </summary> <remarks> 計測中でもStart値からやり直します </remarks> <param name="newStart"> Start値を変更 </param>
        public void Start(float? newStart = null) {
            timer = newStart ?? start;
            play  = true;
        }
    
        /// <summary> (u) 時間計測 </summary>
        public void Update() {
            if (play && 0f < timer) timer -= Time.deltaTime;
        }
    
        /// <summary> bool値でポーズする </summary> <param name="flag"> true:再生 , false:一時停止 </param> <returns> 今の残り時間 </returns>
        public float Pause(bool flag) {
            play = flag;
            return IsExit() ? 0f : timer;
        }
    
        /// <summary> タイマー終了したか？ </summary> <returns> true:した , false:してない </returns>
        public bool IsExit() {
            return timer <= 0f;
        }
    
    }
    
    // ===== タイマーリスナ ===== // List<Timer>
    [Serializable]
    public class Timerlistener {
    
        // 使用タイマー
        List<Timer> timerListener;
    
        // === コンストラクタ ===
        public Timerlistener() {
            timerListener = new List<Timer>();
        }
    
        // === インデクサー === タイマーリスト
        public Timer this[int index] {
            get { return 0 <= index && index < timerListener.Count ? timerListener[index] : null; }
            set { timerListener[index] = value; }
        }
    
        /// <summary> (u) 使用タイマーを動かす </summary>
        public void Update() {
            for(int i = 0; i < timerListener.Count; i++) timerListener[i].Update();
        }
    
        /// <summary> タイマースタート </summary> <param name="index"> 範囲 > 0:return 有:[有] 無:枠追加[indexは追加値に変更] </param> <param name="newStart"> 新規Start値 </param>
        public bool TimerStart(ref int index, float? newStart = null) {
            if (index < 0) {
                Debug.LogWarning("範囲外(下)");
                return false;
            }
            if (timerListener.Count <= index) {
                Debug.LogWarning("範囲外(上), AddTimer");
                index = timerListener.Count;
                Timer ad = new Timer();
                timerListener.Add(ad);
            }
            float start = newStart ?? timerListener[index].StartTime;
            timerListener[index].Start(start);
            return true;
        }
    
        /// <summary> タイマーの一時停止機能 </summary> <param name="index"> 範囲 </param> <param name="flag"> true:再生 , false:一時停止 </param> <returns> (true:範囲内 , false:範囲外), 範囲内なら残り時間 </returns>
        public (bool Out, float timer) TimerPause(int index, bool flag) {
            if (index < 0 || timerListener.Count <= index) {
                Debug.LogWarning("範囲外");
                return (false, 0f);
            }
            return (true, timerListener[index].Pause(flag));
        }
    
        /// <summary> タイマー終了したか？ </summary> <param name="index"> 範囲 </param> <returns> (true:範囲内 , false:範囲外), 範囲内なら(true:した , false:してない) </returns>
        public (bool Out, bool isExit) TimerIsExit(int index) {
            if (index < 0 || timerListener.Count <= index) {
                Debug.LogWarning("範囲外");
                return (false, false);
            }
            return (true, timerListener[index].IsExit());
        }
    
    }
    
    // === Sound Data ===         // SoundData(1)
    [Serializable]
    public class SoundData {
    
        // 音声命名
        [SerializeField] string soundName;
        public string SoundName { get { return soundName; } set { if (value != "") soundName = value; } }
    
        // Mp3アタッチ欄
        [SerializeField] AudioClip audioClip;
        public AudioClip AudioClip { get { return audioClip; } set { audioClip = value; } }
    
        // 各ボリューム
        [SerializeField, Range(0f, 1f)] float volume;
        public float Volume { get { return volume; } set { volume = Clamp(value ,0f ,1f); } }
    
        // === コンストラクタ ===
        public SoundData(float volume = 1) {
            Volume = volume;
        }
    
    }
    
    // === Sound System ===       // SoundSystem(list)
    [Serializable]
    public class SoundSystem {

        // Component
        [SerializeField]
        AudioSource audioSource;
        public void Set(AudioSource source) { audioSource = source; }
    
        // SoundData
        [SerializeField] List<SoundData> bgm, se;
        public List<SoundData> BGM { get { return bgm; } set { bgm = value; } }
        public List<SoundData> SE  { get { return se;  } set { se  = value; } }
    
        // 全てのボリューム
        [SerializeField, Range(0, 1)] float allVolume = 1;
        public float AllVolume { get { return allVolume; } set { allVolume = Clamp(allVolume, 0f, 1f); } }
    
        // === コンストラクタ ===
        public SoundSystem(float allVolume = 1, List<SoundData> bgm = null, List<SoundData> se = null) {
            BGM = bgm ?? new List<SoundData>();
            SE  = se  ?? new List<SoundData>();
            AllVolume = allVolume;
        }
    
       /// <summary> 再生速度を変更するプロパティ </summary>
       public float Pitch {
           get{ return audioSource.pitch; } 
           set{ audioSource.pitch = value; }
       }

        /// <summary> BGMを鳴らす </summary>
        public bool PlayBGM(string soundName) {
            SoundData data = bgm.Find(data => data.SoundName == soundName);
            if (audioSource == null) { Log("AudioSourceがnullだよ"); return false; }
            StopBGM();
            if (data == null) return false;
            audioSource.clip   = data.AudioClip;
            audioSource.volume = data.Volume * allVolume;
            audioSource.loop   = true;
            audioSource.Play();
            return true;
        }

        /// <summary> BGMを止める </summary>
        public void StopBGM() {
            if (audioSource == null) { Log("AudioSourceがnullだよ"); return; }
            audioSource.Stop();
        }

        /// <summary> SEを鳴らす </summary>
        public bool PlaySE(string soundName) {
            SoundData data = se.Find(data => data.SoundName == soundName);
            if (audioSource == null) { Log("AudioSourceがnullだよ"); return false; }
            if (data == null) return false;
            audioSource.volume = data.Volume * allVolume;
            audioSource.PlayOneShot(data.AudioClip);
            return true;
        }
    
    }
    
    // ===== EnemyGenerator ===== // EnemyGenerator
    [Serializable]
    public class EnemyGenerator {
    
        // エネミープレハブ種類
        [SerializeField] List<GameObject> enemyPrefab;
        public List<GameObject> EnemyPrehab { get { return enemyPrefab; } set { enemyPrefab = value; } }
    
        // 生成済Enemy
        List<GameObject> enemies = new List<GameObject>();
        public List<GameObject> Enemys { get { enemies.Remove(null); return enemies; } }
    
        // === 直接生成 === 引数にInstantiate
        public bool Generate(Func<GameObject, GameObject> action, int index = -1) {
            // indexがリスト範囲内なら指定生成
            if (0 <= index && index < enemyPrefab.Count) { enemies.Add(action(enemyPrefab[index])); return true; }
            // indexが0以下ならランダム生成
            if (0 > index && 0 < enemyPrefab.Count) { enemies.Add(action(enemyPrefab[Random.Range(0, enemyPrefab.Count)])); return true; }
            // 生成失敗
            return false;
        }
    
    }
    
    // ===== Flag3 =====          // Bool(3)
    [Serializable]
    public class Flag3 {
    
        // bool(3)
        [SerializeField] bool x, y, z;
        public bool X { get { return x; } set { x = value; } }
        public bool Y { get { return y; } set { y = value; } }
        public bool Z { get { return z; } set { z = value; } }
    
        // ＆とOR
        public bool And(bool flag) {
            return flag && X && Y && Z;
        }
        public bool Or(bool flag) {
            return X == flag || Y == flag || Z == flag;
        }
    
        // === コンストラクタ ===
        public Flag3() {
            X = false; Y = false; Z = false;
        }
        public Flag3(bool all) {
            X = all; Y = all; Z = all;
        }
        public Flag3(bool x, bool y, bool z) {
            X = x; Y = y; Z = z;
        }
    
        // === オペレーター ===
        public static implicit operator Flag3(bool allFlag) {
            return new Flag3(allFlag);
        }
        public static bool operator ==(Flag3 a, Flag3 b) {
            if (a == null || b == null) return false;
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(Flag3 a, Flag3 b) {
            return !(a == b);
        }
    
        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (obj is Flag3 other) {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
            return false;
        }
    
        public override int GetHashCode() {
            unchecked {  // オーバーフローを許可するために unchecked ブロックを使用
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }
    
    }
    
    // ===== Limit4 =====         // 四方Limit
    [Serializable]
    public class Limit4 {
    
        // 四方の制限
        [SerializeField] float top, right, bottom, left;
        public float Top    { get { return top;    } set { top    = value; } }
        public float Right  { get { return right;  } set { right  = value; } }
        public float Bottom { get { return bottom; } set { bottom = value; } }
        public float Left   { get { return left;   } set { left   = value; } }
    
        // 外側制限(true)、内側制限(false)
        [SerializeField] bool side = true;
        public bool  Side   { get { return side;   } set { side   = value; } }
    
        // === コンストラクタ ===
        public Limit4() {
            Top = 0f; Right = 0f; Bottom = 0f; Left = 0f; Side = true;
        }
        public Limit4(float top, float right, float bottom, float left, bool side = true) {
            Top = top; Right = right; Bottom = bottom; Left = left; Side = side;
        }
        public Limit4(Vector3 a, Vector3 b, bool side = true) {
            Top    = a.y > b.y ? a.y : b.y;
            Right  = a.x > b.x ? a.x : b.x;
            Bottom = a.y < b.y ? a.y : b.y;
            Left   = a.x < b.x ? a.x : b.x;
            Side   = side;
        }
    
        // === 制限に引っかかったらtrue ===
        public bool LimitCheck(Vector3 vector) {
            return Limit_top(vector) || Limit_right(vector) || Limit_bottom(vector) || Limit_left(vector);
        }
    
        // === 方向別、制限チェック ===
        public bool Limit_top(Vector3 vector)    { return side ? top    <= vector.y : top    >= vector.y; }
        public bool Limit_right(Vector3 vector)  { return side ? right  <= vector.x : right  >= vector.x; }
        public bool Limit_bottom(Vector3 vector) { return side ? bottom >= vector.y : bottom <= vector.y; }
        public bool Limit_left(Vector3 vector)   { return side ? left   >= vector.x : left   <= vector.x; }
    
        // === 制限で限界点に戻す ===
        public Vector3 LimitPoint(Vector3 vector) {
            vector.y = Limit_top(vector)    ? Top    : vector.y;
            vector.x = Limit_right(vector)  ? Right  : vector.x;
            vector.y = Limit_bottom(vector) ? Bottom : vector.y;
            vector.x = Limit_left(vector)   ? Left   : vector.x;
            return vector;
        }
    
    }

    // ===== Image(1), bool(1) =====
    [Serializable]
    public class ImageBool {
        // メンバ
        public Image panel;
        public bool  isFlag;
        // === コンストラクタ ===
        public ImageBool() {
            isFlag = false;
        }
    }

    /// <summary> Transformの値型 </summary>
    [Serializable]
    public struct TransValue {

        [SerializeField]
        Vector3 _pos, _angle, _scale;

        public Vector3 pos     { get => _pos    ; set => _pos     = value; }
        public float   pos_x   { get => _pos.x  ; set => _pos.x   = value; }
        public float   pos_y   { get => _pos.y  ; set => _pos.y   = value; }
        public float   pos_z   { get => _pos.z  ; set => _pos.z   = value; }

        public Vector3 angle   { get => _angle  ; set => _angle   = value.AngleNormalized(); }
        public float   angle_x { get => _angle.x; set => _angle.x = value.AngleNormalized(); }
        public float   angle_y { get => _angle.y; set => _angle.y = value.AngleNormalized(); }
        public float   angle_z { get => _angle.z; set => _angle.z = value.AngleNormalized(); }

        public Vector3 scale   { get => _scale  ; set => _scale   = value; }
        public float   scale_x { get => _scale.x; set => _scale.x = value; }
        public float   scale_y { get => _scale.y; set => _scale.y = value; }
        public float   scale_z { get => _scale.z; set => _scale.z = value; }

        Vector2 _size;

        public Vector2 size    { get => _size   ; set => _size    = value; }
        public float   width   { get => _size.x ; set => _size.x  = value; }
        public float   height  { get => _size.y ; set => _size.y  = value; }

        /// <summary> ローカル系で作成 </summary>
        public TransValue(RectTransform rect) {
            _pos   = rect.GetAnchorPos();
            _angle = rect.GetLocalAngle();
            _scale = rect.GetScale();
            _size  = rect.GetSize();
        }
        /// <summary> ローカル系で作成 </summary>
        public static implicit operator TransValue(RectTransform rect) { return new TransValue(rect); }

        /// <summary> ローカル系で作成 </summary>
        public TransValue(Transform tra) {
            _pos   = tra.GetLocalPos();
            _angle = tra.GetLocalAngle();
            _scale = tra.GetScale();
            _size  = new Vector2();
        }
        /// <summary> ローカル系で作成 </summary>
        public static implicit operator TransValue(Transform tra) { return new TransValue(tra); }

        /// <summary> ObjかUIか判断して作成 </summary>
        public TransValue(Component component) {
            TransValue value = new TransValue(component.gameObject);
            this._pos   = value.pos;
            this._angle = value.angle;
            this._scale = value.scale;
            this._size  = value.size;
        }
        /// <summary> ObjかUIか判断して作成 </summary>
        public static implicit operator TransValue(Component component) { return new TransValue(component); }

        /// <summary> ObjかUIか判断して作成 </summary>
        public TransValue(GameObject obj) {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null) {
                _pos   = rect.GetAnchorPos();
                _angle = rect.GetLocalAngle();
                _scale = rect.GetScale();
                _size  = rect.GetSize();
            }else {
                _pos   = obj.GetLocalPos();
                _angle = obj.GetLocalAngle();
                _scale = obj.GetScale();
                _size  = new Vector2();
            }
        }
        /// <summary> ObjかUIか判断して作成 </summary>
        public static implicit operator TransValue(GameObject obj) { return new TransValue(obj); }



    }

#if Pun2

    /// <summary> ネットワークデータ集合 </summary>
    [Serializable]
    public class Network {

        // 権限を持つ
        public enum AuthorityType { player, gameMaster }
        public AuthorityType authority;

        // サーバーに繋がっているか？
        public bool   inServer = false;

        // 参加中のルーム
        public string nowRoom = "";

        // ルームの情報
        public List<Room> room = new List<Room>();

        // 新規作成ルーム情報
        public Room create = new Room();
        public int  roomID = 0;

    }

    // ===== ルーム =====
    [Serializable]
    public class Room {

        // 内部情報
        public string name        = "";
        [Min(1)]
        public int    maxPlayer   =  1;
        public int    playerCount =  0;

        // コンストラクタ
        public Room(string name = "", int maxPlayer = 1) {
            this.name      = name;
            this.maxPlayer = maxPlayer;
        }

    }

#endif

}