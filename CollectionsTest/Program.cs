using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionsTest
{
    class Program
    {
        // http://msdn.microsoft.com/en-us/vstudio/ee906600.aspx
        /**
         * Writing a sample like this is tricky: I want to highlight the differences between these collection styles, 
         * but I don’t want to lead you to believe that the test code validates all assumptions in all cases.
         * The sample shows the differences in the time cost of these two different types of collections, but that’s no excuse 
         * to argue dogmatically instead of measuring. It’s true that HashSet<T> will be faster than SortedSet<T> on Add and
         * Search operations. It’s also true that SortedSet<T> will be faster when you need to enumerate the set in a sorted order.
         * What must be measured is which collection would be better and faster in your application. 
         * In some cases, you may find that a design that combines both collection classes provides the needed performance. 
         * The only way to know that is to measure typical scenarios in your application using both collection types. 
         * The only technique I want you to recognize is universal is to enable changing the collection types in your application. 
         * By writing your code using interfaces, hopefully only on the particular functionality you need to create your application. 
         * You may even need to write some utility methods to enable cheaper tests instead of blindly performing unneeded work.
         * ***************/
        static void Main(string[] args)
        {
            Program2.Run();
            Console.Read();



            //var hashSet = new HashSet<double>();
            //var sortedSet = new SortedSet<double>();

            //var testSizesForTestOne = new int[] { 100, 1000, 10000, 50000, 100000 };
            //var testSizesForTestTwo = new int[] { 10, 50, 100, 500, 1000, 5000 };

            //RunTest(hashSet, sortedSet, testSizesForTestOne, "TestOne",
            //     (set, testSize) => RunTestOne(set, testSize));

            //RunTest(hashSet, sortedSet, testSizesForTestTwo, "TestTwo",
            //    (set, testSize) => RunTestTwo(set, testSize));
        }

        private static void RunTest(HashSet<double> hashSet, SortedSet<double> sortedSet, IEnumerable<int> testSizes,
            string label,
            Func<ISet<double>, int, TimeSpan> testFunc)
        {

            foreach (int test in testSizes)
            {
                var result = testFunc(hashSet, test);
                Console.WriteLine("{0} with Hash Set ({1}): {2}", label, test, result);
                result = testFunc(sortedSet, test);
                Console.WriteLine("{0} with Sorted Set ({1}): {2}", label, test, result);
                hashSet.Clear();
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
        static TimeSpan RunTestOne(ISet<double> collection, int numItemsToTest)
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
                               select rnd.NextDouble() * n;

                foreach (var item in sequence)
                    collection.Add(item);


                // search for 1000 random items 
                var sequence2 = from n in Enumerable.Range(1, 1000)
                                select rnd.NextDouble() * n;

                bool found = false;
                foreach (var item in sequence2)
                {
                    found &= collection.Contains(item);
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
        static TimeSpan RunTestTwo(ISet<double> collection, int numItemsToTest)
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
                               select rnd.NextDouble() * n;

                foreach (var item in sequence)
                    collection.Add(item);


                // enumerate 1000 times 
                double sum = 0;
                for (int inner = 0; inner < 1000; inner++)
                {
                    //  I am performing the sort each time through the loop on purpose, even though it does take more time. 
                    // I want to measure the cost of the sort because in most applications where order matters, you would need
                    // to enumerate the set in different locations in the code, or after items have been added or removed. 
                    // That requires a sort.
                    IEnumerable<double> ordered = collection.IsSorted() ?
                                                    collection as IEnumerable<double> :
                                                    from item in collection
                                                    orderby item
                                                    select item;

                    // must enumerate to make it work: 
                    sum += ordered.Sum();
                }
            }

            timer.Stop();
            return timer.Elapsed;

        }
    }

    //  the call to IsSorted(). ISet<T> does not have this method, so I wrote an extension method that tests for a sorted sequence
    public static class Extensions
    {
        public static bool IsSorted<T>(this IEnumerable<T> sequence)
            where T : IComparable<T>
        {
            // Simplified algorithm for this example:
            // Production code would look for other sorted collections 
            // (SortedList, etc)
            if (sequence is SortedSet<T>)
                return true;
            IEnumerator<T> iter = sequence.GetEnumerator();
            if (!iter.MoveNext())
                return true;

            T value = iter.Current;
            while (iter.MoveNext())
            {
                if (value.CompareTo(iter.Current) > 0)
                    return false;
                value = iter.Current;
            }
            return true;
        }
    }
}
