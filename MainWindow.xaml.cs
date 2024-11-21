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
            ShowZoos(); // Methodenaufruf | Zeigt alle 'Städte' mit Zoo's die in der Datenbank gelistet sind
            ShowAllAnimals(); // Methodenaufruf, zeigte alle 'Tiere' die in der Datenbank gelistet sind
        }

        //-------------------------------------------------------------------------->Methode zeigt die Städte an, die ein Zoo haben<
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

        //-------------------------------------------------------------------------->Methode zeigt alle tiere an, die in der Datenbank gelistet sind<
        public void ShowAllAnimals()
        {
            try
            {
                string query = "select * from Animal";
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlDataAdapter)
                {
                    DataTable animalTable = new DataTable();
                    sqlDataAdapter.Fill(animalTable);
                    listAllAnimals.DisplayMemberPath = "Name";
                    listAllAnimals.SelectedValue = "Id";
                    listAllAnimals.ItemsSource = animalTable.DefaultView;



                }
            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
            }
        }


        //-------------------------------------------------------------------------->Methode zeigt tiere im ausgewählten Zoo an<
        private void ShowAssociatedAnimals()               
        {
            if(listZoos.SelectedValue == null) // wenn das Selectedvalue leer ist geh aus der methode raus und mach nichts!, wenn er nicht lee ist versuche, denn code im try catch auszuführen
            {
                return;
            }
            try
            {
                string query =  "SELECT a.Name \r\n" +  // bedeutet zeilenumbruch in der anweisung: \r\n
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
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
            }

            
        }

        private void listZoos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAssociatedAnimals();
        }



        // Zoo löschen knopf
        private void DeleteZoo_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string query = "delete from Zoo where id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open(); // wiel die verbindung geöffnet wurde, muss sie unten in finally geschlossenw werden!
                sqlCommand.Parameters.AddWithValue("@ZooId", listZoos.SelectedValue);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close(); // Verbindung wird geschlossen
                ShowZoos();
            }

            
        }
    }
}
