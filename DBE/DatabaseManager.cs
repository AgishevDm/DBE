using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace DBE
{
    public class DatabaseManager
    {
        public string SelectedServer { get; set; }
        public string SelectedDatabase { get; set; }

        //Метод для проверки введенного сервера
        public async Task ServerInput(string serverName, ComboBox serverComboBox, ComboBox databaseComboBox)
        {
            if (await CheckServerExistsAsync(serverName))
            {
                // Сервер существует - добавляем в список, выбираем и очищаем ComboBox
                serverComboBox.Items.Add(serverName);
                serverComboBox.SelectedIndex = serverComboBox.Items.Count - 1;
                serverComboBox.Text = "";

                // Обновляем список баз данных
                PopulateDatabasesList(serverName, databaseComboBox);
            }
            else
            {
                serverComboBox.Text = "";
                // Сервер не существует - выводим сообщение об ошибке
                MessageBox.Show("Сервер не существует или не запущен. Убедитесь в правильности введенных данных.", "Ошибка");

                // Удаляем неверный сервер из списка
                serverComboBox.Items.Remove(serverName);
            }
        }

        // Метод для проверки существования сервера
        public async Task<bool> CheckServerExistsAsync(string serverName)
        {
            // Проверяем, можно ли подключиться к серверу
            try
            {
                using (SqlConnection connection = new SqlConnection($"Server={serverName};Database=master;Integrated Security=true;"))
                {
                    await connection.OpenAsync();
                    return true; // Сервер существует и запущен
                }
            }
            catch (SqlException)
            {
                return false; // Сервер не существует или не запущен
            }
        }

        // Метод для заполнения ComboBox баз данных
        public void PopulateDatabasesList(string server, ComboBox databaseComboBox)
        {
            databaseComboBox.Items.Clear();
            // Используем строку подключения
            string connectionString = $"Server={server};Integrated Security=true;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT name FROM sys.databases;", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        databaseComboBox.Items.Add(reader.GetString(0));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении баз данных на сервере {server}: {ex.Message}", "Ошибка");
                }
            }
        }
        // Метод для подключения
        public void Connect(string selectedServer, string selectedDatabase)
        {
            // Проверяем, выбраны ли сервер и база данных
            if (!string.IsNullOrEmpty(selectedServer) && !string.IsNullOrEmpty(selectedDatabase))
            {
                Window1 tableManagerForm = new Window1(selectedServer, selectedDatabase);
                tableManagerForm.ShowDialog();
                // Закрываем форму
                //this.DialogResult = DialogResult.OK;
                //this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сервер и базу данных.", "Ошибка");
            }
        }
    }
}
