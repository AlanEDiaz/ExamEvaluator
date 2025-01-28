using System.Collections.Generic;

namespace ExamEvaluator.Models
{
    public class Evaluation
    {
        public List<Answer> Answers { get; set; }
        public List<Questions>  Questions { get; set; }
        public double EvaluationScore { get; set; }
        public string Feedback { get; set; } 
    }
}
