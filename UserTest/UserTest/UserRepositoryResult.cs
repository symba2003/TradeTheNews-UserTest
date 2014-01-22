using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTest
{
    /// <summary>
    /// Класс-результат.
    /// </summary>
    /// <remarks> Предполагаем, что объекты User - immutable (неизменяемы, read-only)</remarks>
    class UserRepositoryResult
    {
        // главное помни, что thread-safe еще означает, что ты не будешь выставлять ссылки в public-интерфейс
        
        // коллекция для более быстрого доступа к объектам
        private ConcurrentDictionary<int, User> concurrentUsers = new ConcurrentDictionary<int, User>();

        //кроллекция, хранящая сортированный массив объектов
        private SortedDictionary<int, User> sortedUsers = new SortedDictionary<int, User>();
        
        private Object synchronizer = new Object();

        public User GetUser(int userId)
        {
            //INFO: не подходит! а если пользователя с таким id нет в коллекции? исключение!
            //return concurentUsers[user_id];

            User user = null;
            concurrentUsers.TryGetValue(userId, out user);
            return user;
        }

        public void AddUser(User user)
        {
            lock (synchronizer)
            {
                //if (!concurrentUsers.ContainsKey(user.UserID)) 
                //{
                //    concurrentUsers.TryAdd(user.UserID, user);
                //    sortedUsers.Add(user.UserID, user);
                //}

                if (concurrentUsers.TryAdd(user.UserID, user))
                    sortedUsers.Add(user.UserID, user);
                // или: (произв-ть существенно не изменится...
                // это O(1)
                // и оно тут самое маленькое
                // и оно все равно выполнится
                // декомпилируй "оптимизированный" метод, посмотри, что получится
                // декомпилируй "неоптимизированный" метод, посмотри, что получится.
                // уверен, получится одно и то же
                //bool exists = concurrentUsers.TryAdd(user.UserID, user);
                //if (exists)
                //    sortedUsers.Add(user.UserID, user);
            }
        }

        public User[] GetOrderedUsers()
        {
            lock (synchronizer)
            {
                return sortedUsers.Values.ToArray<User>();
            }
            
        }

        public void PrintRepository()
        {
            foreach (KeyValuePair<int, User> pair in concurrentUsers)
            {
                Console.WriteLine(pair.Value.ToString());
            }
        }
    }
}
