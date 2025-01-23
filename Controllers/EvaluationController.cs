using ExamEvaluator.Interfaces;
using ExamEvaluator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExamEvaluation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;

        public EvaluationController(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        /// <summary>
        /// Get questions for the exam.
        /// </summary>
        /// <returns>A response containing questions</returns>
        [HttpGet("get-questions")]
        public async Task<IActionResult> GetQuestions()
        {
            var questions = _evaluationService.GetRandomQuestions();
            return Ok(questions);
        }

        /// <summary>
        /// send evaluation answers.
        /// </summary>
        /// <returns>A response containing the evaluation results.</returns>
        [HttpPost("sendAnswers")]
        public IActionResult SendAnswers(List<Answer>answers)
        {
            var evaluation = _evaluationService.EvaluateAnswersAsync(answers,CancellationToken.None);

            return Ok(evaluation);
        }
    }
}
