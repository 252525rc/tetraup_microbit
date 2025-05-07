using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;      // �v���C���[�I�u�W�F�N�g
    public MazeGenerator mazeGen; // MazeGenerator�ւ̎Q�Ɓi���H�̏����擾���邽�߁j
    public float height = 30f;    // �^�ォ��̍���
    public float transitionTime = 2f; // �J�����ړ��̑J�ڎ���
    private Vector3 offset;      // �^�ォ�猩���낷���߂̃I�t�Z�b�g
    public bool isFPSMode = false; // FPS���[�h���ǂ���
    private Vector3 fpsOffset = new Vector3(0f, 1.6f, 0f); // FPS���̃J�����ʒu�I�t�Z�b�g

    private void Start()
    {
        // ���H�̒��S�ʒu���擾
        Vector3 mazeCenter = new Vector3(mazeGen.width / 2f * mazeGen.cellSize, 0f, mazeGen.height / 2f * mazeGen.cellSize);

        // �ŏ��ɖ��H�̐^��ɃJ������z�u
        offset = new Vector3(0f, height, 0f);  // ���H�̒��S�̏�ɔz�u
        transform.position = mazeCenter + offset;
        transform.LookAt(mazeCenter); // ���H�̒��S������悤�ɂ���
    }

    private void Update()
    {
        if (isFPSMode)
        {
            transform.position = player.position + fpsOffset;
            transform.rotation = Quaternion.Euler(0f, player.eulerAngles.y, 0f);//�v���C���[�̌����ɍ��킹��
        }
        else
        {
            // ���H�̒��S��^�ォ�猩���낷���_�ɌŒ�
            Vector3 mazeCenter = new Vector3(mazeGen.width / 2f * mazeGen.cellSize, 0f, mazeGen.height / 2f * mazeGen.cellSize);
            Vector3 targetPosition = mazeCenter + offset; // ���H�̒��S + �I�t�Z�b�g
            transform.position = Vector3.Lerp(transform.position, targetPosition, transitionTime * Time.deltaTime); // �X���[�Y�ɑJ��
            transform.LookAt(mazeCenter); // ���H�̒��S������悤�ɂ���
        }
    }

    public void SwitchToFPS()
    {
        // FPS���_�ɐ؂�ւ�
        isFPSMode = true;
        offset = fpsOffset; // �^��I�t�Z�b�g��FPS�p�ɕύX
    }
}
