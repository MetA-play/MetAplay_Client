using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public bool IsLockCursor;
    [SerializeField] private int activeWindow;
    [SerializeField] private Transform player;

    [Header("Window")]
    public GameObject menuObject;

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in players)
        {
            if (obj.GetComponent<NetworkingObject>().isMine)
                player = obj.GetComponent<Transform>();
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            OpenMenu();
        }
    }


    /// <summary>
    /// 2023.1.1 / LJ
    /// 플레이어 커서 관리
    /// </summary>
    public void CursorLock(bool locking)
    {
        player.TryGetComponent<PlayerCameraView>(out PlayerCameraView cam);
        if (!locking)
        {
            activeWindow += 1;
            cam.cursorInputForLook = false;
            cam.cursorLocked = false;
        }
        else
        {
            activeWindow -= 1;
            if (activeWindow == 0)
            {
                cam.cursorLocked = true;
                cam.cursorInputForLook = true;
            }
        }
    }

    /// <summary>
    /// 2023.1.1 / LJ
    /// 메뉴 창 열기
    /// </summary>
    void OpenMenu()
    {
        menuObject.SetActive(true);
        CursorLock(false);
    }

    /// <summary>
    /// 2023.1.1 / LJ
    /// 메뉴 창 닫기
    /// </summary>
    public void CloseMenu()
    {
        menuObject.SetActive(false);
        CursorLock(true);
    }

    /// <summary>
    /// 2023.1.1 / LJ
    /// 게임 종료
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
