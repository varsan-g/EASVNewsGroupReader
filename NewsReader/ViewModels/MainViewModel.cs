using NewsReader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NewsReader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _serverDetails;
        private string _username;
        private string _password;
        private string selectedArticleNumber;
        private TcpClient _tcpClient;

        private ObservableCollection<string> _newsGroups;




        public string ServerDetails
        {
            get { return _serverDetails; }
            set
            {
                if (_serverDetails != value)
                {
                    _serverDetails = value;
                    OnPropertyChanged(nameof(ServerDetails));
                }
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }
        public ObservableCollection<string> NewsGroups
        {
            get { return _newsGroups; }
            set
            {
                if (_newsGroups != value)
                {
                    _newsGroups = value;
                    OnPropertyChanged(nameof(NewsGroups));
                }
            }
        }

        private string _selectedNewsGroup;
        public string SelectedNewsGroup
        {
            get { return _selectedNewsGroup; }
            set
            {
                if (_selectedNewsGroup != value)
                {
                    _selectedNewsGroup = value;
                    OnPropertyChanged(nameof(SelectedNewsGroup));
                }
            }
        }
        private ObservableCollection<string> _articleHeadlines;
        public ObservableCollection<string> ArticleHeadlines
        {
            get { return _articleHeadlines; }
            set
            {
                _articleHeadlines = value;
                OnPropertyChanged(nameof(ArticleHeadlines));
            }
        }

        private string _selectedArticle;
        public string SelectedArticle
        {
            get { return _selectedArticle; }
            set
            {
                if (_selectedArticle != value)
                {
                    _selectedArticle = value;
                    OnPropertyChanged(nameof(SelectedArticle));
                }
            }
        }


        public ICommand LoginCommand { get; }
        public ICommand ViewNewsGroupCommand { get; }
        public ICommand ReadArticleCommand { get; }
        public ICommand PostArticleCommand { get; }




        public MainViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            ViewNewsGroupCommand = new RelayCommand(ViewNewsGroup);
            ReadArticleCommand = new RelayCommand(ReadArticle);
            PostArticleCommand = new RelayCommand(PostArticle);


            NewsGroups = new ObservableCollection<string>();

            // Load saved settings
            ServerDetails = Properties.Settings.Default.ServerDetails;
            Username = Properties.Settings.Default.Username;
            Password = Properties.Settings.Default.Password;
            ArticleHeadlines = new ObservableCollection<string>();


        }


        public void Login()
        {
            LoginToServer();
            
        }

        public void LoginToServer()
        {
            int port = 119;

            try
            {

                _tcpClient = new TcpClient(ServerDetails, port);

                NetworkStream stream = _tcpClient.GetStream();

                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                string response = reader.ReadLine();
                Console.WriteLine(response);

                writer.WriteLine($"AUTHINFO USER {Username}");
                response = reader.ReadLine();
                Console.WriteLine(response);

                // Check if the response contains "500" (indicating wrong details)
                if (response.Contains("500"))
                {
                    throw new Exception("Wrong details");
                }

                Console.WriteLine(response);

                writer.WriteLine($"AUTHINFO PASS {Password}");
                response = reader.ReadLine();
                Console.WriteLine(response);


                // Check if the response contains "281" (indicating successful connection)
                if (response.Contains("281"))
                {
                    ListNewsGroups(writer, reader);
                    MessageBox.Show("Connection successful", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Save settings
                    Properties.Settings.Default.ServerDetails = ServerDetails;
                    Properties.Settings.Default.Username = Username;
                    Properties.Settings.Default.Password = Password;
                    Properties.Settings.Default.Save();
                }
                else if (response.Contains("500"))
                {
                    throw new Exception("Wrong details");
                }

                Console.WriteLine(response);


            }
            catch (WebException ex)
            {
                MessageBox.Show($"WebException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IOException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ListNewsGroups(StreamWriter writer, StreamReader reader)
        {
            writer.WriteLine("LIST ACTIVE");

            // Start a background task to read the server response and process the newsgroups
            Task.Run(() =>
            {
                string response = reader.ReadLine();
                Console.WriteLine(response);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    NewsGroups.Clear();
                });

                while ((response = reader.ReadLine()) != null)
                {
                    if (response == ".")
                    {
                        // The dot signifies the end of the newsgroup list
                        break;
                    }

                    Console.WriteLine(response);

                    // Extract the news group name from the response
                    string[] parts = response.Split(' ');
                    if (parts.Length >= 1)
                    {
                        string newsGroup = parts[0];

                        // Update the NewsGroups collection on the UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            NewsGroups.Add(newsGroup);
                        });

                        if (string.Equals(newsGroup, SelectedNewsGroup))
                        {
                            // Set the selected news group
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SelectedNewsGroup = newsGroup;
                            });
                        }


                    }
                }
            });
        }

        public void ReadArticle()
        {
            try
            {
                string selectedArticleNumber = SelectedArticle;
                string command = $"BODY {selectedArticleNumber}";
                StringBuilder articleBuilder = new StringBuilder();

                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    throw new Exception("Not connected to the server");
                }

                if (string.IsNullOrEmpty(SelectedNewsGroup))
                {
                    throw new Exception("Please select a news group");
                }

                NetworkStream stream = _tcpClient.GetStream();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                writer.WriteLine(command);

                string response = reader.ReadLine();
                Console.WriteLine(response);

                if (!response.StartsWith("222"))
                {
                    MessageBox.Show("Article not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Read the response until we hit a dot indicating the end of the article
                while ((response = reader.ReadLine()) != null)
                {
                    Console.WriteLine(response);

                    if (response == ".")
                    {
                        break;
                    }

                    articleBuilder.AppendLine(response);
                }

                string encodedArticleContent = articleBuilder.ToString();
                string decodedArticleContent = Helper.DecodeUtf8String(encodedArticleContent);

                // Open the ArticleWindow with the decoded article content
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ArticleWindow articleWindow = new ArticleWindow(decodedArticleContent);
                    articleWindow.Show();
                });
            }
            catch (WebException ex)
            {
                MessageBox.Show($"WebException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IOException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void ViewNewsGroup()
        {
            try
            {
                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    throw new Exception("Not connected to the server");
                }

                if (string.IsNullOrEmpty(SelectedNewsGroup))
                {
                    throw new Exception("Please select a news group");
                }

                NetworkStream stream = _tcpClient.GetStream();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8); 
                StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                writer.WriteLine($"GROUP {SelectedNewsGroup}");

                string response = reader.ReadLine();
                Console.WriteLine(response);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ArticleHeadlines.Clear();
                });

                // Split the response to get the firstArticle and lastArticle
                string[] parts = response.Split(' ');
                if (parts.Length >= 4 && int.TryParse(parts[2], out int firstArticle) && int.TryParse(parts[3], out int lastArticle))
                {
                    // Send XHDR subject {firstArticle}-{lastArticle} command
                    string xhdrCommand = $"XHDR subject {firstArticle}-{lastArticle}";
                    Console.WriteLine(xhdrCommand);
                    writer.WriteLine(xhdrCommand);

                    response = reader.ReadLine();

                    if (!response.StartsWith("221"))
                    {
                        MessageBox.Show("No articles in the selected newsgroup", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return; 
                    }

                    response = reader.ReadLine();

                    while (response != ".")
                    {
                        Console.WriteLine(response);

                        // Add subject to the ArticleHeadlines collection on the UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!string.IsNullOrEmpty(response))
                            {
                                ArticleHeadlines.Add(response);
                            }
                        });

                        response = reader.ReadLine();
                    }
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show($"WebException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IOException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PostArticle()
        {
            try
            {
                // Check if connected to the server
                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    throw new Exception("Not connected to the server");
                }

                // Check if a news group is selected
                if (string.IsNullOrEmpty(SelectedNewsGroup))
                {
                    throw new Exception("Please select a news group");
                }

                // Open the PostWindow
                var postWindow = new PostWindow();
                var result = postWindow.ShowDialog();

                if (result == true)
                {
                    // Retrieve values from the ArticleWindow
                    string from = postWindow.From;
                    string subject = postWindow.Subject;
                    string body = postWindow.Body;


                    NetworkStream stream = _tcpClient.GetStream();

                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                    // Send GROUP command to select the news group
                    writer.WriteLine($"GROUP {SelectedNewsGroup}");
                    string response = reader.ReadLine();
                    Console.WriteLine(response);

                    // Send POST command
                    writer.WriteLine($"POST");
                    response = reader.ReadLine();
                    Console.WriteLine(response);

                    // Check if posting is allowed
                    if (response.StartsWith("440"))
                    {
                        MessageBox.Show("You are not allowed to post in this group", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (response.StartsWith("340"))
                    {

                        // Send the article text with user input
                        writer.WriteLine($"From: {from}");
                        writer.WriteLine($"Newsgroups: {SelectedNewsGroup}");
                        writer.WriteLine($"Subject: {subject}");
                        writer.WriteLine();
                        writer.WriteLine(body);
                        writer.WriteLine(".");

                        // Read the response after sending the article text
                        response = reader.ReadLine();
                        Console.WriteLine(response);
                    }
                }
            }

            catch (WebException ex)
            {
                MessageBox.Show($"WebException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IOException: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}