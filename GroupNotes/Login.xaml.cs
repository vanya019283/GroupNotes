using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace GroupNotes
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        //регистрация
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SPLogin.Visibility = Visibility.Collapsed;
            SPRegistration.Visibility = Visibility.Visible;
        }
        //войти
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SPLoginSource.Text == "")
                    throw new Exception("Не заполнен адрес базы данных!!!");
                if (SPLoginLog.Text == "")
                    throw new Exception("Не заполнен имя пользователя!!!");
                if (SPLoginPass.Password == "")
                    throw new Exception("Не заполнен пароль пользователя!!!");
                string connectionString = "Data Source=" + SPLoginSource.Text + ";Initial Catalog=DBGroupNotes;User=" + SPLoginLog.Text + ";Password=" + SPLoginPass.Password;
                using (SqlConnection connection = new SqlConnection(connectionString)) { connection.Open(); }
                ((MainWindow)this.Owner).conStr = connectionString;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //отмена
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SPLogin.Visibility = Visibility.Visible;
            SPRegistration.Visibility = Visibility.Collapsed;
            SPRegistrationLog.Text = "";
            SPRegistrationPass.Password = "";
            SPRegistrationFIO.Text = "";
            SPRegistrationBirthday.Text = "";
        }
        //завершить регистрацию
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SPRegistrationLog.Text == "")
                    throw new Exception("Не заполнен Login!!!");
                if (SPRegistrationPass.Password == "")
                    throw new Exception("Не заполнен Password!!!");
                if (SPRegistrationFIO.Text == "")
                    throw new Exception("Не заполнен ФИО пользователя!!!");
                if (SPRegistrationBirthday.Text == "")
                    throw new Exception("Не заполнен день рождения пользователя!!!");
                string connectionString = "Data Source= ;Initial Catalog=DBGroupNotes;User=sa;Password=zxc";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    List<string> allLogin = connection.Query<string>("select loginname from syslogins where dbname = 'DBGroupNotes'").ToList();
                    if (allLogin.Contains(SPRegistrationLog.Text) == true)
                        throw new Exception("Логин уже существует!!!");
                    connection.Execute(
                        "CREATE LOGIN [" + SPRegistrationLog.Text + "] WITH PASSWORD = N'" + SPRegistrationPass.Password + "', DEFAULT_DATABASE = [DBGroupNotes], DEFAULT_LANGUAGE = [русский], CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF; " +
                        "CREATE USER[" + SPRegistrationLog.Text + "] FOR LOGIN[" + SPRegistrationLog.Text + "] WITH DEFAULT_SCHEMA =[dbo] " +
                        "ALTER ROLE[db_datareader] ADD MEMBER[" + SPRegistrationLog.Text + "] " +
                        "ALTER ROLE[db_datawriter] ADD MEMBER[" + SPRegistrationLog.Text + "]");

                    User newUser = new User
                    {
                        FIO = SPRegistrationFIO.Text,
                        Birthday = SPRegistrationBirthday.SelectedDate.Value,
                        Login = SPRegistrationLog.Text
                    };
                    connection.Insert(newUser);
                }
                MessageBox.Show("Регистрация успешно завершена");
                Button_Click_2(new object(), new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
