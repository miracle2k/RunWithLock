using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;

namespace RunWithLock
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Count() < 1)
                {
                    MessageBox.Show("Requires one argument: an xml configuration file.");
                    return;
                }

                var settings = XElement.Load(args[0]);
                var executable = settings.Element("exe").Value;
                var workingDir = settings.Element("wd") != null ? settings.Element("wd").Value : null;
                var lockfile = settings.Element("lockfile") != null ? settings.Element("lockfile").Value : null;
                var shellExecute = settings.Element("shellexec") != null ? settings.Element("shellexec").Value.ToLower() == "true" : false;

                if (lockfile == null)
                {
                    lockfile = String.Format("{0}.lock", executable);
                }

                // Try to create the lock file
                try
                {
                    var machineName = File.ReadAllText(lockfile);
                    var result = MessageBox.Show(
                        String.Format("Das Programm scheint bereits auf einem PC zu laufen ({0}). Soll es trotzdem gestartet werden (nicht empfohlen)?", machineName),
                        "Fehler", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                        return;
                }
                catch (FileNotFoundException)
                {
                    File.WriteAllText(lockfile, System.Environment.MachineName);
                }

                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = executable;
                    proc.StartInfo.WorkingDirectory = workingDir;
                    proc.StartInfo.UseShellExecute = shellExecute;
                    proc.Start();
                    proc.WaitForExit();
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Fehler beim Starten des gewrappten Programms: {0}", e.Message), "Fehler");
                }
                finally
                {
                    File.Delete(lockfile);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Fehler: {0}", e.Message), "Fehler");
            }
        }
    }
}
