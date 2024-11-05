using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Management;

namespace DBE
{
    public partial class MainWindow : Window
    {
        private DatabaseManager databaseManager = new DatabaseManager();
        
        //
        public MainWindow()
        {
            InitializeComponent();
            // Обработчик события KeyDown для ComboBox
            serverComboBox.KeyDown += serverComboBox_KeyDown;
            // Загружаем список серверов при запуске
            LoadServerList();
        }

        // Метод для загрузки списка серверов
        private async void LoadServerList()
        {
            try
            {
                //Получаем список доступных серверов
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject computer in collection)
                {
                    string serverName = computer["Name"].ToString();
                    //Проверяем, существует ли сервер
                    if (await databaseManager.CheckServerExistsAsync(serverName))
                    {
                        //Сервер существует - добавляем в список
                        serverComboBox.Items.Add(serverName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении списка серверов: {ex.Message}");
            }
        }
        //Обработка нажатия на Enter после ввода Название сервера
        private async void serverComboBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Получаем текст из ComboBox
                string serverName = serverComboBox.Text;
                // Вызываем метод ServerInput в DatabaseManager
                await databaseManager.ServerInput(serverName, serverComboBox, databaseComboBox);
            }
        }
        private async void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текст из TextBox
            string serverName = serverComboBox.Text;
            // Вызываем метод HandleServerInput в DatabaseManager
            await databaseManager.ServerInput(serverName, serverComboBox, databaseComboBox);
        }

        private void serverComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Сохраняем выбранный сервер
            if (serverComboBox.SelectedItem != null)
            {
                databaseManager.SelectedServer = serverComboBox.SelectedItem.ToString();
               //databaseComboBox.SelectedIndex = 0;
            }
        }
            private void databaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Сохраняем выбранную базу данных
            if (databaseComboBox.SelectedItem != null)
            {
                databaseManager.SelectedDatabase = databaseComboBox.SelectedItem.ToString();
            }  
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            // Подключаемся к базе данных
            databaseManager.Connect(databaseManager.SelectedServer, databaseManager.SelectedDatabase);
        }
    }
}
