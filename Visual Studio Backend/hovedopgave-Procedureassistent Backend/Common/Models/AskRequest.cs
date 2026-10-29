namespace Common.Models;

public class AskRequest
{
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// De foregående beskeder i samtalen, så modellen kan forstå opfølgende spørgsmål.
    /// Sendes med af klienten (webpart eller testkonsol). Backend gemmer intet.
    /// </summary>
    public List<ChatTurn> History { get; set; } = new();
}
