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
    public Choice[] Choices { get; set; }
}

public class Choice
{
    public Message Message { get; set; }
}

public class Message
{
    public string Content { get; set; }
    public string Role { get; set; }
}