﻿#nullable enable

using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using static RollingRess.StaticClass;
using static System.DayOfWeek;
using GGHS.Grade2.Semester2;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Background;

namespace TimeTableUWP
{

    public sealed partial class MainPage : Page
    {
        private string[,] SubjectTable { get; set; } // string[5, 7]
        void InitializeUI()
        {
            Disable(classComboBox, langComboBox, special1ComboBox, special2ComboBox, scienceComboBox);
            RequestedTheme = SettingsPage.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light;
            SetColor();
            if (comboBoxSelection.grade is not -1)
            {
                (gradeComboBox.SelectedIndex, classComboBox.SelectedIndex, langComboBox.SelectedIndex, special1ComboBox.SelectedIndex,
                    special2ComboBox.SelectedIndex, scienceComboBox.SelectedIndex) = comboBoxSelection;
            }
        }

        private void SetColor()
        {
            foreach (var border in new[] { monBorder, tueBorder, wedBorder, thuBorder, friBorder })
                border.Background = new SolidColorBrush(SaveData.ColorType);

            //foreach (var comboBox in ComboBoxes)
            //    comboBox.BorderBrush = new SolidColorBrush(SaveData.ColorType);
        }

        private void DrawTimeTable()
        {
            timeTable.ResetByClass(@class);
            SubjectTable = SetArrayByClass();

            // 월 6, 7 / 금 5, 6은 어차피 창체, 금 7도 어차피 홈커밍
            AssignButtonsByTable(SubjectTable);
            
            mon6Button.Content = mon7Button.Content = fri5Button.Content = fri6Button.Content =
            SubjectTable[0, 5] = SubjectTable[0, 6] = SubjectTable[4, 4] = SubjectTable[4, 5] = Subjects.CellName.Others;
            fri7Button.Content = SubjectTable[4, 6] = Subjects.CellName.HomeComing;
        }

        private void AssignButtonsByTable(string[,] subjectTable)
        {
            var subjects = ((IEnumerable)subjectTable).Cast<string>();
            var lists = Buttons.Zip(subjects, (Button btn, string subject) => (btn, subject));
            foreach (var (btn, subject) in lists)
                btn.Content = subject;
        }
        private void SetSubText() => mainText2.Text = SaveData.ActivateStatus switch
        {
            ActivateLevel.Developer => "Welcome, Karu",
            ActivateLevel.Grade2 => "We're the ones who've made it this far",
            ActivateLevel.Insider => "GTT3 Insider Preview",
            _ => string.Empty
        };
        private async Task LoopTimeAsync()
        {
            (int day, int time) pos;
            while (true)
            {
                await Task.Delay(100);
                now = DateTime.Now;

                void SetClock()
                {
                    clock.Text = now.ToString(SettingsPage.Use24Hour ? "HH:mm" : "hh:mm");
                    amorpmBox.Text = SettingsPage.Use24Hour 
                        ? string.Empty : now.ToString("tt", CultureInfo.InvariantCulture);
                    dateBlock.Text = now.ToString(SettingsPage.DateFormat switch
                    {
                        DateType.MMDDYYYY => "MM/dd/yyyy",
                        DateType.YYYYMMDD => "yyyy/MM/dd",
                        DateType.YYYYMMDD2 => "yyyy-MM-dd",
                        _ => throw new NotImplementedException()
                    });
                    dayBlock.Text = now.ToString("ddd", CultureInfo.CreateSpecificCulture("en-US"));
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, SetClock);

                void RefreshColor()
                {
                    foreach (var item in Buttons)
                    {
                        item.RequestedTheme = SettingsPage.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light;
                        item.Background = new SolidColorBrush(SettingsPage.IsDarkMode
                            ? Color.FromArgb(0xEE, 0x34, 0x34, 0x34)
                            : Color.FromArgb(0xEE, 0xF4, 0xF4, 0xF4));
                        item.Foreground = new SolidColorBrush(SettingsPage.IsDarkMode ? Colors.White : Colors.Black);
                    }
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RefreshColor);

                if (now.DayOfWeek is Sunday or Saturday || now.Hour is >= 17 or < 9 or 13)
                    continue;

                pos.day = (int)now.DayOfWeek;
                pos.time = now.Hour switch
                {
                    9 or 10 or 11 or 12 => now.Hour - 8,
                    14 or 15 or 16 => now.Hour - 9,
                    _ => throw new DataAccessException($"Hour is not in 9, 10, 11, 12, 14, 15, 16. given {now.Hour}.")
                };

                void ChangeColor()
                {
                    var brush = new SolidColorBrush(SaveData.ColorType);
                    Buttons.ElementAt((7 * (pos.day - 1)) + (pos.time - 1)).Background = brush;
                    if (pos.time <= 6)
                        Buttons.ElementAt((7 * (pos.day - 1)) + pos.time).Foreground = brush;
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ChangeColor);

                async void SendToast()
                {
                    // 개발자나 Insider가 아니라면 토스트 메시지를 출력하지 않는다.
                    if (SaveData.IsNotDeveloperOrInsider)
                        return;

                    now = DateTime.Now;
                    int hour = now.Hour;
                    // 날짜 => 시간 순서대로 확인
                    if ((DayOfWeek)now.Day is not Saturday and not Sunday && hour is (>= 9 and <= 12) or (>= 14 and <= 16))
                        if ((now.Minute, now.Second) is (57, 0))
                        {
                            // 동아리(2)나 홈커밍일 때는 토스트 알림 없음.
                            if (pos is ((int)Friday, 7) or ((int)Friday, 6))
                                return; 

                            // TODO: 이때는 뭘 하지 버튼 없는 토스트?
                            if (pos is ((int)Monday, 6) or ((int)Monday, 7) or ((int)Friday, 5))
                                return; 

                            // TimeTable 표에서 현재 날짜와 시간을 통해 과목 꺼내기
                            string subject = SubjectTable[pos.day - 1, pos.time - 1];
                            if (GetClassZoomLink().TryGetValue(subject, out var zoomInfo) is false || (zoomInfo is null))
                                return; // 값이 없다? 그럼 꺼내지 말아야지.

                            ToastContentBuilder toast = new ToastContentBuilder()
                                // TODO: 무음모드 일 경우에는 오디오를 꺼야 한다. 빼버리기.
                                .AddAppLogoOverride(new Uri("ms-appx:///Assets/SecurityAndMaintenance.png")) // 동그라미 i 표시
                                .AddAudio(new Uri("ms-appx:///Assets/Alarm01.wav")) // 알람소리
                                .AddText("다음 수업이 3분 내에 시작됩니다.", hintMaxLines: 1) // 안내문
                                .AddText($"[{subject}] - {zoomInfo.Teacher} 선생님") // 과목 및 선생님
                                .AddText(hour > 12 ?
                                    $"{hour - 12}:00 PM - {hour - 12}:50 PM" :
                                    $"{hour}:00 AM - {hour}:50 AM"); // 시간은 당연한 거고

                            if (zoomInfo.Link is not null)
                                toast.AddButton(new ToastButton()
                                    .SetContent("ZOOM 열기")
                                    .AddArgument("action", "zoom")
                                    .AddArgument("zoomUrl", zoomInfo.Link)
                                    .SetBackgroundActivation());

                            if (zoomInfo.ClassRoom is not null)
                                toast.AddButton(new ToastButton()
                                    .SetContent("클래스룸 열기")
                                    .AddArgument("action", "classRoom")
                                    .AddArgument("classRoomUrl", zoomInfo.ClassRoom)
                                    .SetBackgroundActivation());

                            toast.Show();

                            const string taskName = "ToastZoomOpen";

                            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
                                return;

                            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
                            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
                            {
                                Name = taskName
                            };
                            builder.SetTrigger(new ToastNotificationActionTrigger());
                            BackgroundTaskRegistration registration = builder.Register();
                        }
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, SendToast);
            }
        }
    }
}