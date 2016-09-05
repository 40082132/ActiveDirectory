using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectory
{
    class EnumerationTasks
    {
        public ArrayList EnumerateOrganizationalUnit(string ouDomain)
        {
            ArrayList alObjects = new ArrayList();
            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + ouDomain);
                foreach (DirectoryEntry child in directoryObject.Children)
                {
                    string childPath = child.Path.ToString();
                    alObjects.Add(childPath.Remove(0, 7));
                    //remove the LDAP prefix from the path

                    child.Close();
                    child.Dispose();
                }
                directoryObject.Close();
                directoryObject.Dispose();
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine("An Error Occured: " + e.Message.ToString());
            }
            return alObjects;
        }
    }
}
