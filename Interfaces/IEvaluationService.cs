using ExamEvaluator.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExamEvaluator.Interfaces
{
    public interface IEvaluationService
    {
        Task<Evaluation> EvaluateAnswersAsync(List<Answer> answers, CancellationToken cancellationToken);
        Task<string> EvaluateAnswersAsyncv2(List<Answer> answers, CancellationToken cancellationToken);
        List<Questions> GetRandomQuestions();

    }
}
