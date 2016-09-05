using System.Collections;
using System.DirectoryServices;
using System.Security.Principal;

namespace ActiveDirectory
{
    public class UserTasks
    {
        private string oGUID;

        private bool Authenticate(string userName, string password, string domain)
        {
            bool authentic = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain, userName, password);
                var nativeObject = entry.NativeObject;
                authentic = true;
            }
            catch (DirectoryServicesCOMException)
            {

            }
            return authentic;
        }

        public ArrayList AttributeValuesMultiString(string attributeName,
            string objectDn, ArrayList valuesCollection, bool recursive)
        {
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            PropertyValueCollection ValueCollection = ent.Properties[attributeName];
            IEnumerator en = ValueCollection.GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current != null)
                {
                    if (!valuesCollection.Contains(en.Current.ToString()))
                    {
                        valuesCollection.Add(en.Current.ToString());
                        if (recursive)
                        {
                            AttributeValuesMultiString(attributeName, "LDAP://" +
                                                                      en.Current.ToString(), valuesCollection, true);
                        }
                    }
                }
            }
            ent.Close();
            ent.Dispose();
            return valuesCollection;
        }


        public void AddToGroup(string userDn, string groupDn)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn);
                dirEntry.Properties["member"].Add(userDn);
                dirEntry.CommitChanges();
                dirEntry.Close();
            }
            catch (DirectoryServicesCOMException e)
            {
                //TODO something with exception
            }
        }

        public ArrayList GetGroups()
        {
            ArrayList groups = new ArrayList();
            if (System.Web.HttpContext.Current.Request.LogonUserIdentity != null)
                if (System.Web.HttpContext.Current.Request.LogonUserIdentity.Groups != null)
                    foreach (IdentityReference group in System.Web.HttpContext.Current.Request.LogonUserIdentity.Groups)
                    {
                        groups.Add(@group.Translate(typeof(NTAccount)).ToString());
                    }
            return groups;
        }

        public ArrayList GetGroups(string userDn, bool recursive)
        {
            ArrayList groupMemberships = new ArrayList();
            return AttributeValuesMultiString("memberOf", userDn, groupMemberships, recursive);
        }

        public string CreateUserAccount(string ldapPath, string username, string password)
        {
            try
            {
                
                var connectionPrefix = "LDAP://" + ldapPath;
                DirectoryEntry dirEntry = new DirectoryEntry(connectionPrefix);
                DirectoryEntry newUser = dirEntry.Children.Add("CN=" + username, "user");
                newUser.Properties["samAccountName"].Value = username;
                newUser.CommitChanges();
                oGUID = newUser.Guid.ToString();

                newUser.Invoke("SetPassword", new object[] {password});
                newUser.CommitChanges();

                dirEntry.Close();
                newUser.Close();
            }
            catch (DirectoryServicesCOMException e)
            {
                //TODO: Do something with these errors
            }
            return oGUID;
        }

        public void Enable(string userDN)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDN);
                int val = (int) user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val & ~0x2;

                user.CommitChanges();
                user.Close();
            }
            catch (DirectoryServicesCOMException e)
            {
                //TODO: Do something with error messages
            }
        }

        public void Disable(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDn);
                int val = (int) user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val | 0x2;

                user.CommitChanges();
                user.Close();
            }
            catch (DirectoryServicesCOMException e)
            {
                //TODO: Do something with error messages
            }


        }

        public void Unlock(string userDn)
        {
            try
            {
                DirectoryEntry uEntry = new DirectoryEntry(userDn);
                uEntry.Properties["LockOutTime"].Value = 0;

                uEntry.CommitChanges();
                uEntry.Close();
            }
            catch (DirectoryServicesCOMException e)
            {
                //TODO: Do something with error messages
            }
        }

        public void ResetPassword(string userDn, string password)
        {
            DirectoryEntry uEntry = new DirectoryEntry(userDn);
            uEntry.Invoke("SetPassword", new object[] {password});
            uEntry.Properties["LockOutTIme"].Value = 0;
            uEntry.Close();

        }

        public static void Rename(string objectDn, string newName)
        {
            DirectoryEntry child = new DirectoryEntry("LDAP://" + objectDn);
            child.Rename("CN=" + newName);
        }

    }
}

      
  
