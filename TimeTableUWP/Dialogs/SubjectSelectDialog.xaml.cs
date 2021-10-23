﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTableUWP.Dialogs
{
    public sealed partial class SubjectSelectDialog : ContentDialog
    {
        public string SelectedSubject { get; set; }
        public List<string> Subject { get; } = new()
        {
            "독서",
            "수학Ⅱ",
            "수학과제탐구",
            "과학사",
            "생활과 과학",
            "운동과 건강",
            "창의적문제해결기법",
            "스페인어Ⅰ",
            "중국어Ⅰ",
            "일본어Ⅰ",
            "심화영어Ⅰ",
            "국제경제",
            "국제정치",
            "비교문화",
            "동양근대사",
            "세계 역사와 문화",
            "현대정치철학의 이해",
            "세계 지역 연구",
            "공간 정보와 공간 분석",
        };
        public SubjectSelectDialog()
        {
            InitializeComponent();
            RequestedTheme = MainPage.Theme;
            SubjectComboBox.ItemsSource = Subject;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (SubjectComboBox.SelectedIndex is -1)
            {
                args.Cancel = true;
                TextBlock.Visibility = Visibility.Visible;
            }
            if (SubjectComboBox.SelectedItem is string s)
            {
                SelectedSubject = s;
            }
        }
    }
}