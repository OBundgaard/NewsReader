using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NewsReader
{
    public static class SaveSystem
    {

        public static void SaveUserData(string email, string password, string server)
        {
            // We create a new formatter
            BinaryFormatter formatter = new BinaryFormatter();

            // We want to store our data in the project folder
            string path = Environment.CurrentDirectory + "/data.nsld";

            // We create a new file stream
            FileStream stream = new FileStream(path, FileMode.Create);

            // Now we store our data into one object
            UserData data = new UserData(email, Encrypt(password), server);

            // We serialize it
            formatter.Serialize(stream, data);

            // Then we close it
            stream.Close();
        }

        public static UserData LoadUserData()
        {
            // We want to get our data from the project folder
            string path = Environment.CurrentDirectory + "/data.nsld";

            if (File.Exists(path))
            {
                // We create a new formatter
                BinaryFormatter formatter = new BinaryFormatter();

                // We create a new file stream
                FileStream stream = new FileStream(path, FileMode.Open);

                // We deserialize it as user data
                UserData data = formatter.Deserialize(stream) as UserData;
                data.Password = Decrypt(data.Password);

                // Then we close it
                stream.Close();

                return data;
            } else
            {
                return null;
            }
        }

        private static string Encrypt(string password)
        {
            // We convert the password to bytes
            byte[] bytes = Encoding.Default.GetBytes(password);

            // Then we convert it to a string and removes "-"
            // This gives us a HEX string
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        private static string Decrypt(string encryptedPassword)
        {
            // We create a new byte array
            byte[] data = new byte[encryptedPassword.Length / 2];

            // We loop through the byte array
            for (int i = 0; i < data.Length; i++)
            {
                // We then convert substrings of the encrypted password to bytes
                data[i] = Convert.ToByte(encryptedPassword.Substring(i * 2, 2), 16);
            }

            // Now we can now convert it to a string and then return the original password
            return Encoding.ASCII.GetString(data);
        }

    }

    [System.Serializable]
    public class UserData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }

        public UserData(string parsedEmail, string parsedPassword, string parsedServer)
        {
            this.Email = parsedEmail;
            this.Password = parsedPassword;
            this.Server = parsedServer;
        }
    }
}
