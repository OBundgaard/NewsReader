using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewsReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String serverName;    // The IP address
        int serverPort = 119; // The port

        // Convert an input String to bytes 
        byte[] sendMessage = Encoding.UTF8.GetBytes("Hello Server, how are you today?\n");

        TcpClient socket = null;
        NetworkStream ns = null;
        StreamReader reader = null; // Convinience

        StreamReader sr;
        StreamWriter sw;

        string response;

        public MainWindow()
        {
            InitializeComponent();

            // Load the user data if any
            UserData data = SaveSystem.LoadUserData();

            if (data != null)
            {
                // We had data so set it into the respective textboxes
                this.username.Text = data.Email;
                this.password.Password = data.Password;
                this.server.Text = data.Server;
            }
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.username.Text == "" || this.password.Password == "" || this.server.Text == "")
            {
                MessageBox.Show("ERROR: No email, password or server inputted!");
            }

            // Saving the data
            SaveSystem.SaveUserData(this.username.Text, this.password.Password, this.server.Text);

            // Inserting values
            this.serverName = this.server.Text;

            // Deactivating whilst connecting
            this.loginBtn.IsEnabled = false;
            this.username.IsEnabled = false;
            this.password.IsEnabled = false;
            this.server.IsEnabled = false;

            // Connect to the server
            bool connectionSuccessful = ConnectToServer();

            if (!connectionSuccessful)
            {
                // Setting the connection status
                this.connectionStatus.Content = "Not Connected";
                this.connectionStatus.Foreground = new SolidColorBrush(Colors.Red);

                // Reactivating on fail
                this.loginBtn.IsEnabled = true;
                this.username.IsEnabled = true;
                this.password.IsEnabled = true;
                this.server.IsEnabled = true;

                // Display error message
                MessageBox.Show("ERROR: Can't connect to server");

                return;
            }

            // Login to the server
            bool loginSuccessful = LoginToServer(this.username.Text, this.password.Password);

            if (!loginSuccessful)
            {
                // Setting the connection status
                this.connectionStatus.Content = "Not Connected";
                this.connectionStatus.Foreground = new SolidColorBrush(Colors.Red);

                // Reactivating on fail
                this.loginBtn.IsEnabled = true;
                this.username.IsEnabled = true;
                this.password.IsEnabled = true;
                this.server.IsEnabled = true;

                // Display error message
                MessageBox.Show("ERROR: Incorrect username or password");

                return;
            }

            // Connect and Login succeeded
            // Setting the connection status
            this.connectionStatus.Content = "Connected";
            this.connectionStatus.Foreground = new SolidColorBrush(Colors.LightGreen);

            // Add news groups to the list
            AddGroupsToList();

            return;
        }

        private void groupSelected(object sender, RoutedEventArgs e)
        {
            this.groupName.Text = this.newsgroups.SelectedItem.ToString();
            this.groupArticles.Items.Clear();

            this.sw.WriteLine($"group {this.groupName.Text}");
            this.response = this.sr.ReadLine();
            string[] parts = this.response.Split(' ');

            // Get the article count and put it into the textbox
            string articleCount = parts[1];
            this.groupArticleCount.Content = $"{articleCount} Articles in group";

            // Get articles in that group
            GetGroupArticles(parts[2], parts[3]);

            return;
        }

        private void articleSelected(object sender, RoutedEventArgs e)
        {
            
            if (this.groupArticles.SelectedItem != null)
            {
                // We get the article ID
                string articleID = this.groupArticles.SelectedItem.ToString();

                // We execute the command
                this.sw.WriteLine($"body {articleID}");

                // Article Content
                string articleContent = "";

                // Get responses
                for (int i = 0; i != 1000; i++)
                {
                    this.response = this.sr.ReadLine();

                    if (i > 1)
                    {
                        articleContent += this.response + "\n";
                    }

                    if (this.response == ".")
                    {
                        break;
                    }
                }

                this.selectedArticle.Text = articleContent;
            }

        }

        private bool LoginToServer(string username, string password)
        {
            try
            {
                this.sr = new StreamReader(this.ns, System.Text.Encoding.Default);
                this.sw = new StreamWriter(this.ns);
                this.sw.AutoFlush = true;

                sw.WriteLine($"authinfo user {username}");
                this.response = sr.ReadLine();
                Thread.Sleep(1000);

                sw.WriteLine($"authinfo pass {password}");
                this.response = sr.ReadLine();
                Thread.Sleep(1000);

                return this.response == "381 PASS required";
            } catch
            {
                return false;
            }
        }

        private bool ConnectToServer()
        {

            try
            {
                this.socket = new TcpClient(this.serverName, this.serverPort);
                this.ns = this.socket.GetStream(); // Now we are connected
                return true;
            } catch
            {
                return false;
            }

        }

        private List<string> GetNewsGroups()
        {
            // Making a new list for the groups
            List<string> groups = new List<string>();

            // Sending the command 'list' to the news server
            this.sw.WriteLine("list");

            // Skip responses that are unnecessary
            SkipResponse(2);

            for (int i = 0; i < 18629; i++)
            {
                this.response = this.sr.ReadLine();
                groups.Add(response.Split(' ')[0]);
            }

            // Skip responses that are unnecessary
            SkipResponse();

            return groups;
        }

        private void GetGroupArticles(string firstArticleID, string lastArticleID)
        {
            for (int i = Int32.Parse(firstArticleID); i < Int32.Parse(lastArticleID)+1; i++)
            {
                this.groupArticles.Items.Add(i.ToString());
            }
        }

        private void AddGroupsToList()
        {
            List<string> groups = GetNewsGroups();

            foreach (string group in groups)
            {
                this.newsgroups.Items.Add(group);
            }
        }

        // This method was used because I got multiple responses
        // For every command I used
        private void SkipResponse(int times = 1)
        {
            
            if (times < 1)
            {
                return;
            }

            for (int i = 0; i < times; i++)
            {
                this.response = sr.ReadLine();
            }

        }
    }
}
