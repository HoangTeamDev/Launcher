using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SFB;
public class Launcher : MonoBehaviour
{
    [Header("UI")]
    public Button downloadButton;
    public Button playButton;
    public Slider progressBar;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI pathText; 
    public TextMeshProUGUI textPercent;
    private bool isDownloading = false;

    [Header("Config")]
    public string downloadUrl = "http://192.168.1.8/Game/NinjaHuyenThoai.zip";
    public string exeName = "NinjaHuyenThoai.exe";

    private string zipPath;
    private string extractPath;
    private WebClient client;

    void Start()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "install_path.txt");

       
        if (!File.Exists(configPath))
        {
         
            extractPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Game");

           
            File.WriteAllText(configPath, extractPath);
            UnityEngine.Debug.Log("📝 Đã tạo file cấu hình install_path.txt tại: " + configPath);
        }
        else
        {
            
            extractPath = File.ReadAllText(configPath);
        }

       
        string exePath = Path.Combine(extractPath, exeName);

        if (File.Exists(exePath))
        {
            statusText.text = "Play";
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayGame);
        }
        else
        {
            statusText.text = "Download";
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(ChooseFolder);
        }
    }


    void ChooseFolder()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Chọn nơi lưu game", "", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            
            string basePath = paths[0];          
            extractPath = Path.Combine(basePath, "NinjaHuyenThoai");         
            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);          
            zipPath = Path.Combine(extractPath, "Game.zip");       
           string configPath = Path.Combine(Application.persistentDataPath, "install_path.txt");
            File.WriteAllText(configPath, extractPath);

         
            StartDownload();
        }
    }




    void StartDownload()
    {
        if (string.IsNullOrEmpty(extractPath))
        {
            statusText.text = "⚠️ Hãy chọn thư mục lưu trước!";
            return;
        }

        if (File.Exists(zipPath)) File.Delete(zipPath);
        isDownloading = true;

        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

        client = new WebClient();
        client.DownloadProgressChanged += DownloadProgressChanged;
        client.DownloadFileCompleted += DownloadFileCompleted;
        progressBar.value = 0;
        playButton.interactable = false;
       

        client.DownloadFileAsync(new Uri(downloadUrl), zipPath);
        progressBar.gameObject.SetActive(true);
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        progressBar.value = e.ProgressPercentage / 100f;
        textPercent.text = $"Hoàn tất {e.ProgressPercentage}%";
        statusText.text = $"Downloading";
    }

    private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        isDownloading = false;
        if (e.Error != null)
        {
            statusText.text = "❌ Lỗi tải file: " + e.Error.Message;
            playButton.interactable = true;
            return;
        }

        ExtractZip();
    }

    void ExtractZip()
    {
        try
        {
            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));                   
                    if (!destinationPath.StartsWith(extractPath, StringComparison.OrdinalIgnoreCase))
                        continue;                 
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }                  
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));                    
                    entry.ExtractToFile(destinationPath, true);
                }
            }
            string configPath = Path.Combine(Application.persistentDataPath, "install_path.txt");
            File.WriteAllText(configPath, extractPath);
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            statusText.text = "Play";
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayGame);
            progressBar.value = 1;
            progressBar.gameObject.SetActive(false);
            playButton.interactable = true;
        }
        catch (Exception ex)
        {
            statusText.text = "❌ Lỗi khi giải nén: " + ex.Message;
            playButton.interactable = true;
        }
    }

    


    void PlayGame()
    {
        string exePath = Path.Combine(extractPath, exeName);

        if (!File.Exists(exePath))
        {
            
            var paths = StandaloneFileBrowser.OpenFolderPanel("Chọn lại thư mục game", "", false);
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                extractPath = paths[0];
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "install_path.txt"), extractPath);
                exePath = Path.Combine(extractPath, exeName);
            }

            if (!File.Exists(exePath))
            {
               
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = extractPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            statusText.text = "❌ Lỗi khi mở game: " + ex.Message;
        }
    }
    void OnApplicationQuit()
    {
        if (isDownloading && File.Exists(zipPath))
        {
            try
            {
                client?.CancelAsync(); 
                Thread.Sleep(200);     
                File.Delete(zipPath); 
               
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("⚠️ Không thể xóa file đang tải: " + ex.Message);
            }
        }
    }
    void Update()
    {
        CheckIfGameRunning();
    }

    void CheckIfGameRunning()
    {
        string processName = Path.GetFileNameWithoutExtension(exeName); // Ví dụ: "NinjaHuyenThoai"
        Process[] running = Process.GetProcessesByName(processName);

        if (running.Length > 0)
        {
           
            playButton.interactable = false;
            statusText.text = "Running...";
        }
        else
        {
            
            if (File.Exists(Path.Combine(extractPath, exeName)))
            {
                playButton.interactable = true;
                statusText.text = "Play";
            }
            else
            {
                playButton.interactable = true;
                statusText.text = "Download";
            }
        }
    }

}
