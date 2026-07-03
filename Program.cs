using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dataFolder = "Data";
            string questionsFile = Path.Combine(dataFolder, "questions.txt");

            // Проверка дали папката Data съществува
            if (!Directory.Exists(dataFolder))
            {
                Console.WriteLine("Грешка: Папката Data не е намерена.");
                Console.WriteLine("Създай папка Data и сложи вътре файла questions.txt.");
                return;
            }

            // Проверка дали файлът с въпроси съществува
            if (!File.Exists(questionsFile))
            {
                Console.WriteLine("Грешка: Файлът questions.txt не е намерен в папка Data.");
                Console.WriteLine("Постави файла в Data/questions.txt и стартирай отново.");
                return;
            }

            // Проверка дали вече има създадени test файлове
            string[] existingTests = Directory.GetFiles(dataFolder, "test*.txt");

            if (existingTests.Length > 0)
            {
                Console.WriteLine("Открити са вече създадени тестови файлове в папка Data.");
                Console.WriteLine("Не могат да бъдат генерирани нови файлове.");
                Console.WriteLine("Изтрий или премести старите test*.txt файлове и стартирай отново.");
                return;
            }

            // Зареждане на въпросите
            List<Question> allQuestions = LoadQuestionsFromFile(questionsFile);

            if (allQuestions.Count == 0)
            {
                Console.WriteLine("Няма валидни въпроси във файла questions.txt.");
                return;
            }

            Console.WriteLine($"Успешно заредени въпроси: {allQuestions.Count}");

            // Въвеждане на брой тестове
            int testCount = ReadPositiveInt("Въведи брой тестове за генериране: ");

            // Въвеждане на брой въпроси във всеки тест
            int questionsPerTest;
            while (true)
            {
                questionsPerTest = ReadPositiveInt("Въведи брой въпроси във всеки тест: ");

                if (questionsPerTest > allQuestions.Count)
                {
                    Console.WriteLine($"Грешка: Няма достатъчно въпроси. Максималният възможен брой е {allQuestions.Count}.");
                }
                else
                {
                    break;
                }
            }

            // Генериране на тестовете
            GenerateTests(allQuestions, testCount, questionsPerTest, dataFolder);

            Console.WriteLine();
            Console.WriteLine("Генерирането приключи успешно!");
            Console.WriteLine("Тестовете са записани в папка Data.");
            Console.WriteLine("Натисни произволен клавиш за изход...");
            Console.ReadKey();
        }

        static List<Question> LoadQuestionsFromFile(string filePath)
        {
            List<Question> questions = new List<Question>();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');

                // Формат:
                // ID|Question|Answer1|Answer2|Answer3|Answer4|CorrectAnswer
                if (parts.Length != 7)
                {
                    Console.WriteLine($"Пропуснат невалиден ред: {line}");
                    continue;
                }

                try
                {
                    int id = int.Parse(parts[0]);
                    string questionText = parts[1];

                    List<string> answers = new List<string>
                    {
                        parts[2],
                        parts[3],
                        parts[4],
                        parts[5]
                    };

                    int correctAnswer = int.Parse(parts[6]);

                    if (correctAnswer < 1 || correctAnswer > 4)
                    {
                        Console.WriteLine($"Невалиден CorrectAnswer при ред: {line}");
                        continue;
                    }

                    questions.Add(new Question(id, questionText, answers, correctAnswer));
                }
                catch
                {
                    Console.WriteLine($"Грешка при обработка на ред: {line}");
                }
            }

            return questions;
        }

        static int ReadPositiveInt(string message)
        {
            int value;

            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine();

                if (int.TryParse(input, out value) && value > 0)
                    return value;

                Console.WriteLine("Моля, въведи валидно положително цяло число.");
            }
        }

        static void GenerateTests(List<Question> allQuestions, int testCount, int questionsPerTest, string dataFolder)
        {
            Random random = new Random();

            for (int i = 1; i <= testCount; i++)
            {
                // Избира случайни въпроси без повторение в рамките на текущия тест
                List<Question> selectedQuestions = allQuestions
                    .OrderBy(q => random.Next())
                    .Take(questionsPerTest)
                    .ToList();

                string fileName = Path.Combine(dataFolder, $"test{i}.txt");

                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.WriteLine($"ТЕСТ №{i}");
                    writer.WriteLine(new string('=', 50));
                    writer.WriteLine();

                    for (int qIndex = 0; qIndex < selectedQuestions.Count; qIndex++)
                    {
                        // Бонус: разбъркване на отговорите
                        Question shuffledQuestion = selectedQuestions[qIndex].GetShuffledAnswers(random);

                        writer.WriteLine($"{qIndex + 1}. {shuffledQuestion.Text}");

                        for (int ansIndex = 0; ansIndex < shuffledQuestion.Answers.Count; ansIndex++)
                        {
                            writer.WriteLine($"   {ansIndex + 1}) {shuffledQuestion.Answers[ansIndex]}");
                        }

                        writer.WriteLine();
                    }
                }

                Console.WriteLine($"Създаден файл: test{i}.txt");
            }
        }
    }
}