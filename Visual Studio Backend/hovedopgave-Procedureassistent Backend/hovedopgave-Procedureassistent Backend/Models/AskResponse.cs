namespace hovedopgave_Procedureassistent_Backend.Models;

public class AskResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<Source> Sources { get; set; } = new();
}
