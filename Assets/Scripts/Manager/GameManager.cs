﻿using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Texture2D cursorImg;
    [SerializeField] private Animator TextWindowAnimator; //대화창 애니메이터
    [SerializeField] private Animator bossGateAnimator;//보스 울타리 애니메이터
    [SerializeField] private Animator GameCloseAnimator;//게임 종료 메세지 보드 애니메이터


    [Header("Guide Objects")]
    [SerializeField] private Button GuideButton;//이동조작 가이드 버튼
    [SerializeField] private Image GuideContent;//가이드 내용
    [SerializeField] private Button GuideButtonRemove;//가이드 버튼 없애는 버튼

    [Header("Other Objects")]
    [SerializeField] private GameObject fence;//부서질 펜스 오브젝트
    [SerializeField] private GameObject invisibleWall; //문지기에게로 가는 길을 막는 투명벽
    [SerializeField] private GameObject treasureBox;//보물상자
    [SerializeField] private GameObject elixir;//엘릭서
    [SerializeField] private GameObject resetButton;//리셋 버튼



    public int MainCount { get; set; } = 0; //초기값 0
    public bool IsTalking { get; private set; }

    public GameObject friend_mushroom;
    public GameObject friend_slime; //슬라임 동료

    public bool can_hit = false;//부술 수 있는 울타리를 클릭했을 때

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        Cursor.SetCursor(cursorImg, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void MainProgress()//메인 진행 상황
    {
        switch (MainCount)
        {
            case 0://처음 시작, 주인공 소개 부분
                SoundManager.instance.PlayFirstBgm();
                DialogueManager.instance.SetDialogue(0);
                Fox_Sit();
                break;
            case 1://소개 끝, 주변 둘러보기
                Fox_StandUp();//여우 일어나기
                GuideButton.gameObject.SetActive(true);//가이드버튼 활성화
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                break;
            case 2: //첫 몬스터 발견
            case 3:// 음식 찾기
            case 5://울타리 부숨
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                if (MainCount == 5)
                {
                    SoundManager.instance.PlaySecondBgm();// 음악 변경
                }
                break;
            case 4: //슬라임 등장 후     
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                can_hit = false;
                EnemyHUD.instance.DeactiveArrow();
                break;
            case 6:
            case 10://보스 쓰러뜨림
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                break;
            case 7://버섯 등장 후
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                Destroy(invisibleWall);
                break;
            case 8://문지기 발견 후
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                SoundManager.instance.PlayThirdBgm();
                break;
            case 9://문지기 쓰러뜨림
                Invoke(nameof(Player.instance.EnableMovement), 0.6f); //여우 이동조작 가능
                GameObject.Find("Monster_Dragon_Boss").GetComponent<IEnemyController>().Animator.SetTrigger("Scream");//드래곤 포효
                SoundManager.instance.Invoke("PlayDragonRoarSound", 0.5f);
                break;
            case 11://엘릭서 획득 후
                UIManager.instance.ActiveBlackScreen();
                Player.instance.DisableMovement();
                Invoke(nameof(Ready_To_Talk), 2.2f);
                DialogueManager.instance.SetDialogue(15);
                break;
            case 12://동료들 떠난 뒤
                UIManager.instance.ActiveBlackScreen();
                Player.instance.DisableMovement();
                Invoke(nameof(Ready_To_Talk), 2.2f);
                DialogueManager.instance.SetDialogue(16);
                break;
        }
        MainCount++;
    }

    public void UpdateWorldState()
    {
        if (MainCount <= 5)//펜스 부수기 전
        {
            SoundManager.instance.PlayFirstBgm();
        }
        else if (MainCount >= 6 && MainCount <= 8)//펜스 부수고 난 이후, 문지기 발견 전
        {
            Destroy_Fence();//펜스 오브젝트 파괴
            SoundManager.instance.PlaySecondBgm();//배경음악 변경
        }
        else if (MainCount == 9)//문지기 발견 후
        {
            Destroy_Fence();//펜스 오브젝트 파괴
            SoundManager.instance.PlayThirdBgm();//배경음악 변경
        }
        else if (MainCount > 9 && MainCount <= 10)//문지기 쓰러뜨린 후
        {
            Destroy_Fence();//펜스 오브젝트 파괴
            MonsterList.instance.monsterList[6].list[0].tag = "Untagged";//태그 수정
            Destroy(GameObject.Find("Monster_DogKnight"));//문지기 파괴
            bossGateAnimator.SetTrigger("Open");
            SoundManager.instance.PlayBossBgm();//배경음악 변경
        }
        else if (MainCount >= 13)//모든 게 끝난 뒤
        {
            Destroy_Fence();
            MonsterList.instance.monsterList[6].list[0].tag = "Untagged";//태그 수정
            Destroy(GameObject.Find("Monster_DogKnight"));//문지기 파괴
            bossGateAnimator.SetTrigger("Open");
            Destroy(GameObject.Find("Monster_Dragon_Boss"));//보스 파괴
            SoundManager.instance.PlayEndingBgm();//배경음악 변경
            resetButton.SetActive(true);//리셋버튼 활성화
        }

        if (MainCount >= 5 && MainCount <= 11)
        {
            friend_slime.transform.position = Player.instance.PlayerPos + new Vector3(2f, 0, -3f);//슬라임동료 위치 조정
            friend_slime.transform.rotation = Quaternion.Euler(0, PlayerPrefs.GetFloat("RotY"), 0);
            friend_slime.SetActive(true);
        }
        if (MainCount >= 8 && MainCount <= 11)
        {
            friend_mushroom.transform.position = Player.instance.PlayerPos + new Vector3(-2f, 0, -3f);//버섯동료 위치 조정
            friend_mushroom.transform.rotation = Quaternion.Euler(0, PlayerPrefs.GetFloat("RotY"), 0);
            friend_mushroom.SetActive(true);
            Destroy(invisibleWall);
        }
    }

    void Fox_Sit()
    {
        Player.instance.Animator.SetTrigger("Sit");//여우 앉기 애니메이션 실행
        Start_Talk();
    }

    void Fox_StandUp()
    {
        Player.instance.Animator.SetTrigger("StandUp");//여우 일어나기 애니메이션 실행
    }

    public void Start_Talk()
    {
        IsTalking = true;
        Player.instance.DisableMovement();
        TextWindowAnimator.SetTrigger("Talk_On");//대화창 등장
        DialogueManager.instance.ShowDialogue();//첫 대사는 스페이스바를 누르지 않고도 바로 나오도록
    }

    public void End_Talk()
    {
        TextWindowAnimator.SetTrigger("Talk_Off");//대화창 사라지기
        if (MainCount != 1 && !Player.instance.IsDead())//첫 시작이 아닐 때만
        {
            Player.instance.EnableMovement();
        }
        if (fence)
        {
            Invoke(nameof(FenceClickOn), 1f); //1초 후 다시 펜스 클릭 시 반응
        }
        IsTalking = false;
    }



    public void ClickGuideButton()//가이드버튼 눌렀을 때
    {
        SoundManager.instance.PlayClickSound();
        if (GuideContent.IsActive())//가이드 내용이 활성화된 상태면
        {
            GuideContent.gameObject.SetActive(false); //내용 비활성화
        }
        else//비활성화 상태면
        {
            GuideContent.gameObject.SetActive(true); //내용 활성화
        }
    }

    public void ClickGuideRemove()//가이드버튼 없애는 X버튼 눌렀을 때
    {
        GuideContent.gameObject.SetActive(false); //가이드 내용과 버튼 모두 비활성화
        GuideButton.gameObject.SetActive(false);
    }

    public void PointerEnterGuideButton()//마우스 커서가 가이드버튼 위에 있을 때
    {
        GuideButtonRemove.gameObject.SetActive(true);//X버튼 보임
    }

    public void PointerExitGuideButton()//커서가 가이드버튼을 벗어났을 때
    {
        GuideButtonRemove.gameObject.SetActive(false);//X버튼 안 보임
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))//esc키 누르면
        {
            if (!closeBoard)
            {
                closeBoard = true;
                GameCloseAnimator.SetTrigger("Down");//게임 종료 메세지 보드 다운
            }
        }
    }

    public void CancelGameClose()//게임 종료 취소 버튼 누르면
    {
        SoundManager.instance.PlayClickSound();
        GameCloseAnimator.SetTrigger("Up");//보드 다시 위로
        closeBoard = false;
    }

    public void ClickFence()//레벨2 울타리 클릭했을 때
    {
        if (!fence)
        {
            Player.instance.MovementComponent.DontMove();
            fence = true;
            SoundManager.instance.PlayClickSound();
            if (Player.instance.StatusComponent.CurrentLevel >= 2)//레벨2 이상이면
            {
                if (can_hit)
                {
                    if (MainCount == 4)
                    {
                        UIManager.instance.ActiveBlackScreen();
                        Player.instance.DisableMovement();
                        Invoke(nameof(Ready_To_Talk), 2.2f);
                        DialogueManager.instance.SetDialogue(2);//울타리 부수기 가능
                    }
                    else
                    {
                        DialogueManager.instance.SetDialogue(7);//울타리 부수기 안내
                        Start_Talk();
                        can_hit = false;
                    }
                }
                else
                {
                    DialogueManager.instance.SetDialogue(6);//올바르지 않은 울타리
                    Start_Talk();
                }
            }
            else
            {
                DialogueManager.instance.SetDialogue(3);//울타리 못 부숨
                Start_Talk();
            }
        }
    }

    public void Ready_To_Talk()//슬라임/버섯과 대화 준비, 플레이어 위치 지정
    {
        CameraController.instance.SetFixedState(false);

        if (MainCount == 13)//마지막 대화
        {
            CameraController.instance.SetFieldOfView(35f);
            CameraController.instance.SetYAxisValue(0.6f);
            CameraController.instance.SetXAxisValue(25f);

            Player.instance.PlayerPos = new Vector3(348.2f, 0.545f, 304);//플레이어 위치 조정
            Player.instance.PlayerRot = Quaternion.Euler(0, 205, 0);//플레이어 회전

            friend_mushroom.SetActive(false);//동료들 비활성화
            friend_slime.SetActive(false);
        }
        else
        {
            CameraController.instance.SetFieldOfView(45f);
            if (MainCount >= 10)//보스 해치운 후
            {
                CameraController.instance.SetYAxisValue(0.6f);
                CameraController.instance.SetXAxisValue(0f);

                Player.instance.PlayerPos = new Vector3(348f, 0.545f, 305);//플레이어 위치 조정
                Player.instance.PlayerRot = Quaternion.Euler(0, 162, 0);//플레이어 회전

                friend_mushroom.transform.position = new Vector3(350.4f, 0.62f, 307f);//슬라임 위치 조정
                friend_slime.transform.position = new Vector3(351f, 0.6f, 305.5f);//슬라임 위치 조정
                Player.instance.transform.LookAt(friend_slime.transform);//서로 마주보기
                friend_mushroom.transform.LookAt(Player.instance.transform);
                friend_slime.transform.LookAt(Player.instance.transform);
            }
            else
            {
                CameraController.instance.SetYAxisValue(0.58f);
                CameraController.instance.SetXAxisValue(-40f);
            }

            if (MainCount == 4)//슬라임 등장
            {
                Player.instance.PlayerPos = new Vector3(246.5f, Player.instance.PlayerPos.y, 118);//플레이어 위치 조정
                Player.instance.PlayerRot = Quaternion.Euler(0, 307, 0);//플레이어 회전
            }
            if (MainCount == 7)//버섯 등장
            {
                Player.instance.PlayerPos = new Vector3(343f, 0.29f, 174);//플레이어 위치 조정
                friend_slime.transform.position = new Vector3(340f, 0.17f, 173);//슬라임 위치 조정
                Player.instance.PlayerRot = Quaternion.Euler(1, 308, 0);//플레이어 회전
            }
        }

        UIManager.instance.BlackAnimator.SetTrigger("BlackOff");
        Invoke(nameof(InActiveBlackScreen), 2.3f);
        Invoke(nameof(Player.instance.DisableMovement), 0.5f);
    }

    private void InActiveBlackScreen()
    {
        Start_Talk();
        if (MainCount == 4)
        {
            friend_slime.transform.position = new Vector3(Player.instance.PlayerPos.x + 11f, friend_slime.transform.position.y, Player.instance.PlayerPos.z + 2f);
            friend_slime.SetActive(true);
        }
        if (MainCount == 7)
        {
            friend_mushroom.transform.position = new Vector3(Player.instance.PlayerPos.x + 11f, friend_mushroom.transform.position.y, Player.instance.PlayerPos.z + 2f);
            friend_mushroom.SetActive(true);
        }
        UIManager.instance.DeactiveBlackScreen();
    }

    public void AfterDragonDead()//드래곤 죽은 후
    {
        UIManager.instance.ActiveBlackScreen();
        Player.instance.DisableMovement();
        Invoke(nameof(Ready_To_Talk), 2.2f);
        DialogueManager.instance.SetDialogue(13);
    }

    void FenceClickOn()//다시 펜스 클릭 가능
    {
        fence = false;
    }

    public void HitWoodenFence()
    {
        SoundManager.instance.PlayWoodSound();
        DialogueManager.instance.SetDialogue(8);
        Invoke(nameof(Start_Talk), 0.6f);
        Invoke(nameof(Destroy_Fence), 0.3f);
    }


    public void Destroy_Fence()//펜스 파괴
    {
        Destroy(fence);
    }

    void CanGetElixir()
    {
        treasureBox.tag = "Elixir";//태그 변경
    }

    public string GetObjectName(string name)
    {
        int idx = name.IndexOf("_");
        return name.Substring(idx + 1, name.Length - (idx + 1)); // 몬스터 이름만 반환
    }
}