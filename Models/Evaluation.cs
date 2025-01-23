namespace ExamEvaluator.Models
{
    public class Evaluation
    {
        public string Report { get; set; }
        public string ChatHistory { get; set; } 
        public double EvaluationScore { get; set; }
        public string Feedback { get; set; } 
        public string[] CorrectAnswers { get; set; }  
    }
}
