using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace DBE
{
    public partial class Window1 : Window
    {
        private SqlManager SqlClass;
        private string currentTableName = ""; //Текущая выбранная таблица
        private List<string> tableNames; //Список имен таблиц

        public Window1(string serverName, string databaseName)
        {

            InitializeComponent();
            SqlClass = new SqlManager(serverName, databaseName);

            //Заполняем datatypeComboBox
            datatypeComboBox.Items.Add("INT");
            datatypeComboBox.Items.Add("VARCHAR(255)");
            datatypeComboBox.Items.Add("DATE");
            datatypeComboBox.Items.Add("FLOAT");
            datatypeComboBox.SelectedIndex = 0;

            // Обновляем список таблиц
            UpdateTableList();
        }

        //Метод для обновления списка таблиц
        private void UpdateTableList()
        {
            tablelistBox.Items.Clear();
            tableNames = SqlClass.GetTableNames();
            foreach (string tableName in tableNames)
            {
                tablelistBox.Items.Add(tableName);
            }
        }

        //Выбор таблицы из списка и загрузка данных в DataGrid
        private void tablelistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string tableName = tablelistBox.SelectedItem?.ToString();
                SqlClass.UpdateView(DataTable, columnComboBox, tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке структуры таблицы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Кнопка -> Создать таблицу
        private void CreatTableButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = tablenameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Введите имя таблицы", "Ошибка");
                return;
            }
            if (SqlClass.GetTableNames().Contains(tableName))
            {
                MessageBox.Show($"Таблица с именем '{tableName}' уже существует", "Ошибка");
                return;
            }
            try
            {
                SqlClass.CreateNewTable(tableName);
                UpdateTableList();
                MessageBox.Show($"Таблица '{tableName}' успешно создана", "Успех");
                tablenameTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Ошибка");
            } 
        }

        //Кнопка -> Удалить таблицу
        private void DeleteTableButton_Click(object sender, RoutedEventArgs e)
        {
            currentTableName =  tablelistBox.SelectedItem?.ToString();
            string oldTable = currentTableName;
            if (string.IsNullOrEmpty(currentTableName))
            {
                MessageBox.Show("Выберите таблицу для удаления", "Ошибка");
                return;
            }
            if (MessageBox.Show($"Вы уверены, что хотите удалить таблицу '{currentTableName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    SqlClass.DeleteTable(currentTableName);
                    UpdateTableList();
                    DataTable.ItemsSource = null; // Очистка DataGridView
                    currentTableName = "";
                    SqlClass.UpdateView(DataTable, columnComboBox, currentTableName);
                    MessageBox.Show($"Таблица '{oldTable}' успешно удалена", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Ошибка");
                }
            }
        }

        //Кнопка -> Добавить столбец
        private void AddColumButton_Click(object sender, RoutedEventArgs e)
        {
            currentTableName =  tablelistBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(currentTableName))
            {
                MessageBox.Show("Выберите таблицу для добавления столбца", "Ошибка");
                return;
            }
            if (string.IsNullOrEmpty(columnameTextBox.Text.Trim()))
            {
                MessageBox.Show("Введите имя столбца", "Ошибка");
                return;
            }

            string dataType = datatypeComboBox.SelectedItem.ToString();

            try
            {
                SqlClass.AddColumn(currentTableName, columnameTextBox.Text.Trim(), dataType);
                SqlClass.UpdateView(DataTable, columnComboBox, currentTableName);
                columnameTextBox.Clear();
                MessageBox.Show($"Столбец '{columnameTextBox.Text.Trim()}' успешно добавлен в таблицу '{currentTableName}'", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении столбца: {ex.Message}", "Ошибка");
            } 
        }

        //Кнопка -> Удалить столбец
        private void DeleteColumButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = tablelistBox.SelectedItem?.ToString();
            string columnName = columnComboBox.SelectedItem?.ToString();
            string oldColumn = columnName;
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(columnName))
            {
                try
                {
                    SqlClass.DeleteSelectedColumn(columnName, tableName);
                    SqlClass.UpdateView(DataTable, columnComboBox, tableName);
                    MessageBox.Show($"Столбец '{oldColumn}' удален из таблицы '{tableName}'.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении столбца: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите таблицу и столбец для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //Кнопка -> сохранить изменения в БД
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем имя таблицы
            string tableName = tablelistBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(tableName))
            {
                // Вызываем метод SaveChanges из SqlClass
                SqlClass.SaveChanges(tableName, DataTable.ItemsSource);
                SqlClass.UpdateView(DataTable, columnComboBox, tableName);
            }
            else
            {
                MessageBox.Show("Выберите таблицу для сохранения данных.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DataTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void datatypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void columnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
       
        private void columnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}