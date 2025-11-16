using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace GenAI.HelpDesk.Client.Pages
{
        public class HelpDeskModel : PageModel
        {
            private readonly HttpClient _httpClient;

            [BindProperty]
            public string UserQuestion { get; set; } = string.Empty;

            public List<ChatMessage> ChatHistory { get; set; } = new();

            // Static variable to keep chat history in memory
            private static readonly List<ChatMessage> _staticChatHistory = new();

            public HelpDeskModel(IHttpClientFactory httpClientFactory)
            {
                _httpClient = httpClientFactory.CreateClient();
            }

            public void OnGet()
            {
                ChatHistory = new List<ChatMessage>(_staticChatHistory);
            }

            public async Task<IActionResult> OnPostAsync()
            {
                if (!string.IsNullOrWhiteSpace(UserQuestion))
                {
                    _staticChatHistory.Add(new ChatMessage("user", UserQuestion));

                    try
                    {
                        // Prepare request JSON for FastAPI
                        var requestBody = new { QuestionText = UserQuestion };

                        // Send POST to FastAPI backend
                        var response = await _httpClient.PostAsJsonAsync("https://localhost:7089/api/Ask/ask", requestBody);

                        if (response.IsSuccessStatusCode)
                        {
                            var apiResponse = await response.Content.ReadFromJsonAsync<AskQuestionResponse>();

                            if (apiResponse != null)
                            {
                                _staticChatHistory.Add(new ChatMessage("bot", apiResponse.AnswerText, apiResponse.SourceList));
                            }
                        }
                        else
                        {
                            _staticChatHistory.Add(new ChatMessage("bot", "⚠️ API error: Unable to get a response."));
                        }
                    }
                    catch
                    {
                        _staticChatHistory.Add(new ChatMessage("bot", "⚠️ Error: Could not connect to FastAPI server."));
                    }
                }

                ChatHistory = new List<ChatMessage>(_staticChatHistory);
                return Page();
            }

            public IActionResult OnPostClearChat()
            {
                _staticChatHistory.Clear();
                return RedirectToPage();
            }

            // --- Models ---
            public class ChatMessage
            {
                public ChatMessage() { }
                public ChatMessage(string sender, string message, List<string>? sources = null)
                {
                    Sender = sender;
                    Message = message;
                    Sources = sources ?? new List<string>();
                }

                public string Sender { get; set; } = string.Empty;
                public string Message { get; set; } = string.Empty;
                public List<string> Sources { get; set; } = new();
            }

            public class AskQuestionResponse
            {
          //  [JsonPropertyName("answer_text")]
            public string AnswerText { get; set; } = string.Empty;

          //  [JsonPropertyName("source_list")]
            public List<string> SourceList { get; set; } = new();
        }
        }
    }


