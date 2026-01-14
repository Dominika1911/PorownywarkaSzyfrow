using System;
using System.IO;
using System.Windows.Forms;

namespace PorownywarkaSzyfrow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Inicjalizacja comboboxa, jeśli nie zrobiłeś tego w designerze
            if (cmbAlgorithm.Items.Count == 0)
            {
                cmbAlgorithm.Items.Add("Vigenere");
                cmbAlgorithm.Items.Add("DES");
                cmbAlgorithm.Items.Add("RC2");
            }
            cmbAlgorithm.SelectedIndex = 0;

            // Drag & Drop dla pola pliku wejściowego
            txtInputFile.AllowDrop = true;
            txtInputFile.DragEnter += TxtInputFile_DragEnter;
            txtInputFile.DragDrop += TxtInputFile_DragDrop;

            // Na początku brak napisu
            lblStatus.Text = string.Empty;

            // Podpięcie zdarzeń przycisków (jeśli nie zrobiłeś tego w designerze)
            btnBrowseInput.Click += btnBrowseInput_Click;
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            btnEncrypt.Click += btnEncrypt_Click;
            btnDecrypt.Click += btnDecrypt_Click;
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
                txtInputFile.Text = files[0];

                // przy wyborze nowego pliku status znika
                lblStatus.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(txtOutputFile.Text))
                {
                    var dir = Path.GetDirectoryName(files[0]) ?? "";
                    var name = Path.GetFileName(files[0]);
                    txtOutputFile.Text = Path.Combine(dir, name + ".enc");
                }
            }
        }

        private void btnBrowseInput_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Wybierz plik wejściowy",
                Filter = "Wszystkie pliki|*.*"
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                txtInputFile.Text = ofd.FileName;

                // przy wyborze nowego pliku status znika
                lblStatus.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(txtOutputFile.Text))
                {
                    var dir = Path.GetDirectoryName(ofd.FileName) ?? "";
                    var name = Path.GetFileName(ofd.FileName);
                    txtOutputFile.Text = Path.Combine(dir, name + ".enc");
                }
            }
        }

        private void btnBrowseOutput_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Wybierz plik wynikowy",
                Filter = "Wszystkie pliki|*.*",
                FileName = txtOutputFile.Text
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputFile.Text = sfd.FileName;

                // zmiana pliku wynikowego też może czyścić status
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

        private void RunCrypto(bool isEncrypt)
        {
            try
            {
                lblStatus.Text = "Pracuję...";
                Cursor = Cursors.WaitCursor;

                string inputPath = txtInputFile.Text.Trim();
                string outputPath = txtOutputFile.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                    throw new InvalidOperationException("Podaj istniejący plik wejściowy.");

                if (string.IsNullOrWhiteSpace(outputPath))
                    throw new InvalidOperationException("Podaj ścieżkę pliku wynikowego.");

                if (string.IsNullOrEmpty(password))
                    throw new InvalidOperationException("Hasło nie może być puste.");

                if (isEncrypt)
                {
                    var algo = GetSelectedAlgo();
                    CryptoCore.EncryptFile(inputPath, outputPath, password, algo);
                }
                else
                {
                    CryptoCore.DecryptFile(inputPath, outputPath, password);
                }

                // Po udanej operacji:
                // - status = "Gotowy"
                // - czyścimy pola, żeby przygotować do kolejnej operacji
                lblStatus.Text = "Gotowy";

                txtInputFile.Clear();
                txtOutputFile.Clear();
                txtPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Błąd: " + ex.Message;
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
