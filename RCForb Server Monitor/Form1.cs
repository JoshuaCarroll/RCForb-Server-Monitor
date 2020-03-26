using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace RCForb_Server_Monitor
{
    public partial class Form1 : Form
    {
        SqliteConnection sqlite_conn;
        int mostRecentChatId;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mostRecentChatId = 0;
            txtChat.Text = "";

            try
            {
                sqlite_conn = new SqliteConnection(String.Format("Data Source={0};", Properties.Settings.Default.DatabaseLocation));
                GetChats(false);
                timerDatabasePoll.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception while opening database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            GetInfo();

        }

        private void InsertData()
        {
            sqlite_conn.Open();
            SqliteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable (Col1, Col2) VALUES('Test Text ', 1); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable (Col1, Col2) VALUES('Test1 Text1 ', 2); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable (Col1, Col2) VALUES('Test2 Text2 ', 3); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable1 (Col1, Col2) VALUES('Test3 Text3 ', 3); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_conn.Close();
        }

        private void GetChats(bool playToneOnNewMessage)
        {
            sqlite_conn.Open();
            SqliteDataReader sqlite_datareader = ExecuteQuery(String.Format("SELECT * FROM Chats where ID > {0} order by ID Desc", mostRecentChatId.ToString()));

            bool hadNewRecords = false;
            while (sqlite_datareader.Read())
            {
                hadNewRecords = true;
                mostRecentChatId = sqlite_datareader.GetInt32(0);
                string myreader = sqlite_datareader.GetString(2) + ": " + sqlite_datareader.GetString(5);
                txtChat.Text += myreader + Environment.NewLine + Environment.NewLine;
            }

            sqlite_conn.Close();

            if (hadNewRecords && playToneOnNewMessage) { SystemSounds.Beep.Play(); }
        }

        private void GetInfo()
        {
            dynamic data = DataAccess.GetJson(String.Format("http://{0}:4525/json.js", Properties.Settings.Default.RCForbServerAddress), true);

            string strFreq = data.remote.radio[0].frequency;
            double dblFreq = 0;
            if (double.TryParse(strFreq, out dblFreq))
            {
                strFreq = (dblFreq / 1000000).ToString();
            }

            lblFreq.Text = strFreq;

            string strFreqB = data.remote.radio[0].frequencyb;
            double dblFreqB = 0;
            if (double.TryParse(strFreqB, out dblFreqB))
            {
                strFreqB = (dblFreqB / 1000000).ToString();
            }

            lblFreqB.Text = strFreqB;

            lblUsers.Text = "";
            foreach (var user in data.remote.users)
            {
                lblUsers.Text += user.name;
            }
        }

        private SqliteDataReader ExecuteQuery(string query)
        {
            SqliteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = query;
            return sqlite_cmd.ExecuteReader();
        }

        private void timerDatabasePoll_Tick(object sender, EventArgs e)
        {
            GetChats(true);
            GetInfo();
        }
    }
}
