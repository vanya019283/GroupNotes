using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace GroupNotes
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Group> groups = new ObservableCollection<Group>();
        ObservableCollection<ShowNote> showNotes = new ObservableCollection<ShowNote>();
        public string conStr = "";
        DateTime startDate;
        DateTime endDate;
        public MainWindow()
        {
            InitializeComponent();
            LeftDocLB.ItemsSource = groups;
            RightGBAddGroup.ItemsSource = groups;
            RightListBox.ItemsSource = showNotes;

            startDate = DateTime.Now.Date;
            endDate = DateTime.Now.Date;
        }

        private void LeftDocButton1_Click(object sender, RoutedEventArgs e)
        {
            if (LeftDocGB1.Visibility == Visibility.Collapsed)
            {
                LeftDocButton1.FontWeight = FontWeights.Bold;
                LeftDocGB1.Visibility = Visibility.Visible;
                startDate = DateTime.Now.Date.AddDays(1);
                endDate = DateTime.MaxValue.Date;
                UpdateNotes(startDate, endDate);
            }
            else
            {
                LeftDocButton1.FontWeight = FontWeights.Normal;
                LeftDocGB1.Visibility = Visibility.Collapsed;
            }
        }

        private void LeftDocButton2_Click(object sender, RoutedEventArgs e)
        {
            if (LeftDocGB2.Visibility == Visibility.Collapsed)
            {
                LeftDocButton2.FontWeight = FontWeights.Bold;
                LeftDocGB2.Visibility = Visibility.Visible;
                UpdateGroups();
            }
            else
            {
                LeftDocButton2.FontWeight = FontWeights.Normal;
                LeftDocGB2.Visibility = Visibility.Collapsed;
            }
        }

        private void RightButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            RightGBAdd.Visibility = Visibility.Visible;
            RightButtonAdd.IsEnabled = false;
        }
        void UpdateNotes(DateTime selDateStart, DateTime selDateEnd, string selGroup = null)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    string addQuery = "";
                    if (selGroup != null)
                        addQuery = "and n.Id_Group = " + selGroup;

                    var objSelect = connection.Query<ShowNote>(
                        "select n.Id, n.Text, n.DateCreate, n.DateComplet, n.Complet, g.Color, g.Name " +
                        " from Groups g, Notes n " +
                        " where n.Id_User in( select Id from Users where Login = CURRENT_USER) " + addQuery +
                        " and n.Id_Group = g.Id " +
                        " and n.DateComplet >= '" + selDateStart.ToShortDateString() +
                        "' and n.DateComplet <= '" + selDateEnd.ToShortDateString() +
                        "' Order by n.DateComplet");

                    showNotes.Clear();
                    foreach (var item in objSelect)
                        showNotes.Add(item);
                    RightButtonSave.Visibility = Visibility.Collapsed;
                    UpdateCountNotes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void RightAddPanelButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    var objAdd = new Note
                    {
                        Text = RightGBAddText.Text,
                        DateComplet = RightGBAddDate.SelectedDate.Value,
                        DateCreate = DateTime.Now,
                        Id_User = connection.QueryFirst<User>("select * from Users where Login = CURRENT_USER;").Id,
                        Id_Group = groups[RightGBAddGroup.SelectedIndex].Id
                    };
                    connection.Insert(objAdd);
                    RightGBAddDate.Text = "";
                    RightGBAddDate.SelectedDate = DateTime.Now;
                    RightGBAddGroup.SelectedIndex = -1;
                    RightGBAddText.Text = "";
                    UpdateNotes(startDate,endDate);
                    RightAddPanelButtonCancel_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void RightAddPanelButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            RightGBAddText.Text = "";
            RightGBAddGroup.SelectedIndex = -1;
            RightGBAddDate.SelectedDate = DateTime.Now;
            RightGBAdd.Visibility = Visibility.Collapsed;
            RightButtonAdd.IsEnabled = true;
            RightGBAddDate.Text = "";
            RightGBAddDate.SelectedDate = DateTime.Now;
            RightGBAddGroup.SelectedIndex = -1;
            RightGBAddText.Text = "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Login userLogin = new Login();
            userLogin.Owner = this;
            userLogin.ShowDialog();
            if (conStr == "")
                this.Close();
            else
                this.Visibility = Visibility.Visible;
            UpdateNotes(startDate,endDate);
        }
        void UpdateGroups()
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    var objSelect = connection.Query<Group>("select * from Groups where Id_User in( select Id from Users where Login = CURRENT_USER)");
                    groups.Clear();
                    foreach (var item in objSelect)
                        groups.Add(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    var objAdd = new Group
                    {
                        Name = LeftGroupName.Text,
                        Color = LeftGroupColor.Color.ToString(),
                        Id_User = connection.QueryFirst<User>("select * from Users where Login = CURRENT_USER").Id
                    };
                    connection.Insert(objAdd);
                    LeftGroupName.Text = "";
                    UpdateGroups();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void RightButtonSave_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    foreach (var item in showNotes)
                    {
                        connection.Execute("UPDATE Notes SET Complet = @Complet WHERE Id = @Id", new { item.Complet, item.Id });
                    }
                    RightButtonSave.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            RightButtonSave.Visibility = Visibility.Visible;
        }

        private void LuftDocButton0_Click(object sender, RoutedEventArgs e)
        {
            startDate = DateTime.Now.Date;
            endDate = DateTime.Now.Date;
            UpdateNotes(startDate, endDate);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            startDate = LeftDocGB1DateStart.SelectedDate.Value;
            endDate = LeftDocGB1DateEnd.SelectedDate.Value;
            UpdateNotes(startDate, endDate);
        }

        private void LeftDocLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeftDocLB.SelectedIndex != -1)
            {
                startDate = DateTime.Now.Date;
                endDate = DateTime.MaxValue.Date;
                UpdateNotes(startDate, endDate, groups[LeftDocLB.SelectedIndex].Id.ToString());
            }
        }

        void UpdateCountNotes()
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                try
                {
                    int n = connection.QueryFirst<int>("" +
                        "select count(id) from Notes n " +
                        "where n.Id_User IN( " +
                            "select Id from Users where Login = CURRENT_USER ) " +
                        "and n.DateComplet > CAST(GETDATE() AS DATE) " +
                        "and n.Complet = 0");
                    CountNComing.Text = n.ToString();
                    n = connection.QueryFirst<int>("" +
                        "select count(id) from Notes n " +
                        "where n.Id_User IN( " +
                            "select Id from Users where Login = CURRENT_USER ) " +
                        "and n.DateComplet = CAST(GETDATE() AS DATE) " +
                        "and n.Complet = 0");
                    CountNToday.Text = n.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void RightGBAddGroup_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateGroups();
        }
    }
}
