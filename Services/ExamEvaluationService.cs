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
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Globalization;
using System.Net.Http;

namespace ExamEvaluation.Api.Services
{
    public class ExamEvaluationService : IEvaluationService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ExamEvaluationService> _logger;
        private readonly HttpClient _httpClient;

        public ExamEvaluationService([FromKeyedServices("ExamEvaluatorKernel")] Kernel kernel, ILogger<ExamEvaluationService> logger,HttpClient httpClient)
        {
            _kernel = kernel;
            _logger = logger;
            _httpClient = httpClient;
        }
        public static List<Questions> LoadQuestions()
        {
            var filePath = "C:/Users/alan_/source/repos/ExamEvaluator/Questions/Questions.json";
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

        public List<Questions> GetRandomQuestions()
        {
            int devCount = 2;
            int tfCount = 1;
            int mcCount = 1;
            var questions = LoadQuestions();
            var random = new Random();

            var developmentQuestions = questions.Where(q => q.Type == QuestionTypes.FillInTheBlank).ToList();
            var trueFalseQuestions = questions.Where(q => q.Type == QuestionTypes.TrueFalse).ToList();
            var multipleChoiceQuestions = questions.Where(q => q.Type == QuestionTypes.FillInTheBlank).ToList();

            var selectedQuestions = new List<Questions>();

            selectedQuestions.AddRange(developmentQuestions.OrderBy(x => random.Next()).Take(devCount));

            selectedQuestions.AddRange(trueFalseQuestions.OrderBy(x => random.Next()).Take(tfCount));

            selectedQuestions.AddRange(multipleChoiceQuestions.OrderBy(x => random.Next()).Take(mcCount));

            return selectedQuestions;
        }
        public async Task<string> EvaluateAnswersAsyncv2(List<Answer> answers, CancellationToken cancellationToken)
        {
            try
            {
                var questions = LoadQuestions();
                var chunks = LoadChunks();
                _logger.LogInformation($"Evaluating answers: {answers}");


                var chatHistory = new ChatHistory();
                chatHistory.AddMessage(AuthorRole.System, "You are an AI assistant tasked with evaluating the answers like this:" +
                    " Provide a detailed evaluation in markdown, suitable for review ,and a score for each answer, and finally a final score I will " +
                    "provide each answer with a chunk, and you will return the feedback given that data for each answer, and a general feedback and score from 1 to 10");
                chatHistory.AddSystemMessage("verify if the question requiere justification if not validate it with the provided answer");
                chatHistory.AddSystemMessage("the students answers are:");
                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question.Type == QuestionTypes.MultipleChoice)
                        chatHistory.AddUserMessage($"the answer is {answer.SelectedOption} for question{answer.QuestionId}");

                    else
                        chatHistory.AddUserMessage($"the answer is {answer.TextAnswer} for question{answer.QuestionId}");

                }


                _kernel.CreatePluginFromType<AnswerAppendPlugin>("AnswerAppend");
                OpenAIPromptExecutionSettings settings = new()
                { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

                var evaluation = await _kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(chatHistory, settings, _kernel);
                return evaluation.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating answers");
                return "Error evaluating answers";
            }
        }
        
        public async Task<Evaluation> EvaluateAnswersAsync(List<Answer> answers, CancellationToken cancellationToken)
        {
            try
            {
                var questions = LoadQuestions();
                var chunks = LoadChunks();
                _logger.LogInformation($"Evaluating answers: {answers}");

                string promptTemplate = @"
                        You are an AI assistant tasked with evaluating the answers like this:
                       Provide a detailed evaluation in markdown, suitable for review ,and a score for each answer, and finally a final score
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

                var evaluateFunction = _kernel.CreateFunctionFromPrompt(promptTemplate);

                var evaluation = await evaluateFunction.InvokeAsync(_kernel);

                return new Evaluation
                {
                    EvaluationScore = 90.0,
                    Feedback = evaluation.ToString()
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

            if (question.Type == QuestionTypes.MultipleChoice)
            {
                validationMessage = $" The question was: {question.Text}. The response was: {answer.SelectedOption} and was obtained from the {chunk.Text}.{Environment.NewLine}";
            }
            else if (question.Type == QuestionTypes.FillInTheBlank || question.Type == QuestionTypes.TrueFalse)
            {
                validationMessage = $" The question was: {question.Text}. The response was: {answer.TextAnswer} and was obtained from the {chunk.Text}.{Environment.NewLine}";
            }
            else
            {
                validationMessage = "";
            }

            return validationMessage;
        }
    }
}
