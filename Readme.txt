Provide implementation of the "UserRepository" class methods:

AddUser -- accepts a user instance as a parameter and adds it to the repository.
GetUser -- accepts a user_id as a parameter and returns the corresponding user from the repository (UserID == user_id).
GetOrderedUsers -- returns arrays of users ordered by UserID.

UserRepository must keep all users in memory.

Implementation of the UserRepository class must be thread-safe. 
Read operations (GetUser and GetOrderedUsers) will be called very often from many threads simultaneously.
 Write operation (AddUser) will be called much less often, but also can be called from several threads
 at the same time.

It is important that the GetUser and GetOrderedUsers will execute as quickly as possible.
 Memory consumption is not a concern.