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
using System.Threading.Tasks;
using System.Windows.Media;

using Rargh;

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
            this.Color = Colors.Black;
        }

        public string PathName { get; set; }

        public Color Color { get; set; }

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
                this.Color = Colors.Red;

                // And we quit hencewith.
                return;
            }

            main.StatusIsDone (this);
            this.Color = Colors.Gray;
        }
    }
}
