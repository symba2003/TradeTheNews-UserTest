using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionsTest
{
    // test SortedSet and SortedDictionary
    class Program2
    {
        public static void Run() 
        {
            //INFO: коллекции практически идентичны по скорости при получении сортированного множества (TestTwo)
            // SortedDictionary быстрее для look up operation (TestOne)
            var sortedDictionary = new SortedDictionary<int, User>();
            var sortedSet = new SortedSet<User>();

            var testSizesForTestOne = new int[] { 100, 1000, 10000, 50000, 100000 };
            var testSizesForTestTwo = new int[] { 10, 50, 100, 500, 1000, 5000 };

            RunTest(sortedDictionary, sortedSet, testSizesForTestOne, "TestOne",
                (set, testSize) => RunTestOneSortedDict(set, testSize), (set, testSize) => RunTestOneSortedSet(set, testSize));

            RunTest(sortedDictionary, sortedSet, testSizesForTestTwo, "TestTwo",
                (set, testSize) => RunTestTwoSortedDict(set, testSize), (set, testSize) => RunTestTwoSortedSet(set, testSize));
        }

        private static void RunTest(SortedDictionary<int, User> sortedDict, SortedSet<User> sortedSet, IEnumerable<int> testSizes,
            string label,
            Func<SortedDictionary<int, User>, int, TimeSpan> testFuncSoretdDict, Func<SortedSet<User>, int, TimeSpan> testFuncSortedSet)
        {

            foreach (int test in testSizes)
            {
                var result = testFuncSoretdDict(sortedDict, test);
                Console.WriteLine("{0} with Sorted Dictionary ({1}): {2}", label, test, result);

                result = testFuncSortedSet(sortedSet, test);
                Console.WriteLine("{0} with Sorted Set ({1}): {2}", label, test, result);

                sortedDict.Clear();
                sortedSet.Clear();
                Console.WriteLine();
            }
        }


        /// <summary>
        /// adds several items to a collection, then searches for items in that collection. The test is repeated 50 times to get a more consistent result
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="numItemsToTest"></param>
        /// <returns></returns>
        static TimeSpan RunTestOneSortedSet(SortedSet<User> collection, int numItemsToTest)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // perform the test 50 times: 
            for (int counter = 0; counter < 50; counter++)
            {
                collection.Clear();
                // add some random items 
                Random rnd = new Random();
                var sequence = from n in Enumerable.Range(1, numItemsToTest)
                               select rnd.Next() * n;

                foreach (var item in sequence)
                   collection.Add(new User(item, "family", "name"));


                // search for 1000 random items 
                var sequence2 = from n in Enumerable.Range(1, 1000)
                                select rnd.Next() * n;

                bool found = false;
                foreach (var item in sequence2)
                {
                    // This method is an O(ln n) operation.
                    found &= collection.Contains(new User(item, "family", "name"));
                }
            }

            timer.Stop();
            return timer.Elapsed;
        }

        /// <summary>
        /// adds several items to a collection, then searches for items in that collection. The test is repeated 50 times to get a more consistent result
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="numItemsToTest"></param>
        /// <returns></returns>
        static TimeSpan RunTestOneSortedDict(SortedDictionary<int, User> collection, int numItemsToTest)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // perform the test 50 times: 
            for (int counter = 0; counter < 50; counter++)
            {
                collection.Clear();
                // add some random items 
                Random rnd = new Random();
                var sequence = from n in Enumerable.Range(1, numItemsToTest)
                               select rnd.Next() * n;

                foreach (var item in sequence)
                {
                    int i = item;

                    while (collection.ContainsKey(i) && i < numItemsToTest * 2)
                        i += 1;

                    if(!collection.ContainsKey(i))
                        collection.Add(i, new User(i, "family", "name"));
                }

                // search for 1000 random items 
                var sequence2 = from n in Enumerable.Range(1, 1000)
                                select rnd.Next() * n;

                bool found = false;
                foreach (var item in sequence2)
                {
                    // This method is an O(log n) operation.
                    found &= collection.ContainsKey(item);
                }
            }

            timer.Stop();
            return timer.Elapsed;
        }

        /// <summary>
        ///  test adds several items to the collection, and then enumerates the collection in sorted order. The test is repeated 50 times to get a more consistent result
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="numItemsToTest"></param>
        /// <returns></returns>
        static TimeSpan RunTestTwoSortedSet(SortedSet<User> collection, int numItemsToTest)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // perform the test 50 times: 
            for (int counter = 0; counter < 50; counter++)
            {
                collection.Clear();

                // add random items 
                Random rnd = new Random();

                var sequence = from n in Enumerable.Range(1, numItemsToTest)
                               select rnd.Next() * n;

                foreach (var item in sequence)
                    collection.Add(new User(item, "family", "name"));


                // enumerate 1000 times 
                double sum = 0;
                for (int inner = 0; inner < 1000; inner++)
                {
                    //  I am performing the sort each time through the loop on purpose, even though it does take more time. 
                    // I want to measure the cost of the sort because in most applications where order matters, you would need
                    // to enumerate the set in different locations in the code, or after items have been added or removed. 
                    // That requires a sort.
                    //  IEnumerable ordered = collection as IEnumerable;
                    //SortedDictionary<int, User>.ValueCollection values = collection.Values;
                    //var max = collection.Values.Max();
                    IEnumerable ordered = collection as IEnumerable;

                    // var sum = collection.Values.Sum();
                    // must enumerate to make it work: 
                    // sum += ordered.Sum();
                    ordered.GetEnumerator();
                }
            }

            timer.Stop();
            return timer.Elapsed;
        }

        /// <summary>
        ///  test adds several items to the collection, and then enumerates the collection in sorted order. The test is repeated 50 times to get a more consistent result
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="numItemsToTest"></param>
        /// <returns></returns>
        static TimeSpan RunTestTwoSortedDict(SortedDictionary<int, User> collection, int numItemsToTest)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // perform the test 50 times: 
            for (int counter = 0; counter < 50; counter++)
            {
                collection.Clear();

                // add random items 
                Random rnd = new Random();

                var sequence = from n in Enumerable.Range(1, numItemsToTest)
                               select rnd.Next() * n;

                foreach (var item in sequence)
                    collection.Add(item, new User(item, "family", "name"));


                // enumerate 1000 times 
                double sum = 0;
                for (int inner = 0; inner < 1000; inner++)
                {
                    //  I am performing the sort each time through the loop on purpose, even though it does take more time. 
                    // I want to measure the cost of the sort because in most applications where order matters, you would need
                    // to enumerate the set in different locations in the code, or after items have been added or removed. 
                    // That requires a sort.
                  //  IEnumerable ordered = collection as IEnumerable;
                    //SortedDictionary<int, User>.ValueCollection values = collection.Values;
                    //var max = collection.Values.Max();
                    IEnumerable ordered = collection.Values as IEnumerable;

                   // var sum = collection.Values.Sum();
                    // must enumerate to make it work: 
                   // sum += ordered.Sum();
                    ordered.GetEnumerator();
                }
            }

            timer.Stop();
            return timer.Elapsed;
        }
    }
}
