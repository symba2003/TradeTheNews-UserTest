namespace UserTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

    /// <summary>
    /// Вариант, который я отправила на собеседовании
    /// </summary>
    public class UserRepository
	{
        private Object m_Lock = new object();

        // The SortedDictionary(TKey, TValue) generic class is a binary search tree with O(log n) retrieval, where N is the number of elements in the dictionary.
        private SortedDictionary<int, User> users = new SortedDictionary<int, User>();
       
    
        #region Methods

		public void AddUser(User user)
		{
            lock (m_Lock)
            {
                users.Add(user.UserID, user);
            }
		}

		public User[] GetOrderedUsers()
		{
            // error here: concurrent modification
           return  users.Values.ToArray<User>();
		}

		public User GetUser(int user_id)
		{
            //INFO: может здесь копию User возвращать?
            // чтобы не было ситуации:

            // сам класс User - mutable
            // т.е. можно получить ссылку на инстанс класса, изменить его и изменится
            // состояние репозитория

            //так что я бы проектировал репозиторий так, чтобы он сохранял копии юзеров и возвращал копии юзеров
            // т.е. клонов, как бы

            // и если один из польз-лей изменил, например, свое имя - разве это плохо, что и в репозитории эти изменения отразятся7об этом условие задачи умалчивает.
            //в моем решении, я сделал свойства - get; private set;
            //объект User получился Immutable
            //я могу спокойно их возвращать и не парится о неконсистентности, которую могут внести пользовательские потоки.

            //теперь, если объект mutable, то возвращая его ты:
            //1) меняешь состояние репозитория
            //2) два разных потока получив ссылку на один и тот же объект могут по-разному его менять и думать, что все ок
            //например, два потока получили доступ к одному инстансу пользователя
            //1) один поток меняет имя на Иван
            //2) второй поток меняет имя на Ольга
            //3) второй поток меняет имя на Иванова
            //4) первый поток меняет имя на Петров
            //в итоге, второй поток остался без изменений
            //а может получиться Иван Иванова
            //и т.п.

            // когда ты работаешь в многопоточной среде надо стараться делать объекты immutable
            // если объект mutable, то все изменения состояний должны быть защищены локами, т.е. должны быть "атомарны"

            // так вот, у меня для такой фигни предусмотрен метод - updateUser()
            // а сам класс User - immutable
            User user = null;
            if(users.ContainsKey(user_id))
            { // error here: race condition
                lock (m_Lock)
                {
                    user = users[user_id];
                }
            }

            // INFO: 
            /*
            if (sort.TryGetValue("programmer", out v))
            {
                Console.WriteLine(v);
            }
             * we use the TryGetValue method on the SortedDictionary, which is excellent for avoiding another key lookup. 
             * If the key exists, TryGetValue returns true and it fills the out parameter.
             * */
            return user;


		}

		#endregion Methods
	}
}