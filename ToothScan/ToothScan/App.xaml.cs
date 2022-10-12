using Xamarin.Forms;
using ToothScan.Data;
using System.IO;
using System;
using ToothScan.Models;

namespace ToothScan
{
    public partial class App : Application
    {
        static NotesDB notesDB;
        /*
           Creating an instance of the 'NotesDB' class
           responsible for interacting with the application database
         */
        public static NotesDB NotesDB
        {
            get
            {
                if (notesDB == null)
                {
                    notesDB = new NotesDB(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "NotesDatabase.db3"));
                }
                return notesDB;
            }
        }
        /*
           Setting the main page and setting the default
           value for the variable 'start_search_index'
         */
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            Connections.start_search_index = -1;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
