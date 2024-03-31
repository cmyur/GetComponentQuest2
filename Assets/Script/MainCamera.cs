using UnityEngine;

using MyLibrary;
using static MyLibrary.Static;

public class MainCamera : Base {

    TransValue cameraInitialValue;

    public GameObject game;

    // カメラモード
    public enum CameraMode { Tracking, VR,  World, }
    public CameraMode cameraMode;

    // 追跡ターゲット
    [SerializeField]
    Transform trackingPosLookAt, vrPos;
    Transform vrLookAt;

    // カメラ設定
                public float height     = 0.5f;
    [Min(0.5f)] public float distance   = 2.0f;
    [Min(0.1f)] public float trackSpeed = 150f;
                public float lookDownLimit, lookUpLimit;
    [Min(0.0f)] public float mouseTrackSpeed;

    // === 起動時に始めの1回実行 ===
    void Start () {
        if (vrPos != null) vrLookAt = vrPos.GetChild<Transform>();
        CursorLock(false, false);
        cameraInitialValue = this;
    }

    // === 繰り返しの末尾で実行  ===
    void LateUpdate() {

        if (game.IsActive()) {

            if (cameraMode is CameraMode.Tracking) {
                // 自分の度からターゲット度にspeedで変更するための変更後の値を取得
                float angle = AngleTracking(gameObject.GetWorldAngle().y, trackingPosLookAt.GetWorldAngle().y, GetAxisPower() != 0f ? trackSpeed : 0f);
                // 高さ, 距離, 背後位置, を設定
                gameObject.SetWorldPos(trackingPosLookAt.GetWorldPos() + Vec3(distance * -Mathf.Sin(angle * Mathf.Deg2Rad), height, distance * -Mathf.Cos(angle * Mathf.Deg2Rad)));
                // Playerが動いてたら
                gameObject.LookAt(trackingPosLookAt);
            }

            if (cameraMode is CameraMode.VR) {
                // 高さ, 距離, 角度, を設定
                gameObject.SetWorldPos(vrPos.GetWorldPos());
                // ターゲットの方を向く
                gameObject.LookAt(vrLookAt);
            }

            if (cameraMode is CameraMode.World) {

            }

        }else {

            transform.ToTransform(cameraInitialValue);

        }

    }

}