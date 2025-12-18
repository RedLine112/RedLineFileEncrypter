using System;
using System.IO;
using System.IO.Compression; // Required for compression!
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        string currentPath = "";
        bool isFolder = false;

        public MainWindow()
        {
            InitializeComponent();
            ApplyLanguage("EN");
        }

        // --- DRAG & DROP LOGIC ---
        private void dropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                currentPath = files[0]; // Get the first item

                // Check if it is a folder or file
                FileAttributes attr = File.GetAttributes(currentPath);
                isFolder = (attr & FileAttributes.Directory) == FileAttributes.Directory;

                txtDropInfo.Text = isFolder ? "FOLDER DETECTED" : "FILE DETECTED";
                txtSelectedPath.Text = currentPath;
                txtStatus.Text = "Target locked. Waiting for action...";
            }
        }

        // --- ENCRYPTION (WinRAR Mode) ---
        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentPath) || string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Please drag a file/folder and enter a password!");
                return;
            }

            try
            {
                txtStatus.Text = "Compressing and Encrypting...";

                // STEP 1: Convert everything to a temporary ZIP file first
                string tempZipPath = System.IO.Path.GetTempFileName();
                File.Delete(tempZipPath); // Delete empty file, ZipFile will create it

                if (isFolder)
                {
                    // Zip the entire directory
                    ZipFile.CreateFromDirectory(currentPath, tempZipPath);
                }
                else
                {
                    // Zip even if it's a single file (For compression benefit)
                    using (ZipArchive zip = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(currentPath, System.IO.Path.GetFileName(currentPath));
                    }
                }

                // STEP 2: Read bytes of the Zip file
                byte[] zipBytes = File.ReadAllBytes(tempZipPath);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(txtPassword.Password);
                string algoType = (cmbAlgo.SelectedItem as ComboBoxItem).Content.ToString();

                // STEP 3: Encrypt the Zip bytes
                byte[] encryptedBytes = Universal_Logic(zipBytes, passwordBytes, true, algoType);

                // STEP 4: Save as .redline
                string outputName = isFolder ? new DirectoryInfo(currentPath).Name : System.IO.Path.GetFileName(currentPath);
                string savePath = System.IO.Path.GetDirectoryName(currentPath) + "\\" + outputName + ".redline.encryption";

                File.WriteAllBytes(savePath, encryptedBytes);

                // Cleanup
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);

                txtStatus.Text = "✅ OPERATION COMPLETE!";
                MessageBox.Show($"Target compressed and encrypted.\nSaved to: {savePath}", "REDLINE ARCHIVER");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // --- DECRYPTION (Unrar Mode) ---
        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentPath) || !currentPath.EndsWith(".redline.encryption"))
            {
                MessageBox.Show("Please drag a valid .redline file!");
                return;
            }

            try
            {
                txtStatus.Text = "Decrypting and Extracting...";

                byte[] fileBytes = File.ReadAllBytes(currentPath);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(txtPassword.Password);
                string algoType = (cmbAlgo.SelectedItem as ComboBoxItem).Content.ToString();

                // STEP 1: Decrypt -> Returns ZIP bytes
                byte[] zipBytes = Universal_Logic(fileBytes, passwordBytes, false, algoType);

                // STEP 2: Write bytes to a temporary zip file
                string tempZipPath = System.IO.Path.GetTempFileName();
                File.WriteAllBytes(tempZipPath, zipBytes);

                // STEP 3: Extract Zip to folder
                string outputFolder = currentPath.Replace(".redline.encryption", "") + "_Decrypted";

                // Avoid collision if folder exists
                if (Directory.Exists(outputFolder)) outputFolder += "_" + DateTime.Now.Ticks;
                Directory.CreateDirectory(outputFolder);

                ZipFile.ExtractToDirectory(tempZipPath, outputFolder);

                // Cleanup
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);

                txtStatus.Text = "🔓 SUCCESS!";
                MessageBox.Show($"Files extracted to:\n{outputFolder}", "REDLINE ARCHIVER");
            }
            catch (Exception)
            {
                MessageBox.Show("Incorrect password or corrupted file!", "Error");
            }
        }

        // --- DİL YÖNETİMİ (Bu bloğu class'ın içine ekle) ---

        // 1. DİL SÖZLÜĞÜ: Tüm metinlerin 3 dilde karşılığı burada
        private Dictionary<string, Dictionary<string, string>> LanguageDict = new Dictionary<string, Dictionary<string, string>>()
{
    { "TR", new Dictionary<string, string>() {
        { "Subtitle", "Sıkıştırılmış Klasör Şifreleme Sistemi" },
        { "DropInfo", "DOSYA VEYA KLASÖRÜ BURAYA SÜRÜKLE" },
        { "Engine", "Şifreleme Motoru:" },
        { "Password", "Şifre Belirle:" },
        { "BtnEncrypt", "🔒 SIKIŞTIR & KİLİTLE" },
        { "BtnDecrypt", "🔓 KİLİDİ AÇ & ÇIKART" },
        { "StatusWait", "Hedef kilitlendi. İşlem bekleniyor..." },
        { "StatusReady", "Hazır." }
    }},
    { "EN", new Dictionary<string, string>() {
        { "Subtitle", "Compressed Folder Encryption System" },
        { "DropInfo", "DRAG FILE OR FOLDER HERE" },
        { "Engine", "Encryption Engine:" },
        { "Password", "Set Password:" },
        { "BtnEncrypt", "🔒 COMPRESS & LOCK" },
        { "BtnDecrypt", "🔓 UNLOCK & EXTRACT" },
        { "StatusWait", "Target locked. Waiting for action..." },
        { "StatusReady", "Ready." }
    }},
    { "RU", new Dictionary<string, string>() {
        { "Subtitle", "Система шифрования сжатых папок" },
        { "DropInfo", "ПЕРЕТАЩИТЕ ФАЙЛ ИЛИ ПАПКУ СЮДА" },
        { "Engine", "Механизм шифрования:" },
        { "Password", "Установить пароль:" },
        { "BtnEncrypt", "🔒 СЖАТЬ И ЗАБЛОКИРОВАТЬ" },
        { "BtnDecrypt", "🔓 РАЗБЛОКИРОВАТЬ И ИЗВЛЕЧЬ" },
        { "StatusWait", "Цель захвачена. Ожидание действия..." },
        { "StatusReady", "Готовый." }
    }}
};

        // Varsayılan dil
        string currentLang = "EN";

        // 2. TIKLAMA OLAYI: RU, EN veya TR yazısına basınca çalışır
        private void ChangeLanguage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Tıklanan TextBlock'un "Tag" özelliğinden dili al (RU, EN, TR)
            string selectedLang = (sender as TextBlock).Tag.ToString();
            currentLang = selectedLang;
            ApplyLanguage(selectedLang);
        }

        // 3. DİLİ UYGULA: Arayüzdeki metinleri değiştirir
        private void ApplyLanguage(string lang)
        {
            if (!LanguageDict.ContainsKey(lang)) return;

            var dict = LanguageDict[lang];

            lblSubtitle.Text = dict["Subtitle"];
            txtDropInfo.Text = dict["DropInfo"];
            lblEngine.Text = dict["Engine"];
            lblPassword.Text = dict["Password"];
            btnEncrypt.Content = dict["BtnEncrypt"];
            btnDecrypt.Content = dict["BtnDecrypt"];
            txtStatus.Text = dict["StatusReady"];
        }

        // --- ENCRYPTION ENGINE ---
        public byte[] Universal_Logic(byte[] inputBytes, byte[] passBytes, bool isEncrypt, string algoName)
        {
            // --- OUR CUSTOM ALGORITHM ---
            if (algoName.Contains("XOR") || algoName.Contains("REDLINE"))
            {
                // IronWall expects string password
                string passwordString = Encoding.UTF8.GetString(passBytes);

                if (isEncrypt)
                {
                    return IronWallCipher.Encrypt(inputBytes, passwordString);
                }
                else
                {
                    return IronWallCipher.Decrypt(inputBytes, passwordString);
                }
            }
            // -------------------------------

            // Standard Algorithms
            byte[] resultBytes = null;
            byte[] salt = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2 };

            SymmetricAlgorithm algorithm = null;
            if (algoName.Contains("AES")) algorithm = Aes.Create();
            else if (algoName.Contains("TripleDES")) algorithm = TripleDES.Create();
            else if (algoName.Contains("DES")) algorithm = DES.Create();
            else if (algoName.Contains("RC2")) algorithm = RC2.Create();
            else algorithm = Aes.Create();

            using (MemoryStream ms = new MemoryStream())
            {
                using (algorithm)
                {
                    var keyGenerator = new Rfc2898DeriveBytes(passBytes, salt, 1000);
                    algorithm.Key = keyGenerator.GetBytes(algorithm.KeySize / 8);
                    algorithm.IV = keyGenerator.GetBytes(algorithm.BlockSize / 8);
                    algorithm.Mode = CipherMode.CBC;

                    ICryptoTransform transform = isEncrypt ? algorithm.CreateEncryptor() : algorithm.CreateDecryptor();
                    using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.Close();
                    }
                    resultBytes = ms.ToArray();
                }
            }
            return resultBytes;
        }
    }
}