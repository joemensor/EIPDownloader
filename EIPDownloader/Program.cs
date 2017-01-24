using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Net;

namespace EIPDownloader
{
    class Program
    {
        static int progress = 0;
        static int  status = 0;
        static string downloadID = "0";
        static bool downloading = false;
        static string connstr = "Data Source=SHERLOCK;Initial Catalog=LIVEDOCDB; user=eipdbbulk; pwd=HEj3%P0s8;Connection Timeout=45;";
        static string CompleteDownloadURL(string _downloadID,int status)
        {
            using (SqlConnection connection = new SqlConnection(connstr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("npr.uspCompleteDownloadURL", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@DownloadID", _downloadID));
                command.Parameters.Add(new SqlParameter("@DownloadStatus", status));
                command.Parameters.Add(new SqlParameter("@DownloadComment", null));

                try
                {
                    command.ExecuteNonQuery();                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return "";
        }

        static string GetDownloadURL(string projectID)
        {
            
            string _downloadID ="";
            string _downloadURL = "";
            string _saveToLocation = "";

            using (SqlConnection connection = new SqlConnection(connstr))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("npr.uspGetDownloadURL", connection);
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter pDownloadID = new SqlParameter("@DownloadID", SqlDbType.Int);
                pDownloadID.Direction = ParameterDirection.Output;
                command.Parameters.Add(pDownloadID);
                SqlParameter pDownloadURL = new SqlParameter("@DownloadURL", SqlDbType.VarChar,4000);
                pDownloadURL.Direction = ParameterDirection.Output;
                command.Parameters.Add(pDownloadURL);
                SqlParameter pSaveToLocation = new SqlParameter("@SaveToLocation", SqlDbType.VarChar,4000);
                pSaveToLocation.Direction = ParameterDirection.Output;
                command.Parameters.Add(pSaveToLocation);


                command.Parameters.Add(new SqlParameter("@projectID",projectID));
               
                try
                {
                    command.ExecuteNonQuery();
                    _downloadID = command.Parameters["@DownloadID"].Value.ToString();
                    _downloadURL = command.Parameters["@DownloadURL"].Value.ToString();
                    _saveToLocation = command.Parameters["@SaveToLocation"].Value.ToString();
                }
                catch(Exception e) {

                    Console.WriteLine(e.Message);
                    status = 9;
                    CompleteDownloadURL(_downloadID, status);
                }
               
            }
            try
            {
                Uri uri = new Uri(_downloadURL);
              //  _saveToLocation = @"c:\dloads\";
                string filename = @"\"+System.IO.Path.GetFileName(uri.LocalPath);
                Console.WriteLine("Download Id: " + _downloadID);
                Console.WriteLine("Download Url: " + _downloadURL);
                Console.WriteLine("Save To Location: " + _saveToLocation + filename);



                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted; ;
                    downloading = true;
                    Console.Write("Progress .");
                    wc.Credentials = new System.Net.NetworkCredential("patently", "paulsamazing84!");
                    wc.DownloadFileAsync(new System.Uri(_downloadURL), _saveToLocation + filename);
                }
            }
            catch (Exception e)
            {
                CompleteDownloadURL(_downloadID, status);
            }


            return "" ;
        }

        private static void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine(" Done! ");
            downloading = false;
            status = 2;
            CompleteDownloadURL(downloadID, status);
        }

        static int progressInc = 0;
        static int prevProgress = 0;

        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {            
            progress = e.ProgressPercentage;
            if (progress % 10 == 0 && prevProgress != progress)
            {
                Console.Write(".");
                prevProgress = progress;
            }

        }


        static void Main(string[] args)
        {
            var _quitFlag = false;
            
            Console.Write("Enter a Project ID (or hit enter for none) : ");
            var projectID = Console.ReadLine();            
            while (!_quitFlag)
            {


                if (!downloading)
                {
                    Console.WriteLine("Polling...");
                    GetDownloadURL(projectID);
                }
            }
        }
    }
}
