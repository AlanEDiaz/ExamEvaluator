using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ExamEvaluator.Interfaces;
using ExamEvaluator.Models;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ExamEvaluation.Api.Services
{
    public class ExamEvaluationService : IEvaluationService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ExamEvaluationService> _logger;

        public ExamEvaluationService([FromKeyedServices("ExamEvaluatorKernel")] Kernel kernel, ILogger<ExamEvaluationService> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }
        public static List<Questions> LoadQuestions(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Questions>>(json);
        }

        public List<Questions> GetRandomQuestions()
        {
            int devCount = 2;
            int tfCount = 1;
            int mcCount = 1;
            var questions = LoadQuestions("C:/Users/alan_/source/repos/ExamEvaluator/Questions/Questions.json");
            var random = new Random();

            var developmentQuestions = questions.Where(q => q.Type == "Development").ToList();
            var trueFalseQuestions = questions.Where(q => q.Type == "True/False").ToList();
            var multipleChoiceQuestions = questions.Where(q => q.Type == "Multiple Choice").ToList();

            var selectedQuestions = new List<Questions>();

            selectedQuestions.AddRange(developmentQuestions.OrderBy(x => random.Next()).Take(devCount));

            selectedQuestions.AddRange(trueFalseQuestions.OrderBy(x => random.Next()).Take(tfCount));

            selectedQuestions.AddRange(multipleChoiceQuestions.OrderBy(x => random.Next()).Take(mcCount));

            return selectedQuestions;
        }
        public async Task<Evaluation> EvaluateAnswersAsync(List<Answer> answers, CancellationToken cancellationToken)
        {
            try
            {
                var questions = LoadQuestions("C:/Users/alan_/source/repos/ExamEvaluator/Questions/Questions.json");
                var chunks = LoadChunks();
                _logger.LogInformation($"Evaluating answers: {answers}");

                string promptTemplate = @"
                        You are an AI assistant tasked with evaluating the answers like this:
                        {{ ExamPlugins.GetExampleDevelopmentQuestion }} ,{{ ExamPlugins.GetExampleMultipleChoiseQuestion }},{{ ExamPlugins.GetExampleTrueFalseQuestion }}
                        Provide a detailed evaluation in markdown, suitable for review.
                        I will provide each answer with a chunk, and you will return the feedback given that data";


                foreach (var answer in answers)
                {

                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    var chunk = GetChunkById(answer.QuestionId);
                    if (question != null)
                    {
                        string response = ValidateAnswer(question, answer, chunk);
                        promptTemplate += response;
                    }
                    else
                    {
                        Console.WriteLine($"Question with ID {answer.QuestionId} not found.");
                    }
                }

                _kernel.ImportPluginFromPromptDirectory("C:/Users/alan_/source/repos/ExamEvaluator/Prompts/ExamPlugins");
                var evaluateFunction = _kernel.CreateFunctionFromPrompt(promptTemplate);

                var evaluation = await evaluateFunction.InvokeAsync(_kernel);

                return new Evaluation
                {
                    Report = evaluation.ToString(),
                    ChatHistory = "",
                    EvaluationScore = 90.0, // Example score
                    Feedback = "Well done!",
                    CorrectAnswers = new string[] { "CorrectAnswer1", "CorrectAnswer2" } // Example correct answers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating answers");
                return new Evaluation();
            }
        }
        static List<Chunk> LoadChunks()
        {
            var json = File.ReadAllText("C:/Users/alan_/source/repos/ExamEvaluator/Chunks/Chunks.json");
            return JsonConvert.DeserializeObject<List<Chunk>>(json);
        }

        static Chunk GetChunkById(int id)
        {
            var chunks = LoadChunks();
            return chunks.FirstOrDefault(chunk => chunk.Id == id);
        }
        public static string ValidateAnswer(Questions question, Answer answer, Chunk chunk)
        {
            string validationMessage;

            if (question.Type == "Multiple Choice")
            {
                validationMessage = $"The question was: {question.Text}. The response was: {answer.SelectedOption} and was obtained from the {chunk.Text}.{Environment.NewLine}";
            }
            else if (question.Type == "Development" || question.Type == "True/False")
            {
                validationMessage = $"The question was: {question.Text}. The response was: {answer.TextAnswer} and was obtained from the {chunk.Text}.{Environment.NewLine}";
            }
            else
            {
                validationMessage = "";
            }

            return validationMessage;
        }
    }
}
