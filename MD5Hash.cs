using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Legion_Tactical_Launcher
{
    class MD5Hash
    {
        private string GenerateMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        //private void hashMaster(string startPath)
        //{
        //    string[] files = Directory.GetFiles(startPath, "*.sync");
        //    StreamWriter sw = new StreamWriter(runningDir + "master.sync");

        //    foreach (string file in files)
        //    {
        //        sw.WriteLine(file.Substring(file.LastIndexOf("@")) + "=" + GenerateMD5(file));
        //    }

        //    sw.Close();

        //}

        //private void hashAddonFolders(string startPath)
        //{
        //    string[] dirs = Directory.GetDirectories(startPath, "@*");
        //    progressOverallAction.Maximum = dirs.Count();

        //    foreach (string s in dirs)
        //    {
        //        StreamWriter sw = new StreamWriter(runningDir + "Sync\\" + s.Substring(s.LastIndexOf("@")) + ".sync");
        //        string[] files = Directory.GetFiles(s, "*", SearchOption.AllDirectories);
        //        progressBar1.Maximum = files.Count();
        //        setAction("Hashing: " + s.Substring(s.LastIndexOf("@")));

        //        foreach (string file in files)
        //        {
        //            sw.WriteLine(file.Substring(file.LastIndexOf("@")) + "=" + GenerateMD5(file));
        //            progressBar1.PerformStep();
        //            Application.DoEvents();
        //        }

        //        progressBar1.Value = 1;
        //        sw.Close();
        //        progressOverallAction.PerformStep();

        //    }
        //}

    }
}
