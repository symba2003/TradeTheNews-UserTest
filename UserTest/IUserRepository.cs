using System;
namespace UserTest
{
    interface IUserRepository
    {
        void AddUser(User user);
        User[] GetOrderedUsers();
        User GetUser(int userId);
        void PrintRepository();
    }
}
