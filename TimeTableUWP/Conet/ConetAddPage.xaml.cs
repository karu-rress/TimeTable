﻿#nullable enable
using System.Globalization;
using Windows.UI.Xaml.Media.Animation;

namespace TimeTableUWP.Conet;

public sealed partial class ConetAddPage : Page
{
    public static ConetHelp? Conet { get; set; }
    public ConetAddPage()
    {
        InitializeComponent();
        idText.Text = $"{Info.User.Conet!.Id} {Info.User.Conet.Name}";
        myEggs.Text = $"My Eggs: {Info.User.Conet!.Eggs}";

        if (Conet is not null) // Not creating, but modifying
        {
            idText.Text = $"{Conet.Uploader.Id} {Conet.Uploader.Name}";
            dateText.Text = "Uploaded Date: " + Conet.UploadDate.ToString("yyyy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);
            mainText.Text = "Conet Details";
            TitleTextBox.Text = Conet.Title;
            eggTextBox.Text = $"{Conet.Price?.Value}";
            BodyTextBox.Text = Conet.Body ?? string.Empty;

            // 다른 사람 글이라면
            if (Conet.Uploader.Id != Info.User.Conet.Id)
            {
                BodyTextBox.PlaceholderText = string.Empty;
                ReadOnly(TitleTextBox, eggTextBox, BodyTextBox);
                Disable(PostButton);
                myEggs.Text = string.Empty;
            }
            // 내가 쓴 글이라면
            else
            {
                Visible(DeleteButton);
            }
        }
    }

    private async void PostButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            await ShowMessageAsync("제목을 입력하세요.", "Error", Info.Settings.Theme);
            return;
        }

        bool eggExists = false;
        uint egg = 0;

        // 0이면 그냥 null 처리
        if (!eggTextBox.IsNullOrEmpty() && eggTextBox.Text[0] is not '0')
        {
            eggExists = true;
            if (!uint.TryParse(eggTextBox.Text, out egg))
            {
                await ShowMessageAsync("에그가 올바르게 입력되지 않았습니다.", "Error", Info.Settings.Theme);
                return;
            }
            if (Info.User.Conet!.Eggs.Value < egg)
            {
                await ShowMessageAsync("보유 에그보다 많은 금액을 입력하셨습니다.", "Error", Info.Settings.Theme);
                return;
            }
            if (Conet is not null)
            {
                // 기존 Egg 보충
                Info.User.Conet.Eggs += Conet.Price!.Value;
            }
            Info.User.Conet.Eggs -= new Egg(egg);
            await Info.User.Conet.SyncAsync();
        }

        ConetHelp conet = new(DateTime.Now, Info.User.Conet!, TitleTextBox.Text,
            BodyTextBox.IsNullOrWhiteSpace() ? "" : BodyTextBox.Text, eggExists ? new Egg(egg) : null);

        using SqlConnection sql = new(ChatMessageDac.ConnectionString);
        using ConetHelpDac con = new(sql);
        Disable(PostButton, TitleTextBox, BodyTextBox, eggTextBox);
       
        try
        {
            await sql.OpenAsync();
            if (Conet is null)
                await con.InsertAsync(conet);
            else
                await con.UpdateAsync(Conet.UploadDate, conet);
        }
        catch (Exception ex)
        {
            await Task.WhenAll(
                ShowMessageAsync("업로드에 실패했습니다.", "에러", Info.Settings.Theme),
                TimeTableException.HandleException(ex));
        }
        finally
        {
            Disable(PostButton, TitleTextBox, BodyTextBox, eggTextBox);
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Close() { Conet = null; Frame.Navigate(typeof(ConetPage), null, new DrillInNavigationTransitionInfo()); }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (Modified)
        {
            await ShowMessageAsync("This task has been modified. Save or discard changes and try again.", "Couldn't delete", Info.Settings.Theme);
            return;
        }

        ContentDialog dialog = new()
        {
            Title = "Delete",
            Content = "Are you sure want to delete?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
        };
        if (await dialog.ShowAsync() is not ContentDialogResult.Primary)
        {
            return;
        }

        using SqlConnection sql = new(ChatMessageDac.ConnectionString);
        using ConetHelpDac con = new(sql);

        Disable(DeleteButton);
        try
        {
            await sql.OpenAsync();
            await con.DeleteAsync(Conet!.UploadDate);

            // 기존 Egg 보충: 실제 상황에서는 완료/취소 구분해서.
            Info.User.Conet!.Eggs += Conet.Price!.Value;
            await Info.User.Conet.SyncAsync();
        }
        catch (Exception ex)
        {
            await Task.WhenAll(ShowMessageAsync("업로드에 실패했습니다.", "에러", Info.Settings.Theme),
                TimeTableException.HandleException(ex));
        }
        finally
        {
            Close();
            Enable(DeleteButton);
        }
    }

    private bool Modified => Conet is not null && !(TitleTextBox.Text == Conet.Title
            && BodyTextBox.IsSameWith(Conet.Body)
            && eggTextBox.IsSameWith(Conet.Price?.Value.ToString()));
}