namespace Common.Models;

public class AskRequest
{
    public string Question { get; set; } = string.Empty;

  
    public List<ChatTurn> History { get; set; } = new();
}
