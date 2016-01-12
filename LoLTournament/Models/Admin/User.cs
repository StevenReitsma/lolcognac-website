using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.Helpers;
using System.Web.Security;
using LoLTournament.Helpers;
using MongoDB.Bson;

namespace LoLTournament.Models.Admin
{
    [Serializable]
    public class User
    {
        /// <summary>
        /// Gets the logon name of the membership user.
        /// </summary>
        /// 
        /// <returns>
        /// The logon name of the membership user.
        /// </returns>
        public string UserName { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Gets the user identifier from the membership data source for the user.
        /// </summary>
        /// 
        /// <returns>
        /// The user identifier from the membership data source for the user.
        /// </returns>
        public ObjectId Id { get; set; }

        /// <summary>
        /// Gets or sets the e-mail address for the membership user.
        /// </summary>
        /// 
        /// <returns>
        /// The e-mail address for the membership user.
        /// </returns>
        public string Email { get; set; }

        /// <summary>
        /// Gets the password question for the membership user.
        /// </summary>
        /// 
        /// <returns>
        /// The password question for the membership user.
        /// </returns>
        public string PasswordQuestion { get; set; }

        /// <summary>
        /// Gets or sets application-specific information for the membership user.
        /// </summary>
        /// 
        /// <returns>
        /// Application-specific information for the membership user.
        /// </returns>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets whether the membership user can be authenticated.
        /// </summary>
        /// 
        /// <returns>
        /// true if the user can be authenticated; otherwise, false.
        /// </returns>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets a value indicating whether the membership user is locked out and unable to be validated.
        /// </summary>
        /// 
        /// <returns>
        /// true if the membership user is locked out and unable to be validated; otherwise, false.
        /// </returns>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets the most recent date and time that the membership user was locked out.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.DateTime"/> object that represents the most recent date and time that the membership user was locked out.
        /// </returns>
        public DateTime LastLockoutDate { get; set; }

        /// <summary>
        /// Gets the date and time when the user was added to the membership data store.
        /// </summary>
        /// 
        /// <returns>
        /// The date and time when the user was added to the membership data store.
        /// </returns>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last authenticated.
        /// </summary>
        /// 
        /// <returns>
        /// The date and time when the user was last authenticated.
        /// </returns>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the membership user was last authenticated or accessed the application.
        /// </summary>
        /// 
        /// <returns>
        /// The date and time when the membership user was last authenticated or accessed the application.
        /// </returns>
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        /// Gets the date and time when the membership user's password was last updated.
        /// </summary>
        /// 
        /// <returns>
        /// The date and time when the membership user's password was last updated.
        /// </returns>
        public DateTime LastPasswordChangedDate { get; set; }

        public List<string> Roles { get; set; }

        /// <summary>
        /// Gets whether the user is currently online.
        /// </summary>
        /// 
        /// <returns>
        /// true if the user is online; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual bool IsOnline
        {
            get
            {
                return LastActivityDate.ToUniversalTime() > DateTime.UtcNow.Subtract(new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0));
            }
        }

        /// <summary>
        /// Creates a new membership user object with the specified property values.
        /// </summary>
        /// <param name="name">The <see cref="P:System.Web.Security.MembershipUser.UserName"/> string for the membership user.</param>
        /// <param name="password"> </param>
        /// <param name="email">The <see cref="P:System.Web.Security.MembershipUser.Email"/> string for the membership user.</param>
        /// <param name="passwordQuestion">The <see cref="P:System.Web.Security.MembershipUser.PasswordQuestion"/> string for the membership user.</param>
        /// <param name="comment">The <see cref="P:System.Web.Security.MembershipUser.Comment"/> string for the membership user.</param>
        /// <param name="isApproved">The <see cref="P:System.Web.Security.MembershipUser.IsApproved"/> value for the membership user.</param>
        /// <param name="isLockedOut">true to lock out the membership user; otherwise, false.</param><param name="creationDate">The <see cref="P:System.Web.Security.MembershipUser.CreationDate"/>
        /// <see cref="T:System.DateTime"/> object for the membership user.</param><param name="lastLoginDate">The <see cref="P:System.Web.Security.MembershipUser.LastLoginDate"/><see cref="T:System.DateTime"/> object for the membership user.</param>
        /// <param name="lastActivityDate">The <see cref="P:System.Web.Security.MembershipUser.LastActivityDate"/><see cref="T:System.DateTime"/> object for the membership user.</param>
        /// <param name="lastPasswordChangedDate">The <see cref="P:System.Web.Security.MembershipUser.LastPasswordChangedDate"/><see cref="T:System.DateTime"/> object for the membership user.</param>
        /// <param name="lastLockoutDate">The <see cref="P:System.Web.Security.MembershipUser.LastLockoutDate"/><see cref="T:System.DateTime"/> object for the membership user.</param>
        /// <param name="roles"> </param>
        public User(string name, string password, string email, string passwordQuestion, string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate, List<string> roles)
        {
            if (name != null)
                name = name.Trim();
            if (email != null)
                email = email.Trim();
            if (passwordQuestion != null)
                passwordQuestion = passwordQuestion.Trim();
            UserName = name;
            Password = Crypto.HashPassword(password);
            Id = ObjectId.GenerateNewId();
            Email = email;
            PasswordQuestion = passwordQuestion;
            Comment = comment;
            IsApproved = isApproved;
            IsLockedOut = isLockedOut;
            CreationDate = creationDate.ToUniversalTime();
            LastLoginDate = lastLoginDate.ToUniversalTime();
            LastActivityDate = lastActivityDate.ToUniversalTime();
            LastPasswordChangedDate = lastPasswordChangedDate.ToUniversalTime();
            LastLockoutDate = lastLockoutDate.ToUniversalTime();
            Roles = roles;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="T:System.Web.Security.MembershipUser"/> object for a class that inherits the <see cref="T:System.Web.Security.MembershipUser"/> class.
        /// </summary>
        protected User()
        {
        }

        /// <summary>
        /// Returns the user name for the membership user.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="P:System.Web.Security.MembershipUser.UserName"/> for the membership user.
        /// </returns>
        public override string ToString()
        {
            return UserName;
        }

        internal virtual void Update()
        {
            UpdateSelf();
            Mongo.Users.Save(this);
        }

        /// <summary>
        /// Gets the password for the membership user from the membership data store.
        /// </summary>
        /// 
        /// <returns>
        /// The password for the membership user.
        /// </returns>
        /// <exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual string GetPassword()
        {
            return Membership.Provider.GetPassword(UserName, null);
        }

        /// <summary>
        /// Gets the password for the membership user from the membership data store.
        /// </summary>
        /// 
        /// <returns>
        /// The password for the membership user.
        /// </returns>
        /// <param name="passwordAnswer">The password answer for the membership user.</param><exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual string GetPassword(string passwordAnswer)
        {
            return Membership.Provider.GetPassword(UserName, passwordAnswer);
        }

        internal string GetPassword(bool throwOnError)
        {
            return GetPassword(null, false, throwOnError);
        }

        internal string GetPassword(string answer, bool throwOnError)
        {
            return GetPassword(answer, true, throwOnError);
        }

        private string GetPassword(string answer, bool useAnswer, bool throwOnError)
        {
            string str = null;
            try
            {
                str = !useAnswer ? GetPassword() : GetPassword(answer);
            }
            catch (ArgumentException)
            {
                if (throwOnError)
                    throw;
            }
            catch (MembershipPasswordException)
            {
                if (throwOnError)
                    throw;
            }
            catch (ProviderException)
            {
                if (throwOnError)
                    throw;
            }
            return str;
        }

        /// <summary>
        /// Updates the password for the membership user in the membership data store.
        /// </summary>
        /// 
        /// <returns>
        /// true if the update was successful; otherwise, false.
        /// </returns>
        /// <param name="oldPassword">The current password for the membership user.</param>
        /// <param name="newPassword">The new password for the membership user.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="oldPassword"/> is an empty string.-or-<paramref name="newPassword"/> is an empty string.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="oldPassword"/> is null.-or-<paramref name="newPassword"/> is null.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual bool ChangePassword(string oldPassword, string newPassword)
        {
            if (!Membership.Provider.ChangePassword(UserName, oldPassword, newPassword))
                return false;
            UpdateSelf();
            return true;
        }

        internal bool ChangePassword(string oldPassword, string newPassword, bool throwOnError)
        {
            bool flag = false;
            try
            {
                flag = ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                if (throwOnError)
                    throw;
            }
            catch (MembershipPasswordException)
            {
                if (throwOnError)
                    throw;
            }
            catch (ProviderException)
            {
                if (throwOnError)
                    throw;
            }
            return flag;
        }

        /// <summary>
        /// Updates the password question and answer for the membership user in the membership data store.
        /// </summary>
        /// 
        /// <returns>
        /// true if the update was successful; otherwise, false.
        /// </returns>
        /// <param name="password">The current password for the membership user.</param>
        /// <param name="newPasswordQuestion">The new password question value for the membership user.</param>
        /// <param name="newPasswordAnswer">The new password answer value for the membership user.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="password"/> is an empty string.-or-<paramref name="newPasswordQuestion"/> is an empty string.-or-<paramref name="newPasswordAnswer"/> is an empty string.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="password"/> is null.</exception><exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual bool ChangePasswordQuestionAndAnswer(string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!Membership.Provider.ChangePasswordQuestionAndAnswer(UserName, password, newPasswordQuestion, newPasswordAnswer))
                return false;
            UpdateSelf();
            return true;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// 
        /// <returns>
        /// The new password for the membership user.
        /// </returns>
        /// <param name="passwordAnswer">The password answer for the membership user.</param>
        /// <exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual string ResetPassword(string passwordAnswer)
        {
            string str = Membership.Provider.ResetPassword(UserName, passwordAnswer);
            if (!string.IsNullOrEmpty(str))
                UpdateSelf();
            return str;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// 
        /// <returns>
        /// The new password for the membership user.
        /// </returns>
        /// <exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual string ResetPassword()
        {
            return ResetPassword(null);
        }

        internal string ResetPassword(bool throwOnError)
        {
            return ResetPassword(null, false, throwOnError);
        }

        internal string ResetPassword(string passwordAnswer, bool throwOnError)
        {
            return ResetPassword(passwordAnswer, true, throwOnError);
        }

        private string ResetPassword(string passwordAnswer, bool useAnswer, bool throwOnError)
        {
            string str = null;
            try
            {
                str = !useAnswer ? ResetPassword() : ResetPassword(passwordAnswer);
            }
            catch (ArgumentException)
            {
                if (throwOnError)
                    throw;
            }
            catch (MembershipPasswordException)
            {
                if (throwOnError)
                    throw;
            }
            catch (ProviderException)
            {
                if (throwOnError)
                    throw;
            }
            return str;
        }

        /// <summary>
        /// Clears the locked-out state of the user so that the membership user can be validated.
        /// </summary>
        /// 
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.PlatformNotSupportedException">This method is not available. This can occur if the application targets the .NET Framework 4 Client Profile. To prevent this exception, override the method, or change the application to target the full version of the .NET Framework.</exception>
        public virtual bool UnlockUser()
        {
            if (!Membership.Provider.UnlockUser(UserName))
                return false;
            UpdateSelf();
            return !IsLockedOut;
        }

        private void UpdateSelf()
        {
            var user = Membership.GetUser(UserName, false);
            if (user == null)
                return;
            try
            {
                LastPasswordChangedDate = user.LastPasswordChangedDate.ToUniversalTime();
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                LastActivityDate = user.LastActivityDate;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                LastLoginDate = user.LastLoginDate;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                CreationDate = user.CreationDate.ToUniversalTime();
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                LastLockoutDate = user.LastLockoutDate.ToUniversalTime();
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                IsLockedOut = user.IsLockedOut;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                IsApproved = user.IsApproved;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                Comment = user.Comment;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                PasswordQuestion = user.PasswordQuestion;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                Email = user.Email;
            }
            catch (NotSupportedException)
            {
            }
            try
            {
                Id = (ObjectId)user.ProviderUserKey;
            }
            catch (NotSupportedException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }

        public MembershipUser ToMembershipUser()
        {
            return new MembershipUser("MongoProvider", UserName, Id, Email, PasswordQuestion, Comment,
                                      IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate,
                                      LastPasswordChangedDate, LastLockoutDate);
        }
    }
}
