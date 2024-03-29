﻿#nullable enable


using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace TimeTableUWP.Pages;
public sealed partial class ChattingPage : Page
{
    private static bool isReloadPaused = false;
    private bool isCancelRequested = false;
    private const int chatDelay = 900;
    private const string title = "GGHS Anonymous";

    private List<string> BadWords { get; set; } = new()
    {
        "씨발",
        "좆",
        "개새끼",
        "존나",
        "지랄"
    };

    public ChattingPage()
    {
        InitializeComponent();
        RequestedTheme = Info.Settings.Theme;

        viewBox.SelectionHighlightColor.Opacity = 0.35;
        viewBox.SelectionHighlightColor.Color = Info.Settings.ColorType;

        TextBoxLineColor.Color = Info.Settings.ColorType;
        textBox.BorderBrush = Info.Settings.Brush;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!Connection.IsInternetAvailable)
        {
            await ShowMessageAsync(Messages.ConnectionFailed, "Connection Error", Info.Settings.Theme);
            return;
        }

        if (!Info.User.IsSpecialLevel)
            await ShowMessageAsync(Messages.NotAuthored, title, Info.Settings.Theme);


        if (Info.User.ActivationLevel is ActivationLevel.Developer)
        {
            Visible(camoButtonA, camoButtonB, camoButtonC,
                sqlButton, delButton, notiButton, botButton);
            textBox.Margin = new(55, 0, 167, 35);
        }

        // 아예 이걸 firstloaded로 넣어버리고
        // date 기본값을 아주 먼 옛날로 해버리는 방법도 있음.

        await LoadChatsAsync();
        if (Info.User.IsSpecialLevel)
            textBox.IsEnabled = true;
        _ = ReloadChatsAsync().ConfigureAwait(false);
    }

    private async void textBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is Windows.System.VirtualKey.Enter)
            await SendMessageAsync(Info.User.ActivationLevel);
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
            return;

        switch (btn.Name)
        {
            case "camoButtonA":
                await SendMessageAsync(ActivationLevel.Azure);
                break;
            case "camoButtonB":
                await SendMessageAsync(ActivationLevel.Bisque);
                break;
            case "camoButtonC":
                await SendMessageAsync(ActivationLevel.Coral);
                break;
            case "sqlButton":
                await RunSQL(textBox.Text);
                break;
            case "delButton":
                await DeleteMessage(textBox.Text);
                break;
            case "notiButton":
                await SendNotificationAsync();
                break;
            case "botButton":
                await SendNotificationAsync(true);
                break;
        }
    }

    private async Task LoadChatsAsync()
    {
        Visible(progressGrid);

        try
        {
            DataTable dt = new();
            using (SqlConnection sql = new(ChatMessageDac.ConnectionString))
            {
                ChatMessageDac chat = new(sql);
                await sql.OpenAsync();
                await (Info.User.IsSpecialLevel ? chat.SelectAll(dt) : chat.SelectAllNotifications(dt));
            }

            StringBuilder sb = new();
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendFormat(Datas.ChatFormat, DateTime.Parse(row["Time"].ToString()),
                    Convert((byte)row["Sender"]), row["Message"]);
            }

            viewBox.Text = sb.ToString();
            ScrollViewBox();

        }
        catch (SqlException) // 이건 내가 대응할 수가 없음. 그냥 Swallow.
        {
            await ShowMessageAsync("메시지를 불러오는 데 실패했습니다.\n다른 탭으로 나간 뒤, 다시 GGHS Anonymous로 들어오면\n재접속을 시도합니다.", title, Info.Settings.Theme);
            return;
        }
        finally
        {
            Invisible(progressGrid);
        }
    }

    private async Task ReloadChatsAsync()
    {
        while (true)
        {
            if (isCancelRequested)
                return;

            if (isReloadPaused)
            {
                await Task.Delay(400);
                continue;
            }

            if (!Connection.IsInternetAvailable)
            {
                await ShowMessageAsync("네트워크 연결을 확인하세요.", "Connection Error", Info.Settings.Theme);
                await Task.Delay(1900);
                continue;
            }


            SqlConnection sql = new(ChatMessageDac.ConnectionString);
            ChatMessageDac chat = new(sql);

            await sql.OpenAsync();

            if (await (Info.User.IsSpecialLevel
                ? chat.GetNewMessagesCountAsync()
                : chat.GetNewNotificationsCountAsync()) is 0)
            {
                sql.Close();
                await Task.Delay(chatDelay);
                continue;
            }

            viewBox.Text += await (Info.User.IsSpecialLevel
                ? chat.GetNewMessagesAsync()
                : chat.GetNewNotificationsAsync());

            sql.Close();
            ScrollViewBox();
        }
    }


    private async Task SendMessageAsync(ActivationLevel userLevel)
    {
        if (textBox.IsNullOrWhiteSpace())
        {
            await ShowMessageAsync("보낼 메시지를 입력하세요.", title, Info.Settings.Theme);
            return;
        }
        // 욕설 필터링
        if (BadWords.Any(textBox.Text.Contains))
        {
            ContentDialog dialog = new()
            {
                Content = "부적절한 말들이 포함되어 있습니다. 그래도 보내시겠습니까?",
                Title = title,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            if (await dialog.ShowAsync() is ContentDialogResult.None)
                return;
        }

        using SqlConnection sql = new() { ConnectionString = ChatMessageDac.ConnectionString };
        using ChatMessageDac chatmessage = new(sql, PrepareSend, DisposeSend);
        try
        {
            await sql.OpenAsync();
            await chatmessage.InsertAsync(Convert(userLevel), textBox.Text);
        }
        catch (Exception ex)
        {
            await Task.WhenAll(ShowMessageAsync("채팅 전송에 실패했습니다.\n" + ex.ToString(), title, Info.Settings.Theme),
                TimeTableException.HandleException(ex));
        }
    }

    private async Task SendNotificationAsync(bool isBot = false)
    {
        if (textBox.IsNullOrWhiteSpace())
        {
            await ShowMessageAsync("보낼 공지를 입력하세요.", title, Info.Settings.Theme);
            return;
        }
        // 욕설 필터링

        using SqlConnection sql = new() { ConnectionString = ChatMessageDac.ConnectionString };
        using ChatMessageDac chatmessage = new(sql, PrepareSend, DisposeSend);
        try
        {
            await sql.OpenAsync();
            if (isBot)
                await chatmessage.InsertAsync((byte)ChatMessageDac.Sender.GttBot, textBox.Text);
            else
                await chatmessage.InsertAsync((byte)ChatMessageDac.Sender.Notification, "【공지】 " + textBox.Text);
        }
        catch (Exception ex)
        {
            await Task.WhenAll(ShowMessageAsync("공지 등록 또는 봇 전송에 실패했습니다.\n" + ex.ToString(), title, Info.Settings.Theme),
                TimeTableException.HandleException(ex));
        }
    }

    private async Task RunSQL(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await ShowMessageAsync("Enter query.", title, Info.Settings.Theme);
            return;
        }
        using SqlConnection sql = new(ChatMessageDac.ConnectionString);
        using ChatMessageDac chat = new(sql, PrepareSend, DisposeSend);
        try
        {
            await sql.OpenAsync();
            await chat.RunSqlCommand(query);
        }
        catch (Exception e)
        {
            await Task.WhenAll(ShowMessageAsync("Failed to run the SQL query. \n" + e.ToString(), title, Info.Settings.Theme),
            TimeTableException.HandleException(e));
        }
    }

    private async Task DeleteMessage(string message)
    {
        if (textBox.IsNullOrWhiteSpace())
        {
            ContentDialog dialog = new()
            {
                Content = "Nothing seems to be entered. Do you really want to proceed?",
                Title = title,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
            };
            if (await dialog.ShowAsync() is ContentDialogResult.None)
                return;
        }

        using SqlConnection sql = new(ChatMessageDac.ConnectionString);
        using ChatMessageDac chat = new(sql, PrepareSend, DisposeSend);
        try
        {
            await sql.OpenAsync();
            await chat.DeleteAsync(message);
        }
        catch (SqlException e)
        {
            await ShowMessageAsync("Error:" + e.ToString(), "Unable to delete message");
        }
    }

    private void ScrollViewBox()
    {
        Grid? grid = VisualTreeHelper.GetChild(viewBox, 0) as Grid;
        for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
        {
            if (VisualTreeHelper.GetChild(grid, i) is not ScrollViewer sv)
                continue;
            sv.ChangeView(0.0f, sv.ExtentHeight, 1.0f, true);
            break;
        }
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    => isCancelRequested = true;

    private byte Convert(ActivationLevel level) => level switch
    {
        ActivationLevel.Developer => 0,
        ActivationLevel.Azure => 1,
        ActivationLevel.Bisque => 2,
        ActivationLevel.Coral => 5,
        _ => throw new ArgumentException($"ChattingPage.Convert(ActivationLevel): unknown '{level}'.")
    };

    public static string Convert(byte level) => level switch
    {
        0 => "Karu",
        1 or 3 => "Azure",
        2 or 4 => "Bisque",
        5 => "Coral",
        8 => "Karu✅",
        9 => "GTTℹ️",
        _ => level.ToString(),
    };

    // Callback
    private void PrepareSend()
    {
        isReloadPaused = true;
        textBox.IsEnabled = false;
    }

    // Callback
    private void DisposeSend()
    {
        isReloadPaused = false;
        textBox.IsEnabled = true;
        textBox.Text = string.Empty;
        ScrollViewBox();
    }
}
