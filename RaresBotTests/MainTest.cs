using System;
using System.Collections.Generic;
using Xunit;

namespace RaresBot.Tests
{
    public class MainTest
    {
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
    }
}
