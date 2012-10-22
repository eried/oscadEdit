using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace oscadEdit
{
    public class recmodule
    {
        public string name = "";
        public List<string> param = new List<string>();
        public List<string> value = new List<string>();
    }

    public partial class frmProgress : Form
    {
        public String oscadPath { get; set; }

        public frmProgress()
        {
            InitializeComponent();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            Close();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        public List<recmodule> Modules = new List<recmodule>();

        private void ScanFile(string f)
        {
            string aContent = File.ReadAllText(f);
            // Find Module Declartions
            Regex aMods = new Regex(@"(module)(\s)+(?<modname>[\w\d_-]+)(\s)?(?<tx>[(]+)(?<param>[\w\s\d\,\=,\r\n\t\$]*)(?<ty>[)]+)");
            MatchCollection aMatches = aMods.Matches(aContent);
            foreach (Match aMatch in aMatches)
            {
                string aModName = (aMatch.Groups["modname"].Success) ? aMatch.Groups["modname"].Value : "";
                string aParams = (aMatch.Groups["param"].Success) ? aMatch.Groups["param"].Value : "";
                recmodule aModule = new recmodule();
                aModule.name = aModName;
                Modules.Add(aModule);
                if (aParams != "")
                {
                    if (aParams.Contains("["))
                    {
                        aModule.param.Add(aParams);
                    }
                    else
                    {
                        // ((?<param>[\w\s\d\=\$]+)([\,]?)*)*
                        Regex aParamsR = new Regex(@"((?<param>[\[\]\w\s\d\=\$]+)([\,]?)*)*");
                        MatchCollection aPMatches = aParamsR.Matches(aParams);
                        foreach (Match aPMatch in aPMatches)
                        {
                            // Parameter auswerten :-) irgendwas zwischen "value" or "value=3434" ...                        
                            string aValue = "";
                            foreach (Capture aCap in aPMatch.Groups["param"].Captures)
                            {
                                string aParam = aCap.Value;
                                if (aParam.Contains("="))
                                {
                                    // Mit Wert
                                    var aParts = aParam.Split('=');
                                    aParam = aParts[0].Trim();
                                    aValue = aParts[1];
                                }
                                else
                                {
                                    // Ohne Wert
                                    aValue = "";
                                }
                                aModule.param.Add(aParam);
                                // Values dürfen nur 1x vorkommen ?!?!
                                if ((aValue != "") && (aModule.value.Contains(aValue)))
                                {
                                    int i = 1;
                                    while (aModule.value.Contains(aValue + "_" + i)) i++;
                                    aModule.value.Add(aValue+"_"+i);
                                }
                                else
                                {
                                    aModule.value.Add(aValue);
                                }
                            }
                        }
                    }
                }
            }
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!Directory.Exists(oscadPath)) return;
            backgroundWorker1.ReportProgress(0);

            // Scan the oscad-path for files ...
            List<string> aFileList = new List<string>();
            foreach (var f in Directory.GetFiles(oscadPath, "*.scad", SearchOption.AllDirectories))
            {
                aFileList.Add(f);
            }
            int pg = 0;
            int lp = 0;
            foreach (var f in aFileList) {
                int np = (pg * 100) / aFileList.Count;
                if (np != lp) { backgroundWorker1.ReportProgress(np); lp = np; }
                ScanFile(f);
                pg++;
            }

            backgroundWorker1.ReportProgress(100);
        }

        private void frmProgress_Shown(object sender, EventArgs e)
        {
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
        }
    }
}
