﻿#nullable enable

namespace TimeTableUWP;
public static class Sensitive
{
    public const string GTTMail = "gghstimetable@gmail.com";
    public const string KaruMail = "nsun527@naver.com";
}

internal static class ActivateKeys
{
    public const string Developer = "RRESS-KARU527";
    public const string Azure = "AZURE-A8VP36N";
    public const string Bisque = "BSQUE-LX5MBH3";
    public const string Coral = "CORAL-3GGHS10";
}

internal static class SubTitles
{
    public const string Developer = "Welcome to the Ultimate GTT5, Karu";
    public const string Azure = "Let's try our hardest, our last journey.";
    public const string Bisque = "We're the ones who've made it this far";
    public const string Coral = "Now we are the K-高3, LOL";
}

public static class Datas
{
    public const string Version = "5.0-RC1.5";
    public const string GTTWithVer = "GGHS Time Table 5";
    public const string ChatFormat = "[{0:MM/dd HH:mm}] {1}:\t{2}\n";
}

internal static class Messages
{
    public const string ErrorChat = "⛔ERROR⛔ 카루님, GTT 에러 발생. 메일 확인 요망. ({0})";
    public const string FeedbackChat = "📧Feedback📧 카루님, GTT 피드백이 도착했습니다. (V{0})";

    public static class Dialog
    {
        public const string Activate = "{0}\n카루에게 제공받은 인증키를 입력하세요.\n인증키는 5자리의 영문+7자리의 숫자/영문 조합으로 구성되어 있습니다."
            +"\n인증키를 모르는 경우 설정 창의 'Send Feedback' 기능을 이용하세요.";
    }

    public const string WhatsNew =
@"- 알람 기능 삭제
- (GTD) 수능 디데이 카운터 추가
- 3-1 사탐방 클래스룸 추가
- GGHS Todo / GGHS Anonymous UI 조정";
    public static string Updated => @$"GGHS Time Table이 V{Info.Version}{Info.Version.GetLastNumber() switch
    {
        '0' or '3' or '6' => "으로",
        _ => "로",
    }} 업데이트 되었습니다.

V{Info.Version}에서 변경된 기능
{WhatsNew}

GTT4 부터 To-do 기능이 추가되었습니다. 상단바에서 'To do'를 선택하면 
수행평가 일정 목록을 관리할 수 있습니다. GTT5에서는 Anonymous 채팅
을 통해 익명톡에서 자유롭게 대화가 가능합니다.";

    public const string Welcome =
    @"환영합니다, Rolling Ress의 카루입니다.
GGHS Time Table을 설치해주셔서 감사합니다. 

*** 임시시간표가 나오지 않아 시간표 기능은 동작하지 않습니다.
*** GGHS Anonymous (익명챗) 기능을 우선 시험해보세요 :)

자신의 선택과목을 선택하고, 시간표를 누르면 해당 시간의 
줌 링크와 클래스룸 링크가 띄워집니다.
상단바에서 To do (GTD)를 선택할 경우 
각종 수행평가를 기록하고, 관리할 수 있습니다.
Anonymous 메뉴에선 GGHS 10기 학생들이 자유롭게
익명 채팅이 가능하며, GTT 관련 공지를 확인하실 수 있습니다.
상단바 오른쪽 끝 톱니바퀴 모양을 통해 
설정 메뉴에 들어가실 수 있습니다.

줌 링크가 누락된 경우, 설정 메뉴에서 'Feedback'을 통해
줌 링크/ID/비밀번호를 전달해주시면 바로 추가하겠습니다.";

    public const string About = $@"GGHS Time Table V{Datas.GTTWithVer}

환영합니다, Rolling Ress의 카루입니다.
GGHS Time Table을 설치해주셔서 감사합니다.

자신의 선택과목을 선택하고, 시간표를 누르면 해당 시간의
줌 링크와 클래스룸 링크가 띄워집니다.
왼쪽 위 메뉴에서 'To do'를 선택하면 수행평가 일정 관리를,
'Anonymous'를 선택하면 10기용 익명 채팅을 할 수 있습니다.

기능에 문제가 있거나, 줌 링크가 누락이 된 반 혹은 과목이 있다면
설정 창의 'Send Feedback' 버튼을 통해 제보해주시면 감사하겠습니다.
(혹은 Anonymous에서 바로 메시지를 주셔도 됩니다.)

카루 블로그 링크:
";

    public const string Troubleshoot = "GGHS Time Table 5 사용중 문제가 발생했나요?\n\n"
        + "오류가 난 경우 대부분 개발자에게 자동으로 보고되며, 별다른 조치를 취하실 필요가 없습니다. "
        + "만약 오류 창이 뜨거나 경고 메시지가 뜬 경우 해당 화면을 캡처해서 제게 보내주시기 바랍니다. "
        + "혹은, 'Send Feedback' 버튼을 통해 오류를 제보해주세요.\n\n"
        + "선택과목 및 ZOOM 링크에 오류가 있는 경우에도 제보 부탁드립니다.\n";

    public const string NotAuthored = @"현재 레벨에서는 GGHS Anonymous를 이용하실 수 없습니다.
공지 읽기 전용 모드로 동작합니다.
의견이 있으신 경우 설정창의 'Send Feedback' 기능을 이용하실 수 있으며,
다른 인증키를 받으신 경우 설정창에서 인증 레벨을 변경할 수 있습니다.";
}
