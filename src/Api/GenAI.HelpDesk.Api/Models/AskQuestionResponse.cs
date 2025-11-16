namespace GenAI.HelpDesk.Api.Models;

public class AskQuestionResponse
{
    public string AnswerText { get; set; } = string.Empty;
    public List<string> SourceList { get; set; } = new();
}
