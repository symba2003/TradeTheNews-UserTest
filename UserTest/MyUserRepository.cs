using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserTest
{
    /// <summary>
    /// Класс-результат
    /// </summary>
    class MyUserRepository
    {
        // Since requirements state that: 
        // It is important that the GetUser and GetOrderedUsers will execute as quickly as possible.
        // Memory consumption is not a concern.

        // Will store 2 collections: optimized for concurent access and sorted

        private ConcurrentDictionary<int, User> concurentUsers = new ConcurrentDictionary<int, User>();

        //DONE: decide which SortedCollection to choose: SortedDictionary OR SortedSet - см ниже
        // .NET 4.0 added a 'SortedSet<T>' which is a sorted HashSet. 
        private SortedDictionary<int, User> sortedUsers = new SortedDictionary<int, User>();

        /** ABOUT COLLECTIONS *
         * 
         * The Dictionary <TKey,TVale> is probably the most used associative container class. 
         * The Dictionary<TKey,TValue> is the fastest class for associative lookups/inserts/deletes because it uses a hash table 
         * under the covers. Because the keys are hashed, the key type should correctly implement GetHashCode() and Equals() appropriately
         * or you should provide an external IEqualityComparer to the dictionary on construction. The insert/delete/lookup time of items 
         * in the dictionary is amortized constant time - O(1) - which means no matter how big the dictionary gets, the time it takes
         * to find something remains relatively constant. This is highly desirable for high-speed lookups. The only downside is that
         * the dictionary, by nature of using a hash table, is unordered, so you cannot easily traverse the items in a Dictionary in order.
         * 
         * The SortedDictionary<TKey,TValye> uses a binary tree under the covers to maintain the items in order by the key. 
         * As a consequence of sorting, the type used for the key must correctly implement IComparable<TKey> so that the keys can be
         * correctly sorted. The sorted dictionary trades a little bit of lookup time for the ability to maintain the items in order,
         * thus insert/delete/lookup times in a sorted dictionary are logarithmic - O(log n). Generally speaking, with logarithmic time,
         * you can double the size of the collection and it only has to perform one extra comparison to find the item. Use 
         * the SortedDictionary<TKey,TValue> when you want fast lookups but also want to be able to maintain the collection in order 
         * by the key.
         * 
         * The SortedSet<T> is to HashSet<T> what the SortedDictionary<TKey,TValue> is to Dictionary<TKey,TValue>. That is, the 
         * SortedSet<T> is a binary tree where the key and value are the same object. This once again means that adding/removing/lookups
         * are logarithmic - O(log n) - but you gain the ability to iterate over the items in order. For this collection to be effective,
         * type T must implement IComparable<T> or you need to supply an external IComparer<T>.
         * 
         * SortedSet<T> - is a unique sorted collection, like SortedDictionary except key and value are same object.
         * 
         * Так как и SortedSet<T>, и  SortedDictionary<TKey,TValue> являются binary tree, то по скорости они одинаковы.
         * При этом для SortedSet<T> класс T (в нашем случае User) должен дополнительно implement  IComparable<T>, а для 
         * SortedDictionary<int, User>: int уже наследует IComparable. То поэтому я выбрала SortedDictionary<int, User> для хранения сортированного списка
         * 
         * //INFO: тесты (см. блог) показали, что вставка в Sorted Set быстрее происходит..
         * но насколько можно им доверять? http://blog.chronoclast.com/2011/01/sorted-speeds.html
         * Answer: по мои тестам в проекте CollectionTest (Program2) - получилось примерно одинаково..
         * // Note: as a side note, there are sorted implementations of IDictionary, namely SortedDictionary and SortedList 
         * which are stored as an ordered tree and a ordered list respectively.  While these are not as fast as the non-sorted
         * dictionaries – they are O(log2 n) – they are a great combination of both speed and ordering -- and still greatly outperform 
         * a linear search.
         *    The ConcurrentDictionary gets even more efficient the higher the ratio of reads-to-writes you have.
         */

        private Object synchronizer = new object();

        #region Methods


        public void AddUser(User user)
        {
            // Write operation (AddUser) will be called much less often, but also can be called from several threads at the same time.
            lock (synchronizer)
            {
                // операция дб атомарна: добавление пользователя в обе коллекции
                // DONE: что если такой USER_ID уже существует?? Надо ли вызывать ContainsKey?
                //       для данного метода - не надо вызывать ContainsKey - 
                //      так как если User существует, метод не будет пытаться добавить User  с таким же ключом
                concurentUsers.TryAdd(user.UserID, user);

                //concurentUsers.AddOrUpdate(user.UserID, user);
                //INFO: здесь exception =- если такой key уже естьв коллекции
                sortedUsers.Add(user.UserID, user);
            }
        }

        public User[] GetOrderedUsers()
        {
            User[] users = null;
            //INFO: is this lock needed? Answer: yes
            lock (synchronizer)
            {
                //DONE: is this copies of User objects? - 
                //      Нет, если изменим объект user, полученный из GetOrderedUsers,
                //      то репозиторий тоже изменится
                // INFO: is it ordered by UserID? Answer: yes
                users = sortedUsers.Values.ToArray<User>();
                //sortedUsers.CopyTo();
                 
            }
            return users;
        }

        public User GetUser(int user_id)
        {
            //no lock here, since it's ConcurentCollection
            return concurentUsers[user_id];

            /* // When a program often has to try keys that turn out not to 
        // be in the dictionary, TryGetValue can be a more efficient  
        // way to retrieve values. 
        string value = "";
        if (openWith.TryGetValue("tif", out value))
        {
            Console.WriteLine("For key = \"tif\", value = {0}.", value);
        }
        else
        {
            Console.WriteLine("Key = \"tif\" is not found.");
        }
*/


        }

        #endregion Methods

        public void PrintRepository() 
        {
            // DONE: метод почему то выдает сортированую коллекцию, см. проект UserTestForum
            foreach (int key in concurentUsers.Keys) 
            {
                Console.WriteLine(concurentUsers[key].ToString());
            }

            foreach(KeyValuePair<int, User> pair in concurentUsers)
            {
                Console.WriteLine(pair.Value.ToString());
            }
        }

        /*from stackoverflow
         * 
         * I am able to sort my ConcurrentDictionary by value like so:

static ConcurrentDictionary<string, Proxy> Proxies = 
    new ConcurrentDictionary<string, Proxy>();

Proxies.OrderBy(p => p.Value.Speed);
Which is great, except I want to set that new re-ordered list AS the dictionary, effectively sorting the dictionary itself rather than just receiving a result list of sorted items.

I try to do something like this but had no luck - the dictionary is still unordered after:

Proxies = new ConcurrentDictionary<string,Proxy>(
    Proxies.OrderBy(p => p.Value.Speed));
It seems like doing that has no effect on the dictionary. I also tried casting the OrderBy result to a new var thinking that it may have an effect on the delegate but still no luck.

How can I re-order this ConcurrentDictionary and then force the dictionary to be the re-ordered result from OrderBy?
         * 
         * 
         * 
         * 
         * 
         * 
         A dictionary, especially ConcurrentDictionary, is essentially unsorted.
         If you need a sorted collection, you'll need to store the values in some other type, such as a SortedDictionary<T,U>.
         * 
         *  was hoping to utilize the built-in .NET concurrency offered by the ConcurrentDictionary, though. I can see this is not possible while offering sorted capabilities. Back to using lock() statements... thanks
         *  
         * Just be careful - ordering and concurrent collections usually conflict... Typically, (when possible) it's a good idea to keep your data in a concurrent collection while working on it, then extract the items in a specific order to present results, etc
         * 
         * ConcurrentDictionary, just like Dictionary, doesn't know the concept of sorting, i.e. it doesn't contain any ordering information. The result of OrderBy() has a specific order, but when assigned to Proxies the order information is lost.
            Note that there are sorted implementations of IDictionary, namely SortedDictionary and SortedList.
         * 
         * 
         * The solution is to use a SortedSet<T>, discovered after many hours of research and code revision. A sorted set offers the unique-ness of a Dictionary or HashSet, but also allows sorting - neither a Dictionary nor a HashSet allow sorting.
         
         
         */


        /*
         * http://social.msdn.microsoft.com/Forums/vstudio/en-US/9b18f4c5-bb09-4da1-9f88-034632022300/sorting-a-concurrentconcurrentdictionary
         * 
         * ConcurrentDictionary is an unordered data structure, thus the ordering that you add to it is not preserved.

Consider using SortedDictionary and wrap reads and writes to it with a lock to make it thread-safe.  This is, of course, not as finely tuned for concurrency as the classes in the System.Collections.Concurrent namespace, but it will suffice for applications that are not particularly demanding.
         * 
         * 
         * (You might experiment whether you can improve performance by using ReaderWriterLockSlim which admits multiple concurrent readers.  However in cases like this where the reader lock is only held for extremely brief intervals of time, I get worried that the RW lock will introduce more performance overhead than its worth.  Never actually got around to testing the performance, though.  Maybe someone could enlighten me...)
         * 
         * private Object m_Lock = new object();
private SortedDictionary<string, int> m_Dict = new SortedDictionary<string, int>();

void Main()
{
	// Writing
	lock (m_Lock)
	{
		m_Dict.Add("Hello", 1);
		m_Dict.Add("World", 2);
	}
	
	// Reading
	int val;
	lock (m_Lock)
	{
		val = m_Dict["World"];
	}
	
	Console.WriteLine(val); // Prints 2
}
 
         * 
         * 
         * 
         * 
         * **/

        /*
         Attempting to synchronize internally will almost certainly be insufficient because it's at too low a level of abstraction. Say you make the Add and ContainsKey operations individually thread-safe as follows:

public void Add(TKey key, TValue value)
{
    lock (this.syncRoot)
    {
        this.innerDictionary.Add(key, value);
    }
}

public bool ContainsKey(TKey key)
{
    lock (this.syncRoot)
    {
        return this.innerDictionary.ContainsKey(key);
    }
}
Then what happens when you call this supposedly thread-safe bit of code from multiple threads? Will it always work OK?

if (!mySafeDictionary.ContainsKey(someKey))
{
    mySafeDictionary.Add(someKey, someValue);
}
The simple answer is no. At some point the Add method will throw an exception indicating that the key already exists in the dictionary. How can this be with a thread-safe dictionary, you might ask? Well just because each operation is thread-safe, the combination of two operations is not, as another thread could modify it between your call to ContainsKey and Add.

Which means to write this type of scenario correctly you need a lock outside the dictionary, e.g.

lock (mySafeDictionary)
{
    if (!mySafeDictionary.ContainsKey(someKey))
    {
        mySafeDictionary.Add(someKey, someValue);
    }
}
But now, seeing as you're having to write externally locking code, you're mixing up internal and external synchronisation, which always leads to problems such as unclear code and deadlocks. So ultimately you're probably better to either:

Use a normal Dictionary<TKey, TValue> and synchronize externally, enclosing the compound operations on it, or

Write a new thread-safe wrapper with a different interface (i.e. not IDictionary<T>) that combines the operations such as an AddIfNotContained method so you never need to combine operations from it.
         
         */
    }
}
