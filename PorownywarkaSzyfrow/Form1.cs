using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PorownywarkaSzyfrow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if (cmbAlgorithm.Items.Count == 0)
            {
                cmbAlgorithm.Items.Add("Vigenere");
                cmbAlgorithm.Items.Add("DES");
                cmbAlgorithm.Items.Add("RC2");
            }
            cmbAlgorithm.SelectedIndex = 0;

            txtInputFile.AllowDrop = true;
            txtInputFile.DragEnter += TxtInputFile_DragEnter;
            txtInputFile.DragDrop += TxtInputFile_DragDrop;

            lblStatus.Text = string.Empty;

            btnBrowseInput.Click += btnBrowseInput_Click;
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            btnEncrypt.Click += btnEncrypt_Click;
            btnDecrypt.Click += btnDecrypt_Click;

            btnCopyLog.Click += btnCopyLog_Click;
        }

        private void TxtInputFile_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TxtInputFile_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string filePath = files[0];
                txtInputFile.Text = filePath;

                lblStatus.Text = string.Empty;
                txtLog.Clear();

                if (string.IsNullOrWhiteSpace(txtOutputFile.Text))
                {
                    string dir = Path.GetDirectoryName(filePath) ?? "";
                    string name = Path.GetFileName(filePath);

                    if (name.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                        name = name[..^4];
                    else
                        name += ".enc";

                    txtOutputFile.Text = Path.Combine(dir, name);
                }
            }
        }

        private void btnBrowseInput_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Wybierz plik wejściowy",
                Filter = "Wszystkie pliki|*.*"
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                txtInputFile.Text = filePath;

                lblStatus.Text = string.Empty;
                txtLog.Clear();

                if (string.IsNullOrWhiteSpace(txtOutputFile.Text))
                {
                    string dir = Path.GetDirectoryName(filePath) ?? "";
                    string name = Path.GetFileName(filePath);

                    if (name.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                        name = name[..^4];
                    else
                        name += ".enc";

                    txtOutputFile.Text = Path.Combine(dir, name);
                }
            }
        }

        private void btnBrowseOutput_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "Wybierz plik wynikowy",
                Filter = "Wszystkie pliki|*.*",
                FileName = txtOutputFile.Text
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputFile.Text = sfd.FileName;
                lblStatus.Text = string.Empty;
            }
        }

        private void btnEncrypt_Click(object? sender, EventArgs e)
        {
            RunCrypto(isEncrypt: true);
        }

        private void btnDecrypt_Click(object? sender, EventArgs e)
        {
            RunCrypto(isEncrypt: false);
        }

        private void btnCopyLog_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtLog.Text))
            {
                Clipboard.SetText(txtLog.Text);
            }
        }

        private void RunCrypto(bool isEncrypt)
        {
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                lblStatus.Text = "Pracuję...";
                Cursor = Cursors.WaitCursor;

                string inputPath = txtInputFile.Text.Trim();
                string outputPath = txtOutputFile.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                {
                    MessageBox.Show(this, "Podaj istniejący plik wejściowy.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Błąd";
                    return;
                }

                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    MessageBox.Show(this, "Podaj ścieżkę pliku wynikowego.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Błąd";
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show(this, "Hasło nie może być puste.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Błąd";
                    return;
                }

                bool inputIsEnc = inputPath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase);

                if (isEncrypt && inputIsEnc)
                {
                    MessageBox.Show(this, "Ten plik jest już zaszyfrowany (.enc).", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Błąd";
                    return;
                }

                if (!isEncrypt && !inputIsEnc)
                {
                    MessageBox.Show(this, "Ten plik nie jest zaszyfrowany (.enc).", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Błąd";
                    return;
                }

                stopwatch.Start();

                AlgoId algoUsed;

                if (isEncrypt)
                {
                    algoUsed = GetSelectedAlgo();
                    CryptoCore.EncryptFile(inputPath, outputPath, password, algoUsed);
                }
                else
                {
                    algoUsed = CryptoCore.DecryptFile(inputPath, outputPath, password);
                }

                stopwatch.Stop();

                string operacja = isEncrypt ? "zaszyfrowania" : "odszyfrowania";
                string line = $"Algorytm: {algoUsed}, czas {operacja}: {stopwatch.Elapsed.TotalMilliseconds:0.00} ms";

                if (txtLog.Text.Length > 0)
                    txtLog.AppendText(Environment.NewLine);

                txtLog.AppendText(line);

                lblStatus.Text = "Gotowe";

                txtInputFile.Clear();
                txtOutputFile.Clear();
                txtPassword.Clear();
            }
            catch
            {
                stopwatch.Stop();
                MessageBox.Show(this, "Operacja nie powiodła się.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);

                lblStatus.Text = "Błąd";

                txtInputFile.Clear();
                txtOutputFile.Clear();
                txtPassword.Clear();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private AlgoId GetSelectedAlgo()
        {
            return cmbAlgorithm.SelectedIndex switch
            {
                0 => AlgoId.Vigenere,
                1 => AlgoId.DES,
                2 => AlgoId.RC2,
                _ => throw new InvalidOperationException("Nie wybrano algorytmu.")
            };
        }
    }
}
