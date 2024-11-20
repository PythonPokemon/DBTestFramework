using System;
using System.Collections.Generic;
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
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DBTestFramework
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;
        public MainWindow()
        {
            

            InitializeComponent();
            // Verbindung zu unserer Datenbank
            string connectionString = ConfigurationManager.ConnectionStrings["DBTestFramework.Properties.Settings.CardioVaskularConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connectionString);
            ShowZoos(); // Methodenaufruf
        }

        private void ShowZoos()
        {
            string query = "Select * from Zoo";
            // SqlDataAdapter == ist sowas wie ein Interface was ermöglich tabellen wie c# objekte zu verwenden
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection); 

            using (sqlDataAdapter)
            {
                DataTable zooTable = new DataTable();
                sqlDataAdapter.Fill(zooTable);

                // Welche Informationen der Tabelle, in unserem 'DataTable' sollen in unserer 'Listbox' angezeigt werden.
                listZoos.DisplayMemberPath = "Location";
                // Welcher Wert soll gegeben werden, wenn eines unserer Items von der Listbox ausgewählt wird.
                listZoos.SelectedValuePath = "Id";
                //
                listZoos.ItemsSource = zooTable.DefaultView;
            }
        }

        private void ShowAssociatedAnimals()
        {
            string query = "SELECT a.Name \r\n" +  // bedeutet zeilenumbruch in der anweisung: \r\n
                            "FROM Animal a \r\n" +
                            "INNER JOIN ZooAnimal za ON a.Id = za.AnimalId \r\n" +
                            "WHERE za.ZooId = @ZooId";  // variable die man später setzen kann: @ZooId

            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            using (sqlDataAdapter)
            {
                sqlCommand.Parameters.AddWithValue("@ZooId", listZoos.SelectedValue);

                DataTable animalTable = new DataTable();
                sqlDataAdapter.Fill(animalTable);

                // Welche Informationen der Tabelle, in unserem 'DataTable' sollen in unserer 'Listbox' angezeigt werden.
                listAssociatedAnimals.DisplayMemberPath = "Name";
                // Welcher Wert soll gegeben werden, wenn eines unserer Items von der Listbox ausgewählt wird.
                listAssociatedAnimals.SelectedValuePath = "Id";
                //
                listAssociatedAnimals.ItemsSource = animalTable.DefaultView;
            }
        }

        private void listZoos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAssociatedAnimals();
        }
    }
}
