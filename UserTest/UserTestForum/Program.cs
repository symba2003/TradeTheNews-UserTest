using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTestForum
{
    class Program
    {
        static void Main(string[] args)
        {
            /**
             * Тестовый проект, для проверки почему ConcurentDictionary(int, User) оказался сортированным *
             * Ответ: потому что ConcurentDictionary - это hash-table
             * А для распределения объектов используется хэш-функция, которая для целого числа возвращает его же
             * Хэш-ключи сортированы, поэтому оказалось что и ConcurentDictionary сортирован
             * 
             * Для проверки этого, вместо целочисленного ключа стала использовать строковый: ConcurentDictionary(string, User)
             * И в этом случае ConcurentDictionary действительно оказался несортированным **********/
            User user7 = new User("g2", "family2", "name2");
            User user8 = new User("h2", "family2", "name2");
            User user9 = new User("i2", "family2", "name2");
            User user10 = new User("j2", "family2", "name2");


            User user3 = new User("c3", "family3", "name3");
            User user1 = new User("a1", "family1", "name1");
            User user2 = new User("b2", "family2", "name2");
            User user4 = new User("d2", "family2", "name2");
            User user5 = new User("e2", "family2", "name2");
            User user6 = new User("f2", "family2", "name2");

            UserRepository ur = new UserRepository();
            ur.AddUser(user3);
            ur.AddUser(user1);
            Console.WriteLine("Two users were added (UserID=1 and UserID=3)");
            ur.PrintRepository();
            ur.AddUser(user2);
            Console.WriteLine("One user was added (UserID=2)");
            ur.PrintRepository();

            ur.AddUser(user8);
            ur.AddUser(user10);
            ur.AddUser(user4);
            ur.AddUser(user5);
            ur.PrintRepository();

            ur.AddUser(user9);
            ur.AddUser(user6);
            ur.AddUser(user7);


            Console.WriteLine("CurrentRepository contains..");
            ur.PrintRepository();


            Console.ReadLine();
        }
    }

    class UserRepository
    {
        // It is important that the GetUser and GetOrderedUsers will execute as quickly as possible.
        // Memory consumption is not a concern.

        // Will store 2 collections: optimized for concurent access and sorted

        private ConcurrentDictionary<string, User> concurentUsers = new ConcurrentDictionary<string, User>();
        private SortedDictionary<string, User> sortedUsers = new SortedDictionary<string, User>();

        private Object synchronizer = new object();

        public void AddUser(User user)
        {
            // Write operation (AddUser) will be called much less often, but also can be called from several threads at the same time.
            lock (synchronizer)
            {
                concurentUsers.TryAdd(user.UserID, user);

                sortedUsers.Add(user.UserID, user);
            }
        }

        public User[] GetOrderedUsers()
        {
            User[] users = null;

            lock (synchronizer)
            {
                users = sortedUsers.Values.ToArray<User>();
            }
            return users;
        }

        public User GetUser(string user_id)
        {
            //no lock here, since it's ConcurentCollection
            return concurentUsers[user_id];
        }

        public void PrintRepository()
        {
            Console.WriteLine("Repository of users: ");
            
            // DONE: Why does this collection is sorted?
            // because Dictionary - is a HashSet.
            // The dictonary's key - is of type "int"
            // and hash-function for int, returns that int
            foreach (KeyValuePair<string, User> pair in concurentUsers)
            {
                Console.WriteLine(pair.Value.ToString());
            }
            PrintSortedRepository();
            Console.WriteLine();
        }

        public void PrintSortedRepository()
        {
            Console.WriteLine("Sorted Repository of users: ");

          
            foreach (KeyValuePair<string, User> pair in sortedUsers)
            {
                Console.WriteLine(pair.Value.ToString());
            }

            Console.WriteLine();
        }
    }

    public class User
    {
        public User(string user_id, string family_name, string given_name)
        {
            UserID = user_id;
            FamilyName = family_name;
            GivenName = given_name;
        }

        public string FamilyName
        {
            get;
            set;
        }

        public string GivenName
        {
            get;
            set;
        }

        public string UserID
        {
            get;
            set;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}, {2}", UserID, FamilyName, GivenName);
        }
    }
}
