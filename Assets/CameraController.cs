using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;      // プレイヤーオブジェクト
    public MazeGenerator mazeGen; // MazeGeneratorへの参照（迷路の情報を取得するため）
    public float height = 30f;    // 真上からの高さ
    public float transitionTime = 2f; // カメラ移動の遷移時間
    private Vector3 offset;      // 真上から見下ろすためのオフセット
    public bool isFPSMode = false; // FPSモードかどうか
    private Vector3 fpsOffset = new Vector3(0f, 1.6f, 0f); // FPS時のカメラ位置オフセット

    private void Start()
    {
        // 迷路の中心位置を取得
        Vector3 mazeCenter = new Vector3(mazeGen.width / 2f * mazeGen.cellSize, 0f, mazeGen.height / 2f * mazeGen.cellSize);

        // 最初に迷路の真上にカメラを配置
        offset = new Vector3(0f, height, 0f);  // 迷路の中心の上に配置
        transform.position = mazeCenter + offset;
        transform.LookAt(mazeCenter); // 迷路の中心を見るようにする
    }

    private void Update()
    {
        if (isFPSMode)
        {
            transform.position = player.position + fpsOffset;
            transform.rotation = Quaternion.Euler(0f, player.eulerAngles.y, 0f);//プレイヤーの向きに合わせる
        }
        else
        {
            // 迷路の中心を真上から見下ろす視点に固定
            Vector3 mazeCenter = new Vector3(mazeGen.width / 2f * mazeGen.cellSize, 0f, mazeGen.height / 2f * mazeGen.cellSize);
            Vector3 targetPosition = mazeCenter + offset; // 迷路の中心 + オフセット
            transform.position = Vector3.Lerp(transform.position, targetPosition, transitionTime * Time.deltaTime); // スムーズに遷移
            transform.LookAt(mazeCenter); // 迷路の中心を見るようにする
        }
    }

    public void SwitchToFPS()
    {
        // FPS視点に切り替え
        isFPSMode = true;
        offset = fpsOffset; // 真上オフセットをFPS用に変更
    }
}
