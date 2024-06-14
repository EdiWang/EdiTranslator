namespace Edi.Translator.Models;

public class TranslationResponse
{
    public List<Translation> Translations { get; set; }
}

public class Translation
{
    public string Text { get; set; }
    public string To { get; set; }
}

public class AOAIResponse
{
    public Choice[] choices { get; set; }
}

public class Choice
{
    public Message message { get; set; }
}

public class Message
{
    public string content { get; set; }
    public string role { get; set; }
}