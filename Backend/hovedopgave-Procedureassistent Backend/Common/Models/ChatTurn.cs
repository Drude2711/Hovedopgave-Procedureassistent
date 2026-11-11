namespace Common.Models;

/// <summary>
/// Én tidligere besked i samtalen. Role er "user" eller "assistant".
/// </summary>
public class ChatTurn
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = string.Empty;
}
