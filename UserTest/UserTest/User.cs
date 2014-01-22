namespace UserTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class User
	{
		#region Constructors

		public User(int user_id, string family_name, string given_name)
		{
			UserID = user_id;
			FamilyName = family_name;
			GivenName = given_name;
		}

		#endregion Constructors

		#region Properties

		public string FamilyName
		{
			get; set;
		}

		public string GivenName
		{
			get; set;
		}

		public int UserID
		{
			get; set;
		}

		#endregion Properties

        public override string ToString()
        {
            return String.Format("{0}: {1}, {2}", UserID, FamilyName, GivenName);
        }

        //INFO:так как в моем классе User исп-ся только простые типы (value type and strings),
        // то использование следающх методов - необязательнов моем случае

        /**
         * The MemberwiseClone method creates a shallow copy by creating a new object, and then copying the nonstatic fields 
         * of the current object to the new object. If a field is a value type, a bit-by-bit copy of the field is performed. 
         * If a field is a reference type, the reference is copied but the referred object is not; therefore, the original object
         * and its clone refer to the same object.
         * 
         * For example, consider an object called X that references objects A and B. Object B, in turn, references object C. 
         * A shallow copy of X creates new object X2 that also references objects A and B. In contrast, a deep copy of X creates 
         * a new object X2 that references the new objects A2 and B2, which are copies of A and B. B2, in turn, references 
         * the new object C2, which is a copy of C. The example illustrates the difference between a shallow and a deep copy operation.
         * 
         * There are numerous ways to implement a deep copy operation if the shallow copy operation performed by 
         * the MemberwiseClone method does not meet your needs. These include the following:
         * 
         * - Call a class constructor of the object to be copied to create a second object with property values taken from the first object.
         * This assumes that the values of an object are entirely defined by its class constructor.
         * 
         * - Call the MemberwiseClone method to create a shallow copy of an object, and then assign new objects whose values are the same
         * as the original object to any properties or fields whose values are reference types. The DeepCopy method in the example
         * illustrates this approach.
         * 
         * - Serialize the object to be deep copied, and then restore the serialized data to a different object variable.
         * 
         * - Use reflection with recursion to perform the deep copy operation.
         * 
         * 
         * *********/

        public User ShallowCopy()
        {
            return (User)this.MemberwiseClone();
        }

        public User DeepCopy()
        {
            User other = (User)this.MemberwiseClone();

            //INFO: для строк это не работает (строки - это особые reference types)
            // но если бы в моем классе в кач-ыве одного из полей использовалсядругой класс,
            // - то необходимо было бы использовать конструкцию: Other.SomeField = new SubClass(this.someField)
            //other.FamilyName = new String(this.FamilyName);
            //other.GivenName = new string(this.GivenName);
            return other;
        }

	}

    /// <summary>
    /// by Kostya
    /// </summary>
    class User2 : IComparable<User2>
    {
        public int UserID { get; private set; }
        public string UserName { get; private set; }

        public User2(int userId, string userName)
        {
            this.UserID = userId;
            this.UserName = userName;
        }

        public int CompareTo(User other)
        {
            /*
             * Return values:
Less than zero
 The current instance precedes the object specified by the CompareTo method in the sort order.
 
Zero
 This current instance occurs in the same position in the sort order as the object specified by the CompareTo method.
 
Greater than zero
 This current instance follows the object specified by the CompareTo method in the sort order
 
****/
            return UserID - other.UserID;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", UserID, UserName);
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
                return false;
            if (null == (User2)obj)
                return false;
            User2 other = (User2)obj;
            return other.UserID == UserID
                && other.UserName.Equals(UserName);
        }

        public override int GetHashCode()
        {
            // INFO: пчм не подходит UserID.GetHashCode() + UserName.GetHashCode();?
            // кстати, по поводу GetHashCode()
            // имеет смысл написать так:
            // 3 * UserID.GetHashCode() + 5 * UserName.GetHashCode();
            // чтобы распределение было более равномерным...
            // но для данной конкретной задачи - роли не играет, можно вообще написать :
            // return 0;

            // а почему именно такие коэффициенты? 3 и 5

            // ну.. типа простые числа
            // можно 2 и 3
            // сам класс User - mutable
            // т.е. можно получить ссылку на инстанс класса, изменить его и изменится
            // состояние репозитория
            return UserID.GetHashCode() + UserName.GetHashCode();
        }

        int IComparable<User2>.CompareTo(User2 other)
        {
            if(other == null)
                return -1;
            return other.UserID.CompareTo(this.UserID);
        }
    }
}