#region header

// Rargh - MainWindow.xaml.cs
// 
// Alistair J. R. Young
// Arkane Systems
// 
// Copyright Arkane Systems 2012-2013.
// 
// Created: 2013-07-21 6:53 PM

#endregion

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace ArkaneSystems.Rargh
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string FolderName;

        public MainWindow ()
        {
            this.InitializeComponent ();

            this.Rarchives = new ObservableCollection<Rarchive> ();
        }

        public ObservableCollection<Rarchive> Rarchives { get; set; }

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            redofromstart:

            // Window is loaded.  Now get the folder containing the rarchives.
            var dlg = new CommonOpenFileDialog ();

            dlg.Title = "Select folder containing the rarchives";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            // Close the app if we cancel at this point.
            if (dlg.ShowDialog (this) != CommonFileDialogResult.Ok)
                this.Close ();

            // Get the .rar files in the folder in question.
            this.FolderName = dlg.FileName;

            var rarchiveFiles = Directory.EnumerateFiles (this.FolderName, "*.rar", SearchOption.TopDirectoryOnly);

            if (!rarchiveFiles.Any ())
            {
                MessageBox.Show (this,
                                 "There are no .rarchives in that folder.",
                                 "Rargh",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Warning);
                goto redofromstart;
            }

            var oldCursor = Mouse.OverrideCursor;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                foreach (string s in rarchiveFiles)
                    this.Rarchives.Add (new Rarchive (s));

                this.Progress.Maximum = this.Rarchives.Count;
            }
            finally
            {
                Mouse.OverrideCursor = oldCursor;
            }

            // Now we've got the rarchives, enable the go button.
            this.Go.IsEnabled = true;
        }

        private async void Go_Click (object sender, RoutedEventArgs e)
        {
            // Re-disable the button.
            this.Go.IsEnabled = false;

            // Trigger the async method which does the actual work for each and every one on the thread pool.
            await Task.Run (async () => await this.GoAsync ());
        }

        private async Task GoAsync ()
        {
            foreach (Rarchive rar in this.Rarchives)
                await rar.Process (this);
        }

        public void StatusIsStarting (Rarchive rar)
        {
            this.Dispatcher.Invoke (() =>
                                    {
                                        this.Output.Items.Add (string.Format ("Starting: {0}", rar.Name));

                                        // Make sure Output is scrolled to the bottom.
                                        this.Output.ScrollIntoView (this.Output.Items[this.Output.Items.Count - 1]);
                                    });
        }

        public void StatusIsExtracted (Rarchive rar, string file)
        {
            this.Dispatcher.Invoke (() =>
                                    {
                                        this.Output.Items.Add (string.Format ("...extracted: {0}", file));

                                        // Make sure Output is scrolled to the bottom.
                                        this.Output.ScrollIntoView (this.Output.Items[this.Output.Items.Count - 1]);
                                    });
        }

        public void StatusIsDone (Rarchive rar)
        {
            this.Dispatcher.Invoke (() =>
                                    {
                                        this.Output.Items.Add (string.Format ("Finished: {0}", rar.Name));
                                        this.Progress.Value++;

                                        // Make sure Output is scrolled to the bottom.
                                        this.Output.ScrollIntoView (this.Output.Items[this.Output.Items.Count - 1]);
                                    });
        }

        public void StatusIsError (Rarchive rar, string error)
        {
            this.Dispatcher.Invoke (() =>
                                    {
                                        this.Output.Items.Add (string.Format ("Error processing file: {0} - {1}",
                                                                              rar.Name,
                                                                              error));
                                        this.Progress.Value++;

                                        // Make sure Output is scrolled to the bottom.
                                        this.Output.ScrollIntoView (this.Output.Items[this.Output.Items.Count - 1]);
                                    });
        }
    }
}
