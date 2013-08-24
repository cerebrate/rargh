#region header

// Rargh - Rarchive.cs
// 
// Alistair J. R. Young
// Arkane Systems
// 
// Copyright Arkane Systems 2012-2013.
// 
// Created: 2013-08-24 1:53 AM

#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;

using SharpCompress.Archive;

namespace ArkaneSystems.Rargh
{
    /// <summary>
    ///     Internal representation of a rarchive.
    /// </summary>
    public class Rarchive
    {
        public Rarchive (string path)
        {
            this.PathName = path;
            this.Color = Brushes.Black;
        }

        public string PathName { get; set; }

        public SolidColorBrush Color { get; set; }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension (this.PathName); }
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString ()
        {
            return this.Name;
        }

        public async Task Process (MainWindow main)
        {
            main.StatusIsStarting (this);

            try
            {
                // Get the stuff in the archive.
                using (var archive = ArchiveFactory.Open (this.PathName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            // Handle the case wjere a new file will have the same name as an existing file.
                            var newFileName = Path.Combine (main.FolderName, Path.GetFileName (entry.FilePath));

                            if (File.Exists (newFileName))
                            {
                                var replacementName = Path.Combine (main.FolderName,
                                                                    string.Format ("{0}-{1}.{2}",
                                                                    Path.GetFileNameWithoutExtension (entry.FilePath),
                                                                    Path.GetRandomFileName(),
                                                                    Path.GetExtension (entry.FilePath)));

                                File.Move (newFileName, replacementName);
                            }

                            // Extract the file.
                            entry.WriteToDirectory (main.FolderName);
                            main.StatusIsExtracted (this, entry.FilePath);

                            await Task.Yield ();
                        }
                    }
                }

                File.Delete (this.PathName);
            }
            catch (Exception ex)
            {
                // We don't stop for anything.
                main.StatusIsError (this, ex.Message);
                this.Color = Brushes.Red;

                // And we quit hencewith.
                return;
            }

            main.StatusIsDone (this);
            this.Color = Brushes.Gray;
        }
    }
}
