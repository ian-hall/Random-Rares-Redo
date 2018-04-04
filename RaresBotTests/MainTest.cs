using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

namespace RaresBot.Tests
{
    public class MainTest
    {
        static string localPath = @"rares.json";
        static List<RareGood> allRares = RareGood.LoadRares(localPath, false);

        [Fact]
        public void TestGetUniqueStrings1()
        {
            List<string> testList = new List<string>() { "This", "Is", "A", "List", "With", "Unique", "Elements" };
            var expectedCount = testList.Count;
            var calculated = Program.GetUniqueStrings(testList).Count;
            Assert.Equal(expectedCount, calculated);
        }

        [Fact]
        public void TestGetUniqueStrings2()
        {
            List<string> testList = new List<string>() { "This", "Is", "A", "List", "With", "Unique", "Elements" };
            var expectedCount = testList.Count;
            //DupeList is the list except twice
            var dupeList = new List<string>();
            for( int i = 0; i < testList.Count * 2; i++ )
            {
                dupeList.Add(testList[i % testList.Count]);
            }
            var calculated = Program.GetUniqueStrings(dupeList).Count;
            Assert.Equal(expectedCount, calculated);
        }

        [Fact]
        public void TestBreakdown()
        {
            var rng = new Random();
            var longStrings = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var max = rng.Next(1, 10);
                var sb = new System.Text.StringBuilder();
                for ( int j = 0; j < max-1; j++)
                {
                    sb.AppendFormat("{0} ", rng.Next(1000));
                }
                sb.AppendFormat("{0}", max-1);
                longStrings.Add(sb.ToString());
            }
            var brokenDown = Program.BreakDown(longStrings);
            var longestLen = longStrings.Max(s => s.Split().Count());
            Assert.Equal(longestLen, brokenDown.Count);
        }

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(10000)]
        //~~ 2500 repeats every 10000
        public void TestPortGenerator(int n)
        {
            var allPorts = allRares.Select(item => item.Port);
            var portsBreakDown = Program.BreakDown(allPorts);

            var generatedPorts = new List<string>();
            for( int i = 0; i < n; i++ )
            {
                generatedPorts.Add(Program.GetPortName(portsBreakDown));
            }
            var uniqueNames = Program.GetUniqueStrings(generatedPorts);

            //Assert.Equal(generatedPorts, uniqueNames);
            Assert.Equal(generatedPorts.Count, uniqueNames.Count);
        }

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(10000)]
        //~~~650 repeats every 10000
        public void TestItemGenerator(int n)
        {
            var allItems = allRares.Select(item => item.Item);
            var itemsBreakdown = Program.BreakDown(allItems);

            var generatedItems = new List<string>();
            for (int i = 0; i < n; i++)
            {
                generatedItems.Add(Program.GetPortName(itemsBreakdown));
            }
            var uniqueNames = Program.GetUniqueStrings(generatedItems);

            //Assert.Equal(generatedPorts, uniqueNames);
            Assert.Equal(generatedItems.Count, uniqueNames.Count);
        }
    }
}
