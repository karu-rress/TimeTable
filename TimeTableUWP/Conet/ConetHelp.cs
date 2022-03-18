﻿#nullable enable

namespace TimeTableUWP.Conet;

public record struct Student(int id, string name)
{
    /// <returns>a string like "3116 나선우"</returns>
    public override string ToString() => $"{id} {name}";
}

public class ConetHelp
{
    public ConetHelp(DateTime upload, string uploader, string title, string? body, string? price)
        : this(upload, new Student(Convert.ToInt32(uploader[0..4]), uploader[5..].TrimEnd()),
              title, body,
              string.IsNullOrEmpty(price) ? null : new(Convert.ToUInt32(price)))
    {
    }

    public ConetHelp(DateTime upload, Student uploader, string title, string? body = null, Egg? price = null)
    {
        UploadDate = upload;
        Title = title;
        Body = body;
        Uploader = uploader;
        Price = price;
    }

    public DateTime UploadDate { get; }
    public Egg? Price { get; set; }
    public string Title { get; set; }
    public string? Body { get; set; }
    public Student Uploader { get; set; }

    // 관계 연산자는 UploadDate 기준으로.
}