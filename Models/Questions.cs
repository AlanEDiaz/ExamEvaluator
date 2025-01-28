using ExamEvaluator.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Questions
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }
    [JsonPropertyName("Text")]
    public string Text { get; set; }
    [JsonPropertyName("Type")]
    [JsonConverter(typeof(QuestionTypeConverter))]
    public QuestionTypes Type { get; set; }
    [JsonPropertyName("Options")]
    public List<string> Options { get; set; }
    [JsonPropertyName("NeedJustification")]
    public bool NeedJustification { get; set; }

}