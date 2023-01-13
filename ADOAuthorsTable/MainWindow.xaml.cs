using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace ADOAuthorsTable;

public partial class MainWindow : Window
{
    SqlConnection? connection = null;
    DataTable table;
    SqlDataAdapter adapter;
    SqlCommandBuilder commandBuilder;

    public MainWindow(string? connectionStr)
    {
        InitializeComponent();
        DataContext = this;

        table = new();

        connection = new SqlConnection(connectionStr);
        adapter = new("SELECT * FROM Authors", connection);
        commandBuilder = new(adapter);
    }

    private void FillTableWithData()
    {
        table.Clear();

        adapter?.Fill(table);

        AuthorsTable.ItemsSource = table.AsDataView();
    }

    private void UpdateDatabase()
    {
        // Insert Command With Stored Procedure
        SqlCommand insertCommand = new()
        {
            CommandText = "usp_AddAuthor",
            CommandType = CommandType.StoredProcedure,
            Connection = connection
        };

        insertCommand.Parameters.Add("id", SqlDbType.Int);
        insertCommand.Parameters["id"].SourceVersion = DataRowVersion.Current;
        insertCommand.Parameters["id"].SourceColumn = "Id";

        insertCommand.Parameters.Add("firstName", SqlDbType.NVarChar);
        insertCommand.Parameters["firstName"].SourceVersion = DataRowVersion.Current;
        insertCommand.Parameters["firstName"].SourceColumn = "FirstName";

        insertCommand.Parameters.Add("lastName", SqlDbType.NVarChar);
        insertCommand.Parameters["lastName"].SourceVersion = DataRowVersion.Current;
        insertCommand.Parameters["lastName"].SourceColumn = "LastName";

        adapter.InsertCommand = insertCommand;

        // CREATE PROCEDURE usp_AddAuthor
        // @id int, @firstName nvarchar(15), @lastname nvarchar(25)
        // AS
        // BEGIN
        // 
        //     INSERT Authors VALUES(@id, @firstName, @lastname)
        // END

        // Update Command With Raw Stirng
        SqlCommand updateCommand = new SqlCommand("UPDATE Authors SET FirstName=@aFirstName, LastName=@aLastName WHERE Id=@aId", connection);

        updateCommand.Parameters.Add("aId", SqlDbType.Int);
        updateCommand.Parameters["aId"].SourceVersion = DataRowVersion.Original;
        updateCommand.Parameters["aId"].SourceColumn = "Id";


        updateCommand.Parameters.Add("aFirstName", SqlDbType.NVarChar);
        updateCommand.Parameters["aFirstName"].SourceVersion = DataRowVersion.Current;
        updateCommand.Parameters["aFirstName"].SourceColumn = "FirstName";

        updateCommand.Parameters.Add("aLastName", SqlDbType.NVarChar);
        updateCommand.Parameters["aLastName"].SourceVersion = DataRowVersion.Current;
        updateCommand.Parameters["aLastName"].SourceColumn = "LastName";

        adapter.UpdateCommand = updateCommand;

        try
        {
            adapter.Update(table);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void DeleteSelectedAuthor()
    {
        // Delete Command With Command Builder
        DataRow? row = (AuthorsTable.SelectedItem as DataRowView)?.Row;

        if (row is null)
            return;

        row.Delete();

        adapter.Update(table);
    }

    private void ButtonFill_Click(object sender, RoutedEventArgs e) => FillTableWithData();

    private void ButtonUpdate_Click(object sender, RoutedEventArgs e) => UpdateDatabase();

    private void ButtonDelete_Click(object sender, RoutedEventArgs e) => DeleteSelectedAuthor();

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        table.Clear();

        var txt = SearchTxt.Text;



        adapter.SelectCommand.CommandText = $"SELECT * FROM Authors \r\nWHERE LOWER(FirstName) LIKE LOWER('%{txt}%') OR LOWER(LastName) LIKE LOWER('%{txt}%') OR LOWER(FirstName + ' ' + LastName) LIKE LOWER('%{txt}%')";

        adapter.Fill(table);

        AuthorsTable.ItemsSource = table.AsDataView();

        adapter.SelectCommand.CommandText = "SELECT * FROM Authors";
    }
}
