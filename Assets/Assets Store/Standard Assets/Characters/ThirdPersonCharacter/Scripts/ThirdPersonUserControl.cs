using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        ThirdPersonCharacter m_Character; // オブジェクト上のThirdPersonCharacterへの参照
        Transform m_Cam;                  // シーン内のメインカメラのトランスフォームへの参照
        Vector3 m_CamForward;             // カメラの現在の前方方向
        Vector3 m_Move;
        bool m_Jump;                      // camForwardとユーザー入力から計算された、世界座標系における望ましい移動方向。

        void Start()　{
            // メインカメラのトランスフォームを取得する
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "警告：メインカメラが見つかりません。サードパーソンキャラクターには「Camera」とタグ付けされたカメラが必要です \"MainCamera\", カメラに関連したコントロールのために。", gameObject);

                // この場合、自身に関連したコントロールを使用しますが、これはおそらくユーザーが望むものではないでしょう。しかし、私たちは警告しました！
            }

            // サードパーソンキャラクターを取得する（これは必須コンポーネントのため、nullにはならないはずです）
            m_Character = GetComponent<ThirdPersonCharacter>();
        }


        void Update()
        {
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }


        // FixedUpdateは物理演算と同期して呼び出されます
        void FixedUpdate()
        {
            // 入力を読み取る
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);


            // キャラクターに渡すための移動方向を計算する
            if (m_Cam != null)
            {

                // 移動するためのカメラに関連した方向を計算する：
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v*m_CamForward + h*m_Cam.right;
            }
            else
            {

                // メインカメラがない場合、世界座標系に基づいた方向を使用します
                m_Move = v*Vector3.forward + h*Vector3.right;
            }
#if !MOBILE_INPUT

            // 歩行速度の倍率
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 3f;
#endif

            // すべてのパラメータをキャラクターコントロールスクリプトに渡す
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }
    }
}
