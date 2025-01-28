using ExamEvaluator.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class QuestionTypeConverter : JsonConverter<QuestionTypes>
{
    public override QuestionTypes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = reader.GetString();
        return type switch
        {
            "Multiple Choice" => QuestionTypes.MultipleChoice,
            "True/False" => QuestionTypes.TrueFalse,
            _ => QuestionTypes.FillInTheBlank
        };
    }

    public override void Write(Utf8JsonWriter writer, QuestionTypes value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            QuestionTypes.MultipleChoice => "Multiple Choice",
            QuestionTypes.TrueFalse => "True/False",
            _ => "FillInTheBlank"
        };
        writer.WriteStringValue(stringValue);
    }
}
