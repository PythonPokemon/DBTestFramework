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
            ZeigeZoosAn(); // Methodenaufruf | Zeigt alle 'Städte' mit Zoo's die in der Datenbank gelistet sind
            ZeigeAlleTiereAn(); // Methodenaufruf, zeigte alle 'Tiere' die in der Datenbank gelistet sind
        }

        //-------------------------------------------------------------------------->Methode zeigt die Städte an, die ein Zoo haben<
        private void ZeigeZoosAn()
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
        public void ZeigeAlleTiereAn()
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
        private void ZeigtAlleTiereImZoo()               
        {
            if(listZoos.SelectedValue == null) // wenn das Selectedvalue wert leer ist geh aus der methode raus und mach nichts!, wenn der wert nicht leer ist versuche, denn code im try catch auszuführen
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
            ZeigtAlleTiereImZoo();
            ZeigeAusgewähltenZooInDerTextBoxAn();
        }



        //-------------------------------------------------------------------------->Methode: Zoo aus der Datenbank lösche, samt eintrag | 'löschen' taste<
        private void ZooLöschen(object sender, RoutedEventArgs e)
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
                ZeigeZoosAn();
            }            
        }

        //-------------------------------------------------------------------------->Methode: Zoo der Datenbank hinzufügen | 'Zoo hinzufügen' taste<
        public void NeuenZooHinzufügen(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Funktionstest, ob der knopf funktioniert");

            try
            {
                string query = "insert into Zoo values (@Location)"; // @Location ist ein parameter aus der Datenbank, um den zu setzen brtaucht man sqlcommand
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@Location", myTextBox.Text); // der text der in der textbox eingetragen und durch name'myTextBox' hier referenziert wird, soll in der Datenbank tabelle 'Zoo' in der spalte Location als parameter übergeben werden
                sqlCommand.ExecuteScalar(); // das braucht man zu ausführen!
            }
            catch (Exception )
            {

                MessageBox.Show( "Fehler beim hinzufügen");
            }
            finally
            {
                sqlConnection.Close();
                ZeigeZoosAn();
            }

        }

        //-------------------------------------------------------------------------->Methode: Tier einem Zoo zuweisen, in der Datenbank auch | 'Tier hinzufügen' taste<
        public void TierZumZooHinzufügen(object sender, RoutedEventArgs e)
        {
            if (listZoos.SelectedItem == null || listAllAnimals.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie sowohl einen Zoo als auch ein Tier aus.");
                return;
            }

            try
            {
                // Werte aus DataRowView extrahieren
                int zooId = Convert.ToInt32(((DataRowView)listZoos.SelectedItem)["Id"]);
                int animalId = Convert.ToInt32(((DataRowView)listAllAnimals.SelectedItem)["Id"]);

                

                // SQL-Query
                string query = "INSERT INTO ZooAnimal (ZooId, AnimalId) VALUES (@ZooId, @AnimalId)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                
                    sqlConnection.Open();
                    sqlCommand.Parameters.AddWithValue("@ZooId", zooId);
                    sqlCommand.Parameters.AddWithValue("@AnimalId", animalId);

                // Der Befehl sqlCommand.ExecuteNonQuery() wird in ADO.NET verwendet, um eine SQL-Abfrage auszuführen, die keine Daten zurückgibt.
                int rowsAffected = sqlCommand.ExecuteNonQuery();                                 

            }
            catch (Exception)
            {

                MessageBox.Show("Fehler beim hinzufügen");
            }

            finally
            {                
                sqlConnection.Close();              
                ZeigtAlleTiereImZoo(); // Aktualisiere die Ansicht der zugeordneten Tiere
            }
        }

        //-------------------------------------------------------------------------->Methode: Tier der Tierliste, Datenbank hinzufügen | 'Tier hinzufügen' taste<
        public void TierDerTierListeHinzufügen(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Funktionstest, ob der knopf funktioniert");

            try
            {
                string query = "insert into Animal values (@Name)"; // @Location ist ein parameter aus der Datenbank, um den zu setzen brtaucht man sqlcommand
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@Name", myTextBox.Text); // der text der in der textbox eingetragen und durch name'myTextBox' hier referenziert wird, soll in der Datenbank tabelle 'Zoo' in der spalte Location als parameter übergeben werden
                sqlCommand.ExecuteScalar(); // das braucht man zu ausführen!
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler beim hinzufügen eines Tieres zur Tiertabelle");
            }
            finally
            {
                sqlConnection.Close();
                ZeigeAlleTiereAn();
            }

        }

        //-------------------------------------------------------------------------->Methode: Tier aus der Tierliste, Datenbank löschen | 'Tier löschen' taste<

        public void TierAusTierLIsteLöschen(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Funktionstest, ob der knopf funktioniert");

            try
            {
                // Werte aus DataRowView extrahieren
                int zooId = Convert.ToInt32(((DataRowView)listZoos.SelectedItem)["Id"]);
                int animalId = Convert.ToInt32(((DataRowView)listAllAnimals.SelectedItem)["Id"]);

                string query = "delete from Animal where id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@ZooId", zooId);
                sqlCommand.Parameters.AddWithValue("@AnimalId", animalId);

                // Der Befehl sqlCommand.ExecuteNonQuery() wird in ADO.NET verwendet, um eine SQL-Abfrage auszuführen, die keine Daten zurückgibt.
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close(); // Verbindung wird geschlossen
                ZeigeAlleTiereAn();
                ZeigtAlleTiereImZoo();
            }
        }

        //-------------------------------------------------------------------------->Methode: Zeige den Ausgewählten Zoo In Der TextBox An, Datenbank auch | 'xx' taste<
        public void ZeigeAusgewähltenZooInDerTextBoxAn() // daraus achten das die methoden signatur gleich ist, beim methodenaufruf
        {
            try
            {
                string query = "select location from Zoo where Id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                using (sqlDataAdapter)
                {
                    sqlCommand.Parameters.AddWithValue("@ZooId", listZoos.SelectedValue);
                    DataTable zooDataTable = new DataTable();
                    sqlDataAdapter.Fill(zooDataTable);

                    myTextBox.Text = zooDataTable.Rows[0]["Location"].ToString(); // gib mir an der stelle 0 die location als string
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        //-------------------------------------------------------------------------->Methode: Zeige den Ausgewählten Zoo In Der TextBox An, Datenbank auch | 'xx' taste<
        public void ZeigtAusgewähltesTierAusTierListeInTextBoxAn()
        {
            if (listAllAnimals.SelectedItem == null)
            {
                myTextBox.Text = string.Empty; // Textbox leeren, wenn kein Tier ausgewählt wurde
                return;
            }

            try
            {
                // AnimalId aus DataRowView extrahieren
                int animalId = Convert.ToInt32(((DataRowView)listAllAnimals.SelectedItem)["Id"]);

                string query = "SELECT Name FROM Animal WHERE Id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@AnimalId", animalId);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable zooDataTable = new DataTable();
                sqlDataAdapter.Fill(zooDataTable);

                if (zooDataTable.Rows.Count > 0)
                {
                    myTextBox.Text = zooDataTable.Rows[0]["Name"].ToString();
                }
                else
                {
                    myTextBox.Text = "Kein Tier gefunden.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Anzeigen des ausgewählten Tiers: {ex.Message}");
            }
        }



        public void listAllAnimals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZeigtAusgewähltesTierAusTierListeInTextBoxAn();
        }

       

    }
}

/*
 Good to know Goody

Der Befehl sqlCommand.ExecuteNonQuery() wird in ADO.NET verwendet, um eine SQL-Abfrage auszuführen, die keine Daten zurückgibt.

Bedeutung im Detail:

Rückgabewert:
ExecuteNonQuery gibt die Anzahl der betroffenen Zeilen in der Datenbank zurück.

Beispiel für betroffene Zeilen:
Bei einem INSERT-Statement: Anzahl der eingefügten Zeilen.
Bei einem UPDATE-Statement: Anzahl der aktualisierten Zeilen.
Bei einem DELETE-Statement: Anzahl der gelöschten Zeilen.
Einsatzgebiet:
ExecuteNonQuery wird hauptsächlich verwendet für:

Datenmodifikation: INSERT, UPDATE, DELETE
Datenbank-Objekte: Erstellen oder Löschen von Tabellen, Views etc. mit CREATE TABLE oder DROP TABLE.
Unterschied zu anderen Methoden:

ExecuteReader(): Liefert Daten in Form eines SqlDataReader.
ExecuteScalar(): Liefert den Wert der ersten Spalte der ersten Zeile des Ergebnisses (z. B. ein einzelner Wert).
 */