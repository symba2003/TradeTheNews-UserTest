namespace UserTest
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

	class Program
	{
		#region Methods

		static void Main(string[] args)
		{
            //DONE: написать тест: создаем репозиторий
            // получаем к-л User, меняем его
            // отобразится ли изменение в репозитории?
            // Ответ: для MyUserREpositoiry: изменение полученного User, меняет состояние репозитория
            // поэтому, был создан UserRepositoryWithCopiesOfObjects, который возвращает копию объекта из репозитория
            // и, соответсвенно, изменение копии объекта, не отражается на самом объекте и на хранилище его содержащем
            // Но для решения поставленной задачи, использование копий объектов - является "overkill".

            //TestMyUserRepository();

            //TestUserCopies();

           // TestUserRepositoryWithCopiesOfObjects();

            // итоговый результат
           // TestUserRepositoryResult();

            // сравнение двух итоговых результатов
            TestUserRepositoryResultVSUserRepositoryWithReaderWriterLock();

           

            #region Comment
            /*
                UserRepository repository = new UserRepository();
                repository.AddUser(new User(4, "4"));
                repository.AddUser(new User(1, "1"));
                repository.AddUser(new User(1, "11"));
                repository.AddUser(new User(7, "7"));
                repository.UpdateUser(new User(7, "77"));
                repository.AddUser(new User(3, "3"));
                repository.AddUser(new User(9, "9"));
                repository.AddUser(new User(2, "2"));

                User u = repository.GetUser(music);

                foreach (User user in repository.GetOrderedUsers())
                {
                    Console.WriteLine(user);
                }
                 * 
                 *  UserRepository repository = new UserRepository();
                repository.AddUser(new User(4, "4"));
                repository.AddUser(new User(1, "1"));
                repository.AddUser(new User(1, "11"));
                repository.AddUser(new User(7, "7"));
           
                repository.AddUser(new User(3, "3"));
                repository.AddUser(new User(9, "9"));
                repository.AddUser(new User(2, "2"));

           
                 * */
            //DateTime startTime1 = DateTime.Now; 
            //ArrayList al = new ArrayList();
            //for (int i = 0; i < 1000000; i++)
            //    al.Add(i); //Implicitly boxed because Add() takes an object
            //int f = (int)al[0]; // The element is unboxed
            //DateTime stopTime1 = DateTime.Now;
            //TimeSpan duration1 = stopTime1 - startTime1;
            //Console.WriteLine("First task duration: {0} milliseconds.", duration1.TotalMilliseconds);


            //DateTime startTime2 = DateTime.Now; 
            //int[] al2 = new int[1000000];
            //for (int i = 0; i < 1000000; i++)
            //    al2[i] = i; //Implicitly boxed because Add() takes an object
            //int f2 = al2[0]; // The element is unboxed
            //DateTime stopTime2 = DateTime.Now;
            ///* Compute and print the duration of this second task. */
            //TimeSpan duration2 = stopTime2 - startTime2;
            //Console.WriteLine("Second task duration: {0} milliseconds.", duration2.TotalMilliseconds); 


            /*
            // load up a dictionary.
            var dictionary = new ConcurrentDictionary<string, int>();

            dictionary["A"] = 1;
            dictionary["B"] = 2;
            dictionary["C"] = 3;
            dictionary["D"] = 4;
            dictionary["E"] = 5;
            dictionary["F"] = 6;

            // attempt iteration in a separate thread
            var iterationTask = new Task(() =>
            {
                // iterates using a dirty read
                foreach (var pair in dictionary)
                {
                    Console.WriteLine(pair.Key + ":" + pair.Value);
                }
            });

            // attempt updates in a separate thread
            var updateTask = new Task(() =>
            {
                // iterates, and updates the value by one
                foreach (var pair in dictionary)
                {
                    dictionary[pair.Key] = pair.Value + 1;
                }
            });

            // start both tasks
            updateTask.Start();
            iterationTask.Start();

            // wait for both to complete.
            Task.WaitAll(updateTask, iterationTask)
            */
            
            #endregion

            Console.ReadLine();
		}


        static void TestMyUserRepository() 
        {
            Console.WriteLine("/**************************************************************************************************/");
            Console.WriteLine("Test MyUserRepository");
            
            User user3 = new User(3, "family3", "name3");
            User user1 = new User(1, "family1", "name1");
            User user2 = new User(2, "family2", "name2");
            User user4 = new User(4, "family4", "name4");
            User user5 = new User(5, "family5", "name5");

            MyUserRepository ur = new MyUserRepository();
            ur.AddUser(user3);
            ur.AddUser(user1);
            ur.PrintRepository();
            ur.AddUser(user2);
            ur.PrintRepository();
            ur.AddUser(user5);
            ur.AddUser(user4);


            Console.WriteLine("CurrentRepository contains..");
            ur.PrintRepository();


            Console.WriteLine("Change 3rd user's FamilyName");
            User someUser = ur.GetUser(3);
            someUser.FamilyName = "user333333";
            Console.WriteLine("Repository Now Contains..");
            // Да, изменения видны
            ur.PrintRepository();

            //INFO: exception here
            //ur.AddUser(someUser);

            Console.WriteLine("Get Ordered Users..");
            User[] usersArray = ur.GetOrderedUsers();
            for (int i = 0; i < usersArray.Length; i++)
            {
                Console.WriteLine(String.Format("{0}: {1} {2}.", usersArray[i].UserID, usersArray[i].FamilyName, usersArray[i].GivenName));
            }

            Console.WriteLine("Change first user's family");
            usersArray[0].FamilyName = "user1111111111";
            Console.WriteLine("Repoitory now:");
            ur.PrintRepository();
            Console.WriteLine("/**************************************************************************************************/");
        }

        static void TestUserCopies() 
        {
            // Test user copies
            Console.WriteLine("/**************************************************************************/");
            Console.WriteLine("Test User Copies");
            Console.WriteLine("Create second user based on first user fields. Create third user as \"user3 = user;\"");
            User u1 = new User(1, "family", "name");
            User u2 = new User(u1.UserID, u1.FamilyName, u1.GivenName);
            User u3 = u1;
            Console.WriteLine("First user: " + u1.ToString());
            Console.WriteLine("SecondUser: " + u2.ToString());
            Console.WriteLine("ThirdUser: " + u3.ToString());
            Console.WriteLine("Change first user's family, and look if it changes in the second user and third.");
            u1.FamilyName = "new_family";
            //По идее, u1 теперь ссылается на др строку (это связано с особенностями работы со строками)
            Console.WriteLine("First user: " + u1.ToString());
            Console.WriteLine("SecondUser: " + u2.ToString());
            Console.WriteLine("ThirdUser: " + u3.ToString());

            Assert.AreNotEqual(u1.FamilyName, u2.FamilyName);

            Assert.AreEqual(u1.FamilyName, u3.FamilyName);
            
            Console.WriteLine("/*************************************************************************/");
        }

        static void TestUserRepositoryWithCopiesOfObjects() 
        {
            Console.WriteLine("/****************************************************************************************/");
            Console.WriteLine("Test user repository with copies of original Users");
            User user_3 = new User(3, "family3", "name3");
            User user_1 = new User(1, "family1", "name1");
            User user_2 = new User(2, "family2", "name2");

            User user_4 = new User(4, "family4", "name4");
            User user_5 = new User(5, "family5", "name5");

            UserRepositoryWithCopiesOfObjects ur2 = new UserRepositoryWithCopiesOfObjects();
            ur2.AddUser(user_3);
            ur2.AddUser(user_1);
            ur2.PrintRepository();
            ur2.AddUser(user_2);
            ur2.PrintRepository();
            ur2.AddUser(user_5);
            ur2.AddUser(user_4);

            Console.WriteLine("CurrentRepository contains..");
            ur2.PrintRepository();

            Console.WriteLine("Change 3rd user's FamilyName");
            User some_User = ur2.GetUser(3);
            some_User.FamilyName = "user333333";
            Console.WriteLine("Repository Now Contains..");
            // так как UserRepositoryWithCopiesOfObjects отдает копию User, изменение фамилии не отражается на репозитории
            ur2.PrintRepository();

            //INFO: no exception here!
            ur2.AddUser(some_User);

            Console.WriteLine("Get Ordered Users  Array..");
            User[] users_Array = ur2.GetOrderedUsers();
            for (int i = 0; i < users_Array.Length; i++)
            {
                Console.WriteLine(users_Array[i].ToString());
            }

            Console.WriteLine("Change first user's family in this Array..");
            Console.WriteLine("ReferenceEquals: " + Object.ReferenceEquals(ur2.GetUser(1), users_Array[0]).ToString());
            users_Array[0].FamilyName = "user1111111111";
          
            Console.WriteLine("Repository now:");
            ur2.PrintRepository();
            Console.WriteLine("/******************************************************************************/");
        }

        static void TestUserRepositoryResult()
        {
            Console.WriteLine("/****************************************************************************************/");
            Console.WriteLine("Test UserRepositoryResult");

            int numItemsToTest = 20000;
            int halfOfItems = numItemsToTest / 2;


            
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 10; i++)
            {
                // создаем репозиторий
                UserRepositoryResult urr = new UserRepositoryResult();
                // pre-fill the half of the collection with test data
                Random rnd = new Random();
                var sequence = from n in Enumerable.Range(1, halfOfItems)
                               select rnd.Next(numItemsToTest);
                foreach (var item in sequence)
                    urr.AddUser(new User(item, String.Format("family_{0}", item), "name"));

                timer.Stop();
                Console.WriteLine(String.Format("Elapsed time: {0}", timer.Elapsed));
                ////create different tasks


                // 1. Fill in remaining collection with data in a separate thread
                var fillTask = new Task(() =>
                {
                    var nextSequence = from n in Enumerable.Range(halfOfItems + 1, numItemsToTest)
                                       select rnd.Next(numItemsToTest);
                    foreach (var item in nextSequence)
                        urr.AddUser(new User(item, String.Format("family_{0}", item), "name"));
                });


                // 2. get some User and print it
                var getSomeUserTask = new Task(() =>
                {
                    var someUserSequence = from n in Enumerable.Range(1, numItemsToTest)
                                           select rnd.Next(numItemsToTest);

                    foreach (var item in someUserSequence)
                    {
                        User someUser = urr.GetUser(item);
                        //if (someUser != null)
                        //    Console.WriteLine("GET_USER: {0}", someUser.ToString());
                    }
                });

                // 3. Get Ordered Users collection and print it
                var printOrderedUsersTask = new Task(() =>
                {
                    timer.Start();
                    //for (int j = 0; j < 100; j++)
                    //{
                    //    User[] orderedUsers = urr.GetOrderedUsers();
                    //    Console.WriteLine("**************************************");
                    //}
                    timer.Stop();
                    Console.WriteLine(String.Format("Elapsed time: {0}", timer.Elapsed));
                });


                // start all tasks
                fillTask.Start();
                getSomeUserTask.Start();
                printOrderedUsersTask.Start();

                // wait for all to complete.
                Task.WaitAll(fillTask, getSomeUserTask, printOrderedUsersTask);

                //print resulted array
                User[] orderedUsers2 = urr.GetOrderedUsers();
                Console.WriteLine("RESULT **************************************");
                for (int counter = 0; counter < 5; counter++)
                {
                    Console.WriteLine(String.Format("ORDERED: {0}", orderedUsers2[counter].ToString()));
                }
            }
        }

        static void TestUserRepositoryResultVSUserRepositoryWithReaderWriterLock() 
        {
            Console.WriteLine("/****************************************************************************************/");
            Console.WriteLine("Test UserRepositoryResult vs UserRepositoryWithReaderWriterLock");

            int numItemsToTest = 20000;
         //   int halfOfItems = numItemsToTest / 2;


            Console.WriteLine("Test UserRepositoryResult..");
            for (int i = 0; i < 10; i++)
            {
            // создаем репозиторий
            UserRepositoryResult urr = new UserRepositoryResult();
            TestUserRepository(urr, numItemsToTest);
            }
            Console.WriteLine("End of test");

            Console.WriteLine("Test UserRepositoryWithReaderWriterLock..");
            for (int i = 0; i < 10; i++)
            {
                // создаем репозиторий
                UserRepositoryWithReaderWriterLock ur = new UserRepositoryWithReaderWriterLock();
                TestUserRepository(ur, numItemsToTest);
            }
            Console.WriteLine("End of test");

        }

        static void TestUserRepository(IUserRepository ur, int numItemsToTest) 
        {
            Stopwatch timer = new Stopwatch();
            
            // pre-fill the half of the collection with test data
            Random rnd = new Random();

            ////create different tasks
            // 1. Fill in remaining collection with data in a separate thread
            var fillTask = new Task(() =>
            {
                timer.Start();
                var nextSequence = from n in Enumerable.Range(1, numItemsToTest)
                                    select rnd.Next(numItemsToTest);
                foreach (var item in nextSequence)
                    ur.AddUser(new User(item, String.Format("family_{0}", item), "name"));
                timer.Stop();
                Console.WriteLine(String.Format("fillTask Elapsed time: {0}", timer.Elapsed));
            });


            // 2. get some User and print it
            var getSomeUserTask = new Task(() =>
            {
                var someUserSequence = from n in Enumerable.Range(1, numItemsToTest)
                                        select rnd.Next(numItemsToTest);

                foreach (var item in someUserSequence)
                {
                    User someUser = ur.GetUser(item);
                    //if (someUser != null)
                    //    Console.WriteLine("GET_USER: {0}", someUser.ToString());
                }
            });

            // 3. Get Ordered Users collection and print it
            var printOrderedUsersTask = new Task(() =>
            {
                Stopwatch timer2 = new Stopwatch();
                timer2.Start();
                for (int j = 0; j < numItemsToTest; j++)
                {
                    User[] orderedUsers = ur.GetOrderedUsers();
                    //Console.WriteLine("**************************************");
                    //for (int counter = 0; counter < 5; counter++)
                    //{
                    //    Console.WriteLine(String.Format("ORDERED: {0}", orderedUsers[counter].ToString()));
                    //}
                    //Console.WriteLine("**************************************");
                }
                timer2.Stop();
                Console.WriteLine(String.Format("printOrderedUsersTask Elapsed time: {0}", timer2.Elapsed));
            });


            // start all tasks
            fillTask.Start();
            getSomeUserTask.Start();
            printOrderedUsersTask.Start();

            // wait for all to complete.
            Task.WaitAll(fillTask, getSomeUserTask, printOrderedUsersTask);
        }
		#endregion Methods
	}
}