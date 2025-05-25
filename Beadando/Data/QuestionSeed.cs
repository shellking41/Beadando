using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beadando.Models;
using Microsoft.EntityFrameworkCore;

namespace Beadando.Data
{
    public static class QuestionSeed
    {
        public static async Task SeedQuestionsAsync(ApplicationDbContext context)
        {
            if (await context.Questions.AnyAsync())
                return;

            var questions = new List<Question>();

            var question1 = new Question
            {
                Text = "Mit jelent ez a tábla?",
                CreatedAt = DateTime.UtcNow
            };

            question1.Answers = new List<Answer>
            {
                new Answer { Text = "Elsőbbségadás kötelező", IsCorrect = true, Question = question1 },
                new Answer { Text = "Stop", IsCorrect = false, Question = question1 },
                new Answer { Text = "Főútvonal", IsCorrect = false, Question = question1 }
            };

            questions.Add(question1);

            var question2 = new Question
            {
                Text = "Milyen sebességgel lehet közlekedni lakott területen belül?",
                CreatedAt = DateTime.UtcNow
            };

            question2.Answers = new List<Answer>
            {
                new Answer { Text = "50 km/h", IsCorrect = true, Question = question2 },
                new Answer { Text = "60 km/h", IsCorrect = false, Question = question2 },
                new Answer { Text = "70 km/h", IsCorrect = false, Question = question2 }
            };

            questions.Add(question2);

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();

            // Add answers after questions are saved
            questions[0].Answers.AddRange(new[]
            {
                new Answer { Text = "Állj! Elsőbbségadás kötelező", IsCorrect = true, Question = questions[0] },
                new Answer { Text = "Elsőbbségadás kötelező", IsCorrect = false, Question = questions[0] },
                new Answer { Text = "Mindkét irányból behajtani tilos", IsCorrect = false, Question = questions[0] }
            });

            questions[1].Answers.AddRange(new[]
            {
                new Answer { Text = "50 km/h", IsCorrect = true, Question = questions[1] },
                new Answer { Text = "60 km/h", IsCorrect = false, Question = questions[1] },
                new Answer { Text = "70 km/h", IsCorrect = false, Question = questions[1] },
                new Answer { Text = "90 km/h", IsCorrect = false, Question = questions[1] }
            });

            questions[2].Answers.AddRange(new[]
            {
                new Answer { Text = "Akkora távolságra, hogy az elöl haladó mögé be lehessen sorolni", IsCorrect = false, Question = questions[2] },
                new Answer { Text = "Legalább a járművel másodpercenként megtett út felének megfelelő távolságra", IsCorrect = false, Question = questions[2] },
                new Answer { Text = "Legalább akkora távolságra, mint a járművel másodpercenként megtett út", IsCorrect = true, Question = questions[2] }
            });

            questions[3].Answers.AddRange(new[]
            {
                new Answer { Text = "Meg kell állni a stoptábla előtt", IsCorrect = false, Question = questions[3] },
                new Answer { Text = "Ha biztonságosan meg lehet állni, akkor meg kell állni a fényjelző készülék előtt", IsCorrect = true, Question = questions[3] },
                new Answer { Text = "Fokozott óvatossággal át kell haladni a kereszteződésen", IsCorrect = false, Question = questions[3] }
            });

            questions[4].Answers.AddRange(new[]
            {
                new Answer { Text = "Csak a kijelölt gyalogos-átkelőhelyen", IsCorrect = false, Question = questions[4] },
                new Answer { Text = "A kijelölt gyalogos-átkelőhelyen és a kanyarodás során", IsCorrect = true, Question = questions[4] },
                new Answer { Text = "Csak akkor, ha a gyalogos már megkezdte az átkelést", IsCorrect = false, Question = questions[4] }
            });

            questions[5].Answers.AddRange(new[]
            {
                new Answer { Text = "Csak éjszaka", IsCorrect = false, Question = questions[5] },
                new Answer { Text = "Csak lakott területen kívül", IsCorrect = false, Question = questions[5] },
                new Answer { Text = "Korlátozott látási viszonyok között", IsCorrect = true, Question = questions[5] }
            });

            questions[6].Answers.AddRange(new[]
            {
                new Answer { Text = "A járművet azonnal meg kell állítani az útpadkán", IsCorrect = true, Question = questions[6] },
                new Answer { Text = "Lassan tovább lehet haladni a legközelebbi szervizig", IsCorrect = false, Question = questions[6] },
                new Answer { Text = "Be kell kapcsolni a vészvillogót és folytatni az utat", IsCorrect = false, Question = questions[6] }
            });

            questions[7].Answers.AddRange(new[]
            {
                new Answer { Text = "90 km/h", IsCorrect = false, Question = questions[7] },
                new Answer { Text = "110 km/h", IsCorrect = false, Question = questions[7] },
                new Answer { Text = "130 km/h", IsCorrect = true, Question = questions[7] },
                new Answer { Text = "140 km/h", IsCorrect = false, Question = questions[7] }
            });

            questions[8].Answers.AddRange(new[]
            {
                new Answer { Text = "Csak lakott területen kívül", IsCorrect = false, Question = questions[8] },
                new Answer { Text = "Csak nagy sebességnél", IsCorrect = false, Question = questions[8] },
                new Answer { Text = "Minden esetben, amikor a jármű mozgásban van", IsCorrect = true, Question = questions[8] }
            });

            questions[9].Answers.AddRange(new[]
            {
                new Answer { Text = "Az útkereszteződésbe behajtani tilos", IsCorrect = true, Question = questions[9] },
                new Answer { Text = "A keresztirányból érkezőknek elsőbbséget kell adni", IsCorrect = false, Question = questions[9] },
                new Answer { Text = "Fokozott óvatossággal tovább lehet haladni", IsCorrect = false, Question = questions[9] }
            });

            await context.SaveChangesAsync();
        }
    }
} 