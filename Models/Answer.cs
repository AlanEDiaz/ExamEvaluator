using Microsoft.AspNetCore.Components.Forms;
using System;

namespace ExamEvaluator.Models
{
    public class Answer
    {
        public int QuestionId { get; set; } 
        public string? SelectedOption { get; set; }
        public string? TextAnswer { get; set; }
        public double? Score { get; set; }
    }
}
