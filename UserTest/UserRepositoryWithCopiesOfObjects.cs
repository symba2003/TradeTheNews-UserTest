using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserTest
{
    /// <summary>
    /// Класс, содержащий копии оригинальных объектов User и возвращающий копии User
    /// </summary>
    /// <remarks> Совместно с Костей решили - что это уже overkill для текущей задачи</remarks>
    class UserRepositoryWithCopiesOfObjects
    {
        // Since requirements state that: 
        // It is important that the GetUser and GetOrderedUsers will execute as quickly as possible.
        // Memory consumption is not a concern.

        // Will store 2 collections: optimized for concurent access and sorted
        // QUESTION: если по условию задачи UserKey является Int, и для int хэш-функция возвращает этот же int,
        // то стоит ли "заморачиваться" с хранением обеих коллекций? ConcurrentDictionary & SortedSet?
        // ведь ConcurrentDictionary оказывается сортированным?

        // ANSWER: а что станет когда юзеров будет больше, чем Int? (не представляю себе такого сервиса, но все же ))
        // если нужен будет long?
        // это доп вопрос задаче-дателю (в коментариях к заданию, сделала так и так, потому что считаю, что так и так, в другой ситуации - по другому.)

        /*Костя: а зачем ты добавляешь и возвращаешь все время копии юзеров, это не overkill?
        [13:17:17] Екатерина Ряховская: аа..про копии мы с тобой отдельно обсуждали, понмишь? 
         *          что если изменить объект в к-л потоке, то изменится и репозиторий...и вроде как это не есть гуд
        [13:17:29] Костя Ряховский (актуальн): да, я помню...
        [13:19:00] Екатерина Ряховская: в общем-то для той конкретной задачи - возвращать копии userов - 
         *          Не является необходимостью?
        [13:19:56] Костя Ряховский (актуальн): ну да, это тоже обсуждали. 
                                мы тогда решили , что зависит от задачи 
                                все ок
                                ну только в коментариях к задаче ты опишешь, что ты делаешь так, потому что ....
        [13:20:04] Костя Ряховский (актуальн): и как это скажется на производительности
        [13:20:52] Екатерина Ряховская: насчет производительности - имеются в виду примерные оценки? 
         *              или результаты конкретных тестов?
        [13:21:11] Екатерина Ряховская: (тест длился два часа - и это только одно из 4 заданий)
        [13:21:32] Костя Ряховский (актуальн): зависит от того, что от тебя требуется )
                    в той конкретной ситуации - достаточно примерной оценки, имхо
                    а микробенчмарки - это отдельная наука
        [13:22:38] Костя Ряховский (актуальн): я имел ввиду, отметить, что ты понимаешь, что в lock делаются копии
         * и это не гуд для производительности когда много потоков пытаются получить доступ к критическому участку
        [13:22:49] Костя Ряховский (актуальн): чтоб они понимали, что ты понимаешь..
        **/
        private ConcurrentDictionary<int, User> concurentUsers = new ConcurrentDictionary<int, User>();

        private SortedDictionary<int, User> sortedUsers = new SortedDictionary<int, User>();

        private Object synchronizer = new object();

        #region Methods
        public void AddUser(User user)
        {
            // Write operation (AddUser) will be called much less often, but also can be called from several threads at the same time.
            lock (synchronizer)
            {
                // операция дб атомарна: добавление пользователя в обе коллекции
                if (!sortedUsers.ContainsKey(user.UserID))
                {
                    // сначала проверяем наличие пользователя в коллекции, 
                    // и только если пользователь еще не добавлен в коллекцию,
                    // только после этого создаем копию этого пользователя

                    // create a copy of the user
                    User u = new User(user.UserID, user.FamilyName, user.GivenName);

                    concurentUsers.TryAdd(user.UserID, user);
                    sortedUsers.Add(user.UserID, user);
                }
            }
        }

        /// <summary>
        /// returns arrays of users ordered by UserID.
        /// </summary>
        /// <returns></returns>
        public User[] GetOrderedUsers()
        {
            
            lock (synchronizer)
            {
                User[] users = new User[sortedUsers.Count];
                // DONE: is it ordered by UserID? answer: yes
                // INFO: ConcurentDictionary.ToArray() method returns a static snapshot of the dictionary.
                //       That is, the dictionary is locked, and then copied to an array as a O(n) operation. 

                //INFO: The returned SortedDictionary<TKey, TValue>.ValueCollection is not a static copy; instead, the 
                // SortedDictionary<TKey, TValue>.ValueCollection refers back to the values in the original SortedDictionary<TKey, TValue>.
                // Therefore, changes to the SortedDictionary<TKey, TValue> continue to be reflected in the 
                // SortedDictionary<TKey, TValue>.ValueCollection.  Getting the value of this property is an O(1) operation.

                // здесь возвращаются те же самые объекты User (Некопии)
                //sortedUsers.Values.CopyTo(users, 0);
                //users = sortedUsers.Values.ToArray();
                int i = 0;
                foreach (User u in sortedUsers.Values) 
                {
                    users[i] = u.ShallowCopy();
                    i++;
                }

                    //sortedUsers.ToArray().
                    //concurentUsers.ToArray().

                    //QUESTION: это норм, что Return в Lock'e?
                    // ANSWER: все ок
                    // lock {} - это почти try { Monitor.Enter(); } finally { Monitor.Exit(); }
                    return users;
            }
        }

        public User GetUser(int user_id)
        {
            // QUESTION: does the lock needed? (for creating shallow copy of concurentUsers[user_id])
            //no lock concurentUsers[user_id], since it's ConcurentCollection

            // ANSWER: не знаю, как устроен ConcurrentDictionary в C#, если он не бросит ConcurrentModificationException
            // - то все ок.
            // скорее всего он вернет итератор валидный на определенный момент времени.
            // соответственно, в PrintRepository - то же самое.

            // COMMENTS:
            /**
             * [13:32:29] Екатерина Ряховская:  ConcurentDictionary (который в C#) - Thread-safe...
             *              соответственно операция concurentUsers[user_id] - thread-safe..
             *              а вот вызов ShallowCopy() - уже не thread-safe (это метод объекта User)..
                [13:33:05] Екатерина Ряховская: но с др стороны ShallowCopy - не меняет текуцщий объект User..
             *              а лишь "читает" его и создает копию
                
                [13:33:56] Костя Ряховский (актуальн): ну да, т.е. в худшем случае можно получить неконсистентный объект 
             *                      (наполовину записанный)
                                    но у тебя хранится копия - так что все ок
                [13:34:20] Костя Ряховский (актуальн): вообще, для этих целей User надо делать immutable )
            [13:35:04] Екатерина Ряховская: ок..задача не стояла в изменении User..и это можно было добавить в комментариях...
            [13:35:13] Костя Ряховский (актуальн): ага
            [13:36:40] Екатерина Ряховская: immutable- это значит, что объекты должны быть "неизменяемы"?...
             *              то есть read-only, верно?
            [13:36:58] Костя Ряховский (актуальн): да
             * ****/
            return concurentUsers[user_id].ShallowCopy();
        }

        #endregion Methods

        public void PrintRepository()
        {
            foreach (KeyValuePair<int, User> pair in concurentUsers)
            {
                Console.WriteLine(pair.Value.ToString());
            }
        }
    }
}
