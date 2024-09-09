﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Cinemachine;

public class GameDirector : MonoBehaviour
{
    public static GameDirector instance;
    [SerializeField] Texture2D cursorImg;

    public GameObject Player;
    [SerializeField] Animator TextWindowAnimator; //대화창 애니메이터
    public bool firstStart = false;
    public int mainCount = 0;
    public GameObject Warning;
    public bool talking = false; //대화 중인지 아닌지 판단
    public GameObject fenceObject;//부서질 펜스 오브젝트
    public GameObject gkDiscover;//문지기 감지 오브젝트
    public GameObject wall; //문지기에게로 가는 길을 막던 투명벽
    public Animator bossGate;//보스 울타리 애니메이터
    public GameObject treasureBox;//보물상자
    public GameObject elixir;//엘릭서
    public GameObject resetButton;//리셋 버튼

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        Cursor.SetCursor(cursorImg, Vector2.zero, CursorMode.ForceSoftware);

        //MainProgress();
        //DevelopCheat();
    }

    void DevelopCheat()
    {
    }

    public void MainProgress()//메인 진행 상황
    {
        switch(mainCount)
        {
            case 0://처음 시작, 주인공 소개 부분
                SoundManager.instance.PlayFirstBgm();
                firstStart = true;
                Invoke("Fox_Sit", 1.7f);
                DialogueController.instance.SetDialogue(0);
                break;
            case 1://소개 끝, 주변 둘러보기
                Fox_StandUp();//여우 일어나기
                Warning.SetActive(true);//이동조작 주의사항 활성화
                GuideButton.gameObject.SetActive(true);//가이드버튼 활성화
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                Invoke("InActiveWarning", 4f); //이동조작 주의사항 비활성화
                firstStart = false;
                break;
            case 2: //첫 몬스터 발견
            case 3:// 음식 찾기
            case 5://울타리 부숨
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                if(mainCount == 5)
                {
                    SoundManager.instance.PlaySecondBgm();// 음악 변경
                }
                break;
            case 4: //슬라임 등장 후     
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                can_hit = false;
                MonsterHPBar.instance.Invoke(nameof(MonsterHPBar.instance.Fence_arrow), 0.2f);
                break;
            case 6:
            case 10://보스 쓰러뜨림
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                break;
            case 7://버섯 등장 후
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                Destroy(wall);
                break;
            case 8://문지기 발견 후
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                SoundManager.instance.PlayThirdBgm();
                Destroy(gkDiscover);//문지기 감지 오브젝트 파괴, 콜라이더가 있어서 통행에 방해될 수 있음
                break;
            case 9://문지기 쓰러뜨림
                Invoke("Fox_Can_Move", 0.6f); //여우 이동조작 가능
                //나중에 풀어 MonsterHPBar.instance.boss.GetComponent<IEnemyController>().animator.SetTrigger("Scream");//드래곤 포효
                SoundManager.instance.Invoke(("PlayDragonRoarSound"), 0.5f);
                break;
            case 11://엘릭서 획득 후
                PlayerInfoManager.instance.BlackScreen.SetActive(true);
                PlayerInfoManager.instance.blackAnimator.SetTrigger("BlackOn");
                Fox_Cant_Move();
                Invoke(nameof(Ready_To_Talk), 2.2f);
                DialogueController.instance.SetDialogue(15);
                break;
            case 12://동료들 떠난 뒤
                PlayerInfoManager.instance.BlackScreen.SetActive(true);
                PlayerInfoManager.instance.blackAnimator.SetTrigger("BlackOn");
                Fox_Cant_Move();
                Invoke(nameof(Ready_To_Talk), 2.2f);
                DialogueController.instance.SetDialogue(16);
                break;
        }
        mainCount++;
    }

    public void Fox_Can_Move()
    {
        Player.GetComponent<ThirdPlayerMovement>().enabled = true;//플레이어 이동 가능
        CameraController.instance.SetFixedState(false);//카메라 확대축소 가능
        Player.GetComponent<CharacterController>().enabled = true;//캐릭터 컨트롤러 켜기
    }

    public void Fox_Cant_Move()
    {
        Player.GetComponent<ThirdPlayerMovement>().enabled = false;//플레이어 이동 불가
        CameraController.instance.SetFixedState(true); //카메라 회전 불가
        Player.GetComponent<CharacterController>().enabled = false;//캐릭터 컨트롤러 끄기
        ThirdPlayerMovement.instance.DontMove();
    }

    void Fox_Sit()
    {
        ThirdPlayerMovement.instance.foxAnimator.SetTrigger("Sit");//여우 앉기 애니메이션 실행
        Start_Talk();
    }

    void Fox_StandUp()
    {
        ThirdPlayerMovement.instance.foxAnimator.SetTrigger("StandUp");//여우 일어나기 애니메이션 실행
    }

    public void Start_Talk()
    {
        talking = true;
        Fox_Cant_Move();
        TextWindowAnimator.SetTrigger("Talk_On");//대화창 등장
        DialogueController.instance.ShowDialogue();//첫 대사는 스페이스바를 누르지 않고도 바로 나오도록
    }

    public void End_Talk()
    {
        TextWindowAnimator.SetTrigger("Talk_Off");//대화창 사라지기
        if(!firstStart && !PlayerInfoManager.instance.death)//첫 시작이 아닐 때만
        {
            Fox_Can_Move();
        }
        if(fence)
        {
            Invoke(nameof(FenceClickOn), 1f); //1초 후 다시 펜스 클릭 시 반응
        }
        talking = false;
    }

    void InActiveWarning()
    {
        Warning.SetActive(false);
    }

    public Button GuideButton;//이동조작 가이드 버튼
    public Image GuideContent;//가이드 내용
    public Button GuideButtonRemove;//가이드 버튼 없애는 버튼

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


    public Animator GameCloseAnimator;//게임 종료 메세지 보드 애니메이터

    bool closeBoard = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))//esc키 누르면
        {
            if(!closeBoard)
            {
                closeBoard = true;
                GameCloseAnimator.SetTrigger("Down");//게임 종료 메세지 보드 다운
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) //체력 치트
        {
            PlayerInfoManager.instance.HPCheat();
        }

        if (Input.GetKeyDown(KeyCode.R) && Input.GetKeyDown(KeyCode.T))
        {
            PlayerInfoManager.instance.ResetData();
        }
    }

    public void CancelGameClose()//게임 종료 취소 버튼 누르면
    {
        SoundManager.instance.PlayClickSound();
        GameCloseAnimator.SetTrigger("Up");//보드 다시 위로
        closeBoard = false;
    }

    public bool fence = false;
    public GameObject friend_slime; //슬라임 동료
    public bool can_hit = false;//부술 수 있는 울타리를 클릭했을 때
    public GameObject right_Fence;
    public void ClickFence()//레벨2 울타리 클릭했을 때
    {
        if(!fence)
        {
            ThirdPlayerMovement.instance.DontMove();
            fence = true;
            SoundManager.instance.PlayClickSound();
            if (PlayerInfoManager.instance.level >= 2)//레벨2 이상이면
            {
                if(can_hit)
                {    
                    if(mainCount == 4)
                    {
                        PlayerInfoManager.instance.BlackScreen.SetActive(true);
                        PlayerInfoManager.instance.blackAnimator.SetTrigger("BlackOn");
                        Fox_Cant_Move();
                        Invoke(nameof(Ready_To_Talk), 2.2f);
                        DialogueController.instance.SetDialogue(2);//울타리 부수기 가능
                    }
                    else
                    {
                        DialogueController.instance.SetDialogue(7);//울타리 부수기 안내
                        Start_Talk();
                        can_hit = false;
                    }
                }
                else
                {
                    DialogueController.instance.SetDialogue(6);//올바르지 않은 울타리
                    Start_Talk();
                }               
            }
            else
            {
                DialogueController.instance.SetDialogue(3);//울타리 못 부숨
                Start_Talk();
            }
        }
    }

    public void Ready_To_Talk()//슬라임/버섯과 대화 준비, 플레이어 위치 지정
    {
        CameraController.instance.SetFixedState(false);

        if(mainCount == 13)//마지막 대화
        {
            CameraController.instance.SetFieldOfView(35f);
            CameraController.instance.SetYAxisValue(0.6f);
            CameraController.instance.SetXAxisValue(25f);

            Player.transform.position = new Vector3(348.2f, 0.545f, 304);//플레이어 위치 조정
            Player.transform.rotation = Quaternion.Euler(0, 205, 0);//플레이어 회전

            friend_mushroom.SetActive(false);//동료들 비활성화
            friend_slime.SetActive(false);
        }
        else
        {
            CameraController.instance.SetFieldOfView(45f);
            if (mainCount >= 10)//보스 해치운 후
            {
                CameraController.instance.SetYAxisValue(0.6f);
                CameraController.instance.SetXAxisValue(0f);

                Player.transform.position = new Vector3(348f, 0.545f, 305);//플레이어 위치 조정
                Player.transform.rotation = Quaternion.Euler(0, 162, 0);//플레이어 회전

                friend_mushroom.transform.position = new Vector3(350.4f, 0.62f, 307f);//슬라임 위치 조정
                friend_slime.transform.position = new Vector3(351f, 0.6f, 305.5f);//슬라임 위치 조정
                Player.transform.LookAt(friend_slime.transform);//서로 마주보기
                friend_mushroom.transform.LookAt(Player.transform);
                friend_slime.transform.LookAt(Player.transform);
            }
            else
            {
                CameraController.instance.SetYAxisValue(0.58f);
                CameraController.instance.SetXAxisValue(-40f);
            }

            if (mainCount == 4)//슬라임 등장
            {
                Player.transform.position = new Vector3(246.5f, Player.transform.position.y, 118);//플레이어 위치 조정
                Player.transform.rotation = Quaternion.Euler(0, 307, 0);//플레이어 회전
            }
            if (mainCount == 7)//버섯 등장
            {
                Player.transform.position = new Vector3(343f, 0.29f, 174);//플레이어 위치 조정
                friend_slime.transform.position = new Vector3(340f, 0.17f, 173);//슬라임 위치 조정
                Player.transform.rotation = Quaternion.Euler(1, 308, 0);//플레이어 회전
            }

        }
       
        PlayerInfoManager.instance.blackAnimator.SetTrigger("BlackOff");
        Invoke(nameof(InActiveBlackScreen), 2.3f);
        Invoke(nameof(Fox_Cant_Move), 0.5f);
    }

    public GameObject friend_mushroom;
    private void InActiveBlackScreen()
    {      
        Start_Talk();
        if (mainCount == 4)
        {
            friend_slime.transform.position = new Vector3(Player.transform.position.x + 11f, friend_slime.transform.position.y, Player.transform.position.z + 2f);
            friend_slime.SetActive(true);
        }
        if (mainCount == 7)
        {
            friend_mushroom.transform.position = new Vector3(Player.transform.position.x + 11f, friend_mushroom.transform.position.y, Player.transform.position.z + 2f);
            friend_mushroom.SetActive(true);
        }
        PlayerInfoManager.instance.BlackScreen.SetActive(false);
    }

    public void AfterDragonDead()//드래곤 죽은 후
    {
        PlayerInfoManager.instance.BlackScreen.SetActive(true);
        PlayerInfoManager.instance.blackAnimator.SetTrigger("BlackOn");
        Fox_Cant_Move();
        Invoke(nameof(Ready_To_Talk), 2.2f);
        DialogueController.instance.SetDialogue(13);
    }

    void FenceClickOn()//다시 펜스 클릭 가능
    {
        fence = false;
    }

    public void HitWoodenFence()
    {
        SoundManager.instance.PlayWoodSound();
        DialogueController.instance.SetDialogue(8);
        Invoke(nameof(Start_Talk),0.6f);
        Invoke(nameof(Destroy_Fence), 0.3f);

    }


    public void Destroy_Fence()//펜스 파괴
    {
        Destroy(fenceObject);
    }

    void CanGetElixir()
    {
        treasureBox.tag = "Elixir";//태그 변경
    }

    public string GetObjectName(string name)
    {
        int idx = name.IndexOf("_");
        return name.Substring(idx+1, name.Length - (idx+1)); // 몬스터 이름만 반환
    }
}
