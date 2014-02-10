using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UserTest
{
    /*
     Use ReaderWriterLockSlim to protect a resource that is read by multiple threads and written to by one
     * thread at a time. ReaderWriterLockSlim allows multiple threads to be in read mode, allows one thread
     * to be in write mode with exclusive ownership of the lock, and allows one thread that has read access
     * to be in upgradeable read mode, from which the thread can upgrade to write mode without having to
     * relinquish its read access to the resource.
     * 
     * ReaderWriterLockSlim is similar to ReaderWriterLock, but it has simplified rules for recursion and
     * for upgrading and downgrading lock state. ReaderWriterLockSlim avoids many cases of potential deadlock.
     * In addition, the performance of ReaderWriterLockSlim is significantly better than ReaderWriterLock.
     * ReaderWriterLockSlim is recommended for all new development.
     */
    /// <summary>
    /// Еще один класс-результат, использующий ReaderWriterLockSlim 
    /// </summary>
    /// <remarks> Предполагаем, что объекты User - immutable (неизменяемы, read-only)</remarks>
    class UserRepositoryWithReaderWriterLock : UserTest.IUserRepository
    {
        private ReaderWriterLockSlim sortedUsersLock = new ReaderWriterLockSlim();

        // коллекция для более быстрого доступа к объектам
        private ConcurrentDictionary<int, User> concurrentUsers = new ConcurrentDictionary<int, User>();

        //коллекция, хранящая сортированный массив объектов
        private SortedDictionary<int, User> sortedUsers = new SortedDictionary<int, User>();

       // private Object synchronizer = new Object();

        public User GetUser(int userId)
        {
            // access to concurrentUsers is thread-safe, so not using lock
            User user = null;
            concurrentUsers.TryGetValue(userId, out user);
            return user;
        }

        public void AddUser(User user)
        {
            sortedUsersLock.EnterWriteLock();

            try
            {   // COMMENT: а если элемент уже есть в коллекции concurrentUsers?, то я зря выполняла EnterWriteLock?
                // answer: на это можно забить
                // мне надо чтоб операция была атомарна, так что все ок
                if (concurrentUsers.TryAdd(user.UserID, user))
                    sortedUsers.Add(user.UserID, user);
            }
            finally 
            {
                sortedUsersLock.ExitWriteLock();
            }
        }

        public User[] GetOrderedUsers()
        {
            sortedUsersLock.EnterReadLock();
            try
            {
                return sortedUsers.Values.ToArray<User>();
            }
            finally
            {
                sortedUsersLock.ExitReadLock();
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
