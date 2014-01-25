using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionsTest
{
    class User : IComparable
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
            get;
            set;
        }

        public string GivenName
        {
            get;
            set;
        }

        public int UserID
        {
            get;
            set;
        }

        #endregion Properties

        public override string ToString()
        {
            return String.Format("{0}: {1}, {2}", UserID, FamilyName, GivenName);
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is User))
                return -1;

            User otherUser = (User)obj;
            //if (this.UserID == otherUser.UserID && this.FamilyName.Equals(otherUser.FamilyName) && this.GivenName.Equals(otherUser.GivenName))
            //    return 0;
            //else if (this.)
            return this.UserID - otherUser.UserID;
        }

        #endregion
    }
}
