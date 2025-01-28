using ExamEvaluator.Models;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

public class AnswerAppendPlugin
{
    private List<string> _evaluatedAnswers = new List<string>();

    private List<Questions> _questions = LoadQuestions("C:/Users/alan_/source/repos/ExamEvaluator/Questions/Questions.json");
    private List<Chunk> _chunks = LoadChunks();

    public static List<Questions> LoadQuestions(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new QuestionTypeConverter() }
        };

        // Deserialize JSON
        var questionsList = System.Text.Json.JsonSerializer.Deserialize<List<Questions>>(json, options);
        return questionsList;
    }

    static List<Chunk> LoadChunks()
    {
        var json = File.ReadAllText("C:/Users/alan_/source/repos/ExamEvaluator/Chunks/Chunks.json");
        return JsonConvert.DeserializeObject<List<Chunk>>(json);
    }

    [KernelFunction("get_questions")]
    [Description("get the list of questions made by the professor")]
    public List<Questions> QuestionsMade([Description("Answers from the students")] List<Answer> answers)
    {
        var questions=new List<Questions>();
        foreach (var answer in answers)
        {
            var question = _questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null)
            {
                questions.Add(question);
            }
        }

        return questions;
    }

    [KernelFunction("get_chunks")]
    [Description("get the list of chunks for the questions made")]
    public List<Chunk> AppendAnswers([Description("questions of the answers")] List<Questions> questions)
    {
        var chunks = new List<Chunk>();
        foreach (var question in questions)
        {
            var chunk = _chunks.FirstOrDefault(q => q.Id == question.Id);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    [KernelFunction("justification_needed_by_question")]
    [Description("Verify if justification is needed for the question")]
    public bool AppendAnswers(Questions question)
    {
        return question.NeedJustification;
    }

    [KernelFunction("append_answers")]
    [Description("Appends the list of answers with his corresponding chunk and question")]
    public void AppendAnswers([Description("Answers from the students")]List <Answer> answers, [Description("Chunks of the correct answers")] List<Chunk> chunks, [Description("questions of the answers")] List<Questions> questions)
    {
        foreach (var answer in answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            var chunk = chunks.FirstOrDefault(c => c.Id == answer.QuestionId);

            if (question != null && chunk != null)
            {
                string evaluationMessage = $"Question: {question.Text}, Answer: {answer.TextAnswer}, Chunk: {chunk.Text}";
                _evaluatedAnswers.Add(evaluationMessage);
            }
        }
    }
    [KernelFunction("get_answers")]
    [Description("get the list of answers with his corresponding chunk and question")]
    public List<string> GetEvaluatedAnswers()
    {
        return _evaluatedAnswers;
    }
}
