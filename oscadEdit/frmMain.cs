using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using ScintillaNET;

namespace oscadEdit
{
    public partial class frmMain : Form
    {
        private string _filePath;
        private int zoom = 0;
        private string oscadPath = "";

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.sciEdit.Lexing.Lexer = ScintillaNET.Lexer.Progress;
            this.sciEdit.Lexing.LexerLanguageMap["oscad"] = "cpp";
            this.sciEdit.ConfigurationManager.CustomLocation = System.IO.Path.GetFullPath("oscad.xml");
            this.sciEdit.ConfigurationManager.Language = "oscad";
            this.sciEdit.ConfigurationManager.Configure();
            this.sciEdit.Margins.Margin0.Width = 35;

            zoom = Properties.Settings.Default.ZOOM;
            sciEdit.Zoom = zoom;
            this.Size = Properties.Settings.Default.FormSize;
            this.Location = Properties.Settings.Default.FormLocation;
            oscadPath = Properties.Settings.Default.oscadPath;

            whitespaceToolStripMenuItem.Checked = Properties.Settings.Default.Whitespace;
            wordWrapToolStripMenuItem.Checked = Properties.Settings.Default.WordWrap;
            lineNumbersToolStripMenuItem.Checked = Properties.Settings.Default.LineNumbers;
            endOfLineToolStripMenuItem.Checked = Properties.Settings.Default.EOL;

            this.sciEdit.Margins.Margin0.Width = lineNumbersToolStripMenuItem.Checked ? 35 : 0;
            this.sciEdit.Margins.Margin2.Width = 16;

            sciEdit.Whitespace.Mode = whitespaceToolStripMenuItem.Checked ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
            sciEdit.LineWrapping.Mode = wordWrapToolStripMenuItem.Checked ? LineWrappingMode.Word : LineWrappingMode.None;
            sciEdit.EndOfLine.IsVisible = endOfLineToolStripMenuItem.Checked;
            sciEdit_DocumentChange(sender, null);
            sciEdit.Printing.PrintDocument.DocumentName = "SCAD-File";

            sciEdit.Folding.MarkerScheme = FoldMarkerScheme.BoxPlusMinus;
            sciEdit.Folding.IsEnabled = true;

            openFileDialog.InitialDirectory = Properties.Settings.Default.OpenFileDialogPath;
            saveFileDialog.InitialDirectory = Properties.Settings.Default.SaveFileDialogPath;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Clipboard.Copy();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Clipboard.Cut();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Clipboard.Paste();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.UndoRedo.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.UndoRedo.Redo();
        }

        private void selectallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Selection.SelectAll();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.FindReplace.ShowFind();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.FindReplace.ShowReplace();
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.GoTo.ShowGoToDialog();
        }

        private void toggleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Line currentLine = sciEdit.Lines.Current;
            if (sciEdit.Markers.GetMarkerMask(currentLine) == 0)
            {
                currentLine.AddMarker(0);
            }
            else
            {
                currentLine.DeleteMarker(0);
            }            
        }

        private void previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Line l = sciEdit.Lines.Current.FindPreviousMarker(1);
            if (l != null)
                l.Goto();
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Line l = sciEdit.Lines.Current.FindNextMarker(1);
            if (l != null)
                l.Goto();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Markers.DeleteAll();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Printing.Print();
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Printing.PrintPreview();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public bool Save()
        {
            if (String.IsNullOrEmpty(_filePath))
                return SaveAs();

            return Save(_filePath);
        }


        public bool Save(string filePath)
        {
            using (FileStream fs = File.Create(filePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
                bw.Write(sciEdit.RawText, 0, sciEdit.RawText.Length - 1); // Omit trailing NULL

            sciEdit.Modified = false;
            return true;
        }


        public bool SaveAs()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _filePath = saveFileDialog.FileName;
                Text = "OpenSCAD Editor - <" + _filePath + ">";
                return Save(_filePath);
            }

            return false;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sciEdit.Modified)
            {
                // Prompt if not saved
                string message = String.Format(
                    CultureInfo.CurrentCulture,
                    "The text in the {0} file has changed.{1}{2}Do you want to save the changes?",
                    Text.TrimEnd(' ', '*'),
                    Environment.NewLine,
                    Environment.NewLine);

                DialogResult dr = MessageBox.Show(this, message, "openScad - Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (dr == DialogResult.Cancel)
                {
                    // Stop closing
                    e.Cancel = true;
                    return;
                }
                else if (dr == DialogResult.Yes)
                {
                    // Try to save before closing
                    e.Cancel = !Save();
                    return;
                }
            }
            // Close as normal

            Properties.Settings.Default.ZOOM = zoom;
            Properties.Settings.Default.FormSize = this.Size;
            Properties.Settings.Default.FormLocation = this.Location;
            Properties.Settings.Default.oscadPath = oscadPath;

            Properties.Settings.Default.Whitespace = whitespaceToolStripMenuItem.Checked;
            Properties.Settings.Default.WordWrap = wordWrapToolStripMenuItem.Checked;
            Properties.Settings.Default.LineNumbers = lineNumbersToolStripMenuItem.Checked;
            Properties.Settings.Default.EOL = endOfLineToolStripMenuItem.Checked;

            Properties.Settings.Default.OpenFileDialogPath = openFileDialog.InitialDirectory;
            Properties.Settings.Default.SaveFileDialogPath = saveFileDialog.InitialDirectory;

            Properties.Settings.Default.Save();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sciEdit.Text = "";
            sciEdit.UndoRedo.EmptyUndoBuffer();
            sciEdit.Modified = false;
            Text = "OpenSCAD Editor - <untitled>";
        }

        private void OpenFile()
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            OpenFile(openFileDialog.FileName);
        }


        public bool ExportAsHtml()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                string fileName = (Text.EndsWith(" *") ? Text.Substring(0, Text.Length - 2) : Text);
                dialog.Filter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";
                dialog.FileName = fileName + ".html";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sciEdit.Lexing.Colorize(); // Make sure the document is current
                    using (StreamWriter sw = new StreamWriter(dialog.FileName))
                        sciEdit.ExportHtml(sw, fileName, false);

                    return true;
                }
            }

            return false;
        }

        private void OpenFile(string filePath)
        {
            sciEdit.Text = File.ReadAllText(filePath);
            sciEdit.UndoRedo.EmptyUndoBuffer();
            sciEdit.Modified = false;
            _filePath = filePath;
            Text = "OpenSCAD Editor - <" + _filePath + ">";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void exportAsHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAsHtml();
        }

        private void lineNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lineNumbersToolStripMenuItem.Checked = !lineNumbersToolStripMenuItem.Checked;
            this.sciEdit.Margins.Margin0.Width = lineNumbersToolStripMenuItem.Checked ? 35 : 0;
        }

        private void whitespaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            whitespaceToolStripMenuItem.Checked = !whitespaceToolStripMenuItem.Checked;
            sciEdit.Whitespace.Mode = whitespaceToolStripMenuItem.Checked ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wordWrapToolStripMenuItem.Checked = !wordWrapToolStripMenuItem.Checked;
            sciEdit.LineWrapping.Mode = wordWrapToolStripMenuItem.Checked ? LineWrappingMode.Word : LineWrappingMode.None;
        }

        private void endOfLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            endOfLineToolStripMenuItem.Checked = !endOfLineToolStripMenuItem.Checked;
            sciEdit.EndOfLine.IsVisible = endOfLineToolStripMenuItem.Checked;
        }

        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zoom = 0;
            sciEdit.Zoom = zoom;
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zoom++;
            sciEdit.Zoom = zoom;                 
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            zoom--;
            sciEdit.Zoom = zoom;                 
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAboutbox ab = new frmAboutbox();
            ab.ShowDialog();
            ab.Dispose();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void sciEdit_LocationChanged(object sender, EventArgs e)
        {
        }

        private void sciEdit_CursorChanged(object sender, EventArgs e)
        {
        }

        private void sciEdit_DocumentChange(object sender, NativeScintillaEventArgs e)
        {
            PosMarker.Text = "Line: "+(sciEdit.Lines.Current.Number+1).ToString() + " Column: " + (sciEdit.GetColumn(sciEdit.CurrentPos)+1);
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            ReloadCache();
        }

        private void ReloadCache()
        {
            if (oscadPath != "")
            {
                frmProgress pg = new frmProgress();
                pg.oscadPath = oscadPath+"\\libraries";
                pg.ShowDialog();
                // ph.Modules => Lexer
                foreach (recmodule aModule in pg.Modules)
                {
                    if (!sciEdit.AutoComplete.List.Contains(aModule.name))
                    {
                        sciEdit.AutoComplete.List.Add(aModule.name);
                        if (aModule.param.Count > 0)
                        {
                            // produce auto code snippet
                            string aPs = "";
                            for (int i = 0; i < aModule.param.Count; i++)
                            {
                                string aP = aModule.param[i].Trim();
                                if (aPs != "") aPs += ", ";
                                string aVS = aModule.value[i].Trim();
                                if (aVS == "") aVS = aP;
                                aPs += aP + "=$" + aVS + "$";
                            }
                            string aSnippet = aModule.name + "(" + aPs + ");";
                            Snippet aSnippetObj = new Snippet(aModule.name, aSnippet);
                            sciEdit.Snippets.List.Add(aSnippetObj);
                        }
                    }
                }
                sciEdit.AutoComplete.List.Sort();
                sciEdit.Snippets.List.Sort();
                pg.Dispose();
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadCache();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tssTime.Text = DateTime.Now.ToLongTimeString() + ", " + DateTime.Now.ToLongDateString();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = oscadPath;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                oscadPath = folderBrowserDialog1.SelectedPath;
                ReloadCache();
            }
        }

        private void startOpenSCADToolStripMenuItem_Click(object sender, EventArgs e)        
        {
            if (oscadPath == "")
            {
                MessageBox.Show("Please select first the location of your openscad.exe");
                return;
            }
            string aExe = Path.Combine(oscadPath, "openscad.exe");
            if (!File.Exists(aExe)) return;
            Process.Start(aExe, _filePath);
        }
    }
}
