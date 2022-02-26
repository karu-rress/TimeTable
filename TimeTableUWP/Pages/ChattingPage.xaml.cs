﻿#nullable enable
#define RC

using Windows.Storage;

namespace TimeTableUWP.Pages;
public sealed partial class ChattingPage : Page
{
    private static bool isFirstLoaded = true;
    private static bool isReloadPaused = false;
    private bool isCancelRequested = false;
    private const int chatDelay = 600;
    private const string title = "GGHS Anonymous";
    private static bool authored = false;
    private static string ConnectionString { get; set; } = "";

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
        
        viewBox.SelectionHighlightColor.Opacity = 0.35;
        viewBox.SelectionHighlightColor.Color = Info.Settings.ColorType;

        TextBoxLineColor.Color = Info.Settings.ColorType; 
        textBox.BorderBrush = new SolidColorBrush(Info.Settings.ColorType);
    }


    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!Connection.IsInternetAvailable)
        {
            await ShowMessageAsync("인터넷에 연결하지 못했습니다.\n네트워크 연결을 확인하세요.", "Connection Error");
            return;
        }

        if (isFirstLoaded)
            authored = await TimeTablePage.AuthorAsync("여기는 GTT 유저 대화방으로, Azure/Bisque 레벨만 이용할 수 있습니다.");

        if (!authored)
        {
            await ShowMessageAsync(@"현재 레벨에서는 GGHS Anonymous를 이용하실 수 없습니다.
공지 읽기 전용 모드로 동작합니다.
의견이 있으신 경우 설정창의 'Send Feedback' 기능을 이용하실 수 있으며,
다른 인증키를 받으신 경우 설정창에서 인증 레벨을 변경할 수 있습니다.", "GGHS Anonymous");
        }
            // 인증이 안 된 상황    

        if (Info.User.ActivationLevel is ActivationLevel.Developer)
        {
            camoButtonA.Visibility = camoButtonB.Visibility = sqlButton.Visibility
                = delButton.Visibility = notiButton.Visibility = Visibility.Visible;
            textBox.Margin = new(27, 0, 102, 18);
        }

        if (isFirstLoaded)
        {
            isFirstLoaded = false;
            ConnectionString = await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new("ms-appx:///connection.txt")));
        }

        // 아예 이걸 firstloaded로 넣어버리고
        // date 기본값을 아주 먼 옛날로 해버리는 방법도 있음.
        await LoadChatsAsync();
        if (authored)
            textBox.IsEnabled = true;
        _ = ReloadChatsAsync().ConfigureAwait(false);
    }

    private async void textBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await SendMessageAsync(Info.User.ActivationLevel);
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
            return;

        if (btn.Name is "camoButtonA")
            await SendMessageAsync(ActivationLevel.Azure);
        else if (btn.Name is "camoButtonB")
            await SendMessageAsync(ActivationLevel.Bisque);
        else if (btn.Name is "sqlButton")
            await RunSQL(textBox.Text);
        else if (btn.Name is "delButton")
            await DeleteMessage(textBox.Text);
        else if (btn.Name is "notiButton")
            await SendNotificationAsync();
    }

    private async Task LoadChatsAsync()
    {
        string txt = textBox.PlaceholderText;
        textBox.PlaceholderText = "채팅 불러오는 중...";

        DataTable dt = new();
        using (SqlConnection sql = new(ConnectionString))
        {
            ChatMessageDac chat = new(sql);

            await sql.OpenAsync();
            await chat.SelectAll(dt);
        }

        StringBuilder sb = new();
        foreach (DataRow row in dt.Rows)
        {
            sb.AppendFormat(Datas.ChatFormat, DateTime.Parse(row["Time"].ToString()),
                Convert((byte)row["Sender"]), row["Message"]);
        }

        viewBox.Text = sb.ToString();
        ScrollViewBox();
        textBox.PlaceholderText = txt;
    }

    private async Task ReloadChatsAsync()
    {
        while (true)
        {
            while (isReloadPaused)
                await Task.Delay(400);

            while (!Connection.IsInternetAvailable)
            {
                await ShowMessageAsync("네트워크 연결을 확인하세요.", "Connection Error");
                await Task.Delay(1900);
            }

            if (isCancelRequested)
                return;

            Stopwatch sw = Stopwatch.StartNew();

            SqlConnection sql = new(ConnectionString);
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

            sw.Stop();
            await ShowMessageAsync($"{sw.ElapsedMilliseconds} ms", "GTT");
        }
    }


    private async Task SendMessageAsync(ActivationLevel userLevel)
    {
        try
        {
            if (textBox.IsNullOrWhiteSpace())
            {
                await ShowMessageAsync("보낼 메시지를 입력하세요.", title);
                return;
            }
            // 욕설 필터링
            if (BadWords.Any(s => textBox.Text.Contains(s)))
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

            using SqlConnection sql = new() { ConnectionString = ConnectionString };
            using ChatMessageDac chatmessage = new(sql, PrepareSend, DisposeSend);
            await sql.OpenAsync();
            await chatmessage.InsertAsync(Convert(userLevel), textBox.Text);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("채팅 전송에 실패했습니다.\n" + ex.ToString(), title);
            throw;
        }
    }

    private async Task SendNotificationAsync()
    {
        try
        {
            if (textBox.IsNullOrWhiteSpace())
            {
                await ShowMessageAsync("보낼 공지를 입력하세요.", title);
                return;
            }
            // 욕설 필터링

            using SqlConnection sql = new() { ConnectionString = ConnectionString };
            using ChatMessageDac chatmessage = new(sql, PrepareSend, DisposeSend);
            await sql.OpenAsync();
            await chatmessage.InsertAsync(8, "【공지】 " + textBox.Text);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("공지 등록에 실패했습니다.\n" + ex.ToString(), title);
            throw;
        }
    }

    private async Task RunSQL(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await ShowMessageAsync("Enter query.", title);
            return;
        }
        try
        {
            using SqlConnection sql = new(ConnectionString);
            using ChatMessageDac chat = new(sql, PrepareSend, DisposeSend);
            await sql.OpenAsync();
            await chat.RunSqlCommand(query);
        }
        catch (Exception e)
        {
            await ShowMessageAsync("Failed to run the SQL query. \n" + e.ToString(), title);
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

        using SqlConnection sql = new(ConnectionString);
        using ChatMessageDac chat = new(sql, PrepareSend, DisposeSend);
        await sql.OpenAsync();
        await chat.DeleteAsync(message);
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
        _ => throw new ArgumentException($"ChattingPage.Convert(ActivationLevel): unknown '{level}'.")
    };

    public static string Convert(byte level) => level switch
    {
        0 => "Karu",
        1 or 3 => "Azure",
        2 or 4 => "Bisque",
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
