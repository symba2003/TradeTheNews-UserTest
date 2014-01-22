using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace UserTest
{
    // by Kostya
    class UserRepository2
    {
        // главное помни, что thread-safe еще означает, что ты не будешь выставлять ссылки в public-интерфейс
        /*
         т.е. у тебя класс должен быть аля-immutable
         т.е. GetAllUsers не должен возвращать всю Map, а ее копию, либо только ее значения
         All public and protected members of ConcurrentDictionary<TKey, TValue> are thread-safe and may be used concurrently
         * from multiple threads.
         * 
         * 
         * Java:
         * т.е. надо помнить о visibility, reordering, happend-before
             т.е. visibility - это проблема, когда один поток пишет значения в переменные, которые 2му потоку видны не сразу,
         * а только после выполнения инструкций с happend-before

         * reordering - изменение порядка инструкций в одном потоке.
            т.е. для однопоточного приложения reordering ничего плохого не представляет, а в многопоточном - может быть опасен.
            т.е. здесь опять нужно выполнять все синхронизации между потоками только с учетом happened-before
             happend-before в java - это:
            захват всегда hp освобождение лока, 
            запись volitale поля всегда hp его чтения, 
            ну и еще несколько правил
             т.е. наверняка что-то подобное есть и в Net
         * 
         * ты конечно можешь написать свой класс, ReadOnlySortedSet<T> : SortedSet<T>
и переопределить все методы, которые модифицируют сет (запретить модификацию)
сделать приватное поле - SortedSet<T>
и в конструкторе присвоить ему свой set

и тогда можно будет быстро возвращать новый ReadOnlySortedSet
наверно это будет правильнее, чем создавать новый list
         * 
         * я: решение с хранением двух коллекций -   SortedSet<User> & ConcurrentDictionary<int, User> - наверное 
         * более корректное 
         * - чем я реализовала...
         * так как требований к "экономии памяти" у нас в задаче нет..а есть требования - более быстрого исполнения 
         * процедур GetUser 
         * and GetOrderedUsers
         * 
         * требование:
                быстрые и конкурентные get, конкурентный add, память - вторична
         */

        private ConcurrentDictionary<int, User> map = new ConcurrentDictionary<int, User>();
        //INFO: is it ordered by UserID? Answer: для User реализуем интерфейс IComparable, в котором сравниваем по UserID
        // тогда и SortedSet будет сортировать по UserID
        private SortedSet<User> set = new SortedSet<User>();
        private Object synchronizer = new Object();

        public UserRepository2()
        {

        }

        public User GetUser(int userId)
        {
            User user = null;
            bool exists = map.TryGetValue(userId, out user);
            return user;
        }

        public void AddUser(User user)
        {
            lock (synchronizer)
            {
                User old = map.GetOrAdd(user.UserID, user);
                bool added = set.Add(user);
            }

            //lock (synchronizer)
            //{
            //    bool added = map.TryAdd(user.UserID, user);
            //    if (!added)
            //        return false;
            //    return set.Add(user);
            //}
        }

         public bool UpdateUser(User user)
        {
            lock (synchronizer)
            {
                if (!map.ContainsKey(user.UserID))
                    return false;
                User old = null;
                bool got = map.TryGetValue(user.UserID, out old);
                if (!got)
                    return false;
                bool updated = map.TryUpdate(user.UserID, user, old);
                if (!updated)
                    return false;
                set.Remove(old);
                return set.Add(user);
            }
        }

        public ICollection<User> GetOrderedUsers()
        {
            // тут идея в том, чтобы отдать UnmodifiableCollection
            //DONE: здесь нет Lock'а? должен быть (SortedSet - не thread-safe, в момент итерации по коллекции она может измениться)

            // INFO: еще тебе надо убедиться, что такой вызов в одном потоке:
            // не вызовет исключения, когда в другом потоке добавляется элемент в коллекцию.

            // answer: если нет Lock'а, то вызовет! и будут возвращены не копии объектов, а реальные объекты

            return new List<User>(set);
        }

        
    }
}
