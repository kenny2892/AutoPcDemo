using AutoPcDemoWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPcDemoWebsite.Data
{
    public class DbInitializer
    {
        public static void Initialize(TestingDataContext context)
        {
            if(context.TestDatas.Count() > 0)
            {
                return;
            }

            // Create random data
            var listOfCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var categoryOnes = new string[] { "Cat 1", "Cat 2", "Cat 3" };
            var categoryTwos = new string[] { "Inner Cat 1", "Inner Cat 2", "Inner Cat 3" };
            var minDate = new DateTime(2015, 1, 1);
            var dateRange = (DateTime.Today - minDate).Days;
            var rng = new Random();

            var data = new List<TestData>();
            for(int i = 0; i < 100; i++)
            {
                TestData toAdd = new TestData()
                {
                    ID = new String(Enumerable.Repeat(listOfCharacters, 20).Select(characters => characters[rng.Next(characters.Length)]).ToArray()),
                    CategoryOne = categoryOnes[rng.Next(categoryOnes.Length)],
                    CategoryTwo = categoryTwos[rng.Next(categoryTwos.Length)],
                    Amount = rng.NextDouble() * 200,
                    Enabled = rng.Next(2) == 1,
                    Date = minDate.AddDays(rng.Next(dateRange))
                };

                data.Add(toAdd);
            }

            context.TestDatas.AddRange(data);
            context.SaveChanges();
        }
    }
}
