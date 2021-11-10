﻿#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TimeTableUWP.Dialogs
{
    public sealed partial class DateSelectDialog : ContentDialog
    {
        public DateTime SelectedDate { get; set; }

        public DateSelectDialog()
        {
            InitializeComponent();
            RequestedTheme = MainPage.Theme;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog _, ContentDialogButtonClickEventArgs args)
        {
            if (datePicker.SelectedDate is null)
            {
                args.Cancel = true;
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                var date = datePicker.SelectedDate.Value.DateTime;
                SelectedDate = new(date.Year, date.Month, date.Day);
            }
        }
    }
}