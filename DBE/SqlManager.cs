using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DBE
{
    public class SqlManager: DatabaseManager
    {
        private SqlConnection connection;
        private string serverName;
        private string databaseName;
        private string connectionString;
        private List<string> columnNames; // Список имен столбцов для выбранной таблицы


        public SqlManager(string serverName, string databaseName)
        {
            this.serverName = serverName;
            this.databaseName = databaseName;
            connectionString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
            connection = new SqlConnection(connectionString);
        }

        public void ExecuteNonQuery(string sql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        //Метод для Удаления выбранного столбца (проверено)
        public void DeleteSelectedColumn(string columnName, string tableName)
        {
            string sql = $"ALTER TABLE {tableName} DROP COLUMN {columnName}";
            ExecuteNonQuery(sql);
        }

        //Метод для Создания новой таблицы(проверено)
        public void CreateNewTable(string tableName)
        {
            string sql = $"CREATE TABLE dbo.{tableName} (id INT IDENTITY(1,1) PRIMARY KEY)";
            ExecuteNonQuery(sql);
        }

        //Метод для Вставки Столбца (проверено)
        public void AddColumn(string tableName, string columnName, string dataType)
        {
            string sql = $"ALTER TABLE dbo.{tableName} ADD {columnName} {dataType}";
            ExecuteNonQuery(sql);
        }

        //Метод для Удаление таблицы (проверено)
        public void DeleteTable(string tableName)
        {
            string sql = $"DROP TABLE dbo.{tableName}";
            ExecuteNonQuery(sql);
        }

        //Метод для Сохранения данных в БД (проверено)
        public void SaveChanges(string tableName, object data)
        {
            try
            {
                //Получаем текущую DataTable из DataGrid
                DataView view = (DataView)data;
                DataTable table = view.Table;

                // Используем SqlDataAdapter для обновления базы данных
                using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM dbo.{tableName}", connection))
                {
                    // Создаем SqlCommandBuilder для автоматической генерации команд INSERT, UPDATE, DELETE
                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                    // Открываем соединение с базой данных
                    connection.Open();

                    // Проверка на изменения в DataTable
                    if (table.GetChanges() != null)
                    {
                        // Обновляем базу данных
                        try
                        {
                            adapter.Update(table);
                            MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Обработка ошибки обновления
                            MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет изменений для сохранения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Закрываем соединение
                connection.Close();
            }
        }

        // Получение списка таблиц из базы данных
        public List<string> GetTableNames()
        {
            string connectionString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
            List<string> tableNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'";
                    SqlCommand command = new SqlCommand(sql, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tableNames.Add(reader["TABLE_NAME"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Ошибка");
                }
            }
            return tableNames;
        }

        //Метод для обновления DataGrid и списка столбцов в columnComboBox
        public void UpdateView(DataGrid dataGrid, ComboBox columnComboBox, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                // Очищаем DataGrid
                dataGrid.ItemsSource = null; // Очищаем текущие данные
                columnComboBox.ItemsSource = null; // Очищаем текущие элементы ComboBox

                // Загружаем структуру таблицы в DataGrid
                using (SqlCommand command = new SqlCommand($"SELECT * FROM dbo.{tableName}", connection))
                {
                    try
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        // Устанавливаем загруженные данные в DataGrid
                        dataGrid.ItemsSource = table.DefaultView;
                        dataGrid.AutoGenerateColumns = true; // Генерируем колонки автоматически

                        // Обновляем список столбцов в columnComboBox, исключая столбцы с именем "id"
                        columnComboBox.ItemsSource = table.Columns.Cast<DataColumn>().Where(c => !c.ColumnName
                            .Equals("id", StringComparison.OrdinalIgnoreCase)) // Исключаем столбец "id"
                            .Select(c => c.ColumnName).ToList();
                    }
                    catch (SqlException sqlEx)
                    {
                        // Обработка исключения, связанного с базой данных
                        MessageBox.Show($"Ошибка доступа к базе данных: {sqlEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        // Общая обработка ошибок
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


    }
}

