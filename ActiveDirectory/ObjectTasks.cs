using System;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace ActiveDirectory
{
    class ObjectTasks
    {
        public static bool Exists(string objectPath)
        {
            bool found = DirectoryEntry.Exists("LDAP://" + objectPath);

            return found;

        }

        public void Move(string objectLocation, string newLocation)
        {
            DirectoryEntry eLocation = new DirectoryEntry("LDAP://" + objectLocation);
            DirectoryEntry nLocation = new DirectoryEntry("LDAP://" + newLocation);
            string newName = eLocation.Name;
            eLocation.MoveTo(nLocation, newName);
            eLocation.Close();
            nLocation.Close();
        }
        public ArrayList AttributeValuesMultiString(string attributeName,
     string objectDn, ArrayList valuesCollection, bool recursive)
        {
            DirectoryEntry ent = new DirectoryEntry(objectDn);
            PropertyValueCollection ValueCollection = ent.Properties[attributeName];
            if (ValueCollection == null) throw new ArgumentNullException(nameof(ValueCollection));
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
        public void CreateShareEntry(string ldapPath,
    string shareName, string shareUncPath, string shareDescription)
        {
            string oGUID = string.Empty;
            string connectionPrefix = "LDAP://" + ldapPath;
            DirectoryEntry directoryObject = new DirectoryEntry(connectionPrefix);
            DirectoryEntry networkShare = directoryObject.Children.Add("CN=" +
                shareName, "volume");
            networkShare.Properties["uNCName"].Value = shareUncPath;
            networkShare.Properties["Description"].Value = shareDescription;
            networkShare.CommitChanges();

            directoryObject.Close();
            networkShare.Close();
        }
        public void CreateSecurityGroup(string ouPath, string name)
        {
            if (!DirectoryEntry.Exists("LDAP://CN=" + name + "," + ouPath))
            {
                try
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + ouPath);
                    DirectoryEntry group = entry.Children.Add("CN=" + name, "group");
                    group.Properties["sAmAccountName"].Value = name;
                    group.CommitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }
            else { Console.WriteLine(ouPath + " already exists"); }
        }
        public void DeleteSecurityGroup(string ouPath, string groupPath)
        {
            if (DirectoryEntry.Exists("LDAP://" + groupPath))
            {
                try
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + ouPath);
                    DirectoryEntry group = new DirectoryEntry("LDAP://" + groupPath);
                    entry.Children.Remove(group);
                    group.CommitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }
            else
            {
                Console.WriteLine(ouPath + " doesn't exist");
            }
        }

        public void CreateTrust(string sourceForestName, string targetForestName)
        {
            Forest sourceForest = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, sourceForestName));

            Forest targetForest = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, targetForestName));

            //create a forest trust

            sourceForest.CreateTrustRelationship(targetForest, TrustDirection.Outbound);
        }

        public void DeleteTrust(string sourceForestName, string targetForestName)
        {
            Forest sourceForest = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, sourceForestName));

            Forest targetForst = Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest, targetForestName));

            //delete trust

            sourceForest.DeleteTrustRelationship(targetForst);
        }





    }
}
