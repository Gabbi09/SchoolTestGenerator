using System;
using System.Collections.Generic;
using System.Linq;

namespace TestGenerator
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<string> Answers { get; set; }
        public int CorrectAnswer { get; set; } 

        public Question(int id, string text, List<string> answers, int correctAnswer)
        {
            Id = id;
            Text = text;
            Answers = answers;
            CorrectAnswer = correctAnswer;
        }

        public Question GetShuffledAnswers(Random random)
        {
            string correctAnswerText = Answers[CorrectAnswer - 1];

            List<string> shuffledAnswers = Answers
                .OrderBy(x => random.Next())
                .ToList();

            int newCorrectIndex = shuffledAnswers.IndexOf(correctAnswerText) + 1;

            return new Question(Id, Text, shuffledAnswers, newCorrectIndex);
        }
    }
}
