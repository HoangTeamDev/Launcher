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
using UnityEngine.Rendering;

public class Launcher : MonoBehaviour
{
    [Header("UI")]
    public Button playButton;
    public Slider progressBar;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI textPercent;
    private bool isDownloading = false;
    public InstallPathPanel installPanel;
    public BorderlessLauncher borderlessLauncher;
    [Header("Config")]
    public string downloadUrl = "http://192.168.1.26/Game/NinjaHuyenThoai.zip";
    public string exeName = "NinjaHuyenThoai.exe";

    // 🔹 Link version file trên server
    public string versionUrl = "http://192.168.1.26/Game/version.txt";

    public string zipPath;
    public string extractPath;
    private WebClient client;

    private string localVersionPath => Path.Combine(extractPath, "version.txt");
    public string localVersion = "0.0.0";
    public string serverVersion = "0.0.0";
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.SetResolution(1280, 720, fullscreenMode: FullScreenMode.Windowed);
    }
    void Start()
    {
        borderlessLauncher.StartA();
      
      
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

        // 🔹 Kiểm tra version
        CheckVersion();
        playButton.interactable = true;
    }

    async void CheckVersion()
    {
        string exePath = Path.Combine(extractPath, exeName);

        // 🔹 Đọc version cục bộ (nếu có)
        if (File.Exists(localVersionPath))
            localVersion = File.ReadAllText(localVersionPath).Trim();

        // 🔹 Tải version từ server
        using (WebClient wc = new WebClient())
        {
            try
            {
                serverVersion = wc.DownloadString(versionUrl).Trim();
               
            }
            catch
            {
                UnityEngine.Debug.Log("⚠️ Không thể kiểm tra version.");
                serverVersion = localVersion; 
            }
        }

        // 🔹 So sánh version
        if (!File.Exists(exePath))
        {
            statusText.text = "Download";
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => installPanel.Open(extractPath));
        }
        else if (serverVersion != localVersion)
        {
            statusText.text = $"Update ({localVersion} → {serverVersion})";
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartDownloadAtPath); // Dùng lại đường dẫn cũ, không mở panel chọn lại
        }

        else
        {
            statusText.text = "Play";
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayGame);
        }
    }

    public void StartDownloadAtPath()
    {
        if (!Directory.Exists(extractPath))
            Directory.CreateDirectory(extractPath);
        zipPath = Path.Combine(extractPath, "Game.zip");
        string configPath = Path.Combine(Application.persistentDataPath, "install_path.txt");
        File.WriteAllText(configPath, extractPath);
        StartDownload();
    }

    void StartDownload()
    {
        if (string.IsNullOrEmpty(extractPath))
        {
            statusText.text = "⚠️ Hãy chọn thư mục lưu trước!";
            return;
        }
        using (WebClient wc = new WebClient())
        {
            try
            {
                serverVersion = wc.DownloadString(versionUrl).Trim();
                localVersion = serverVersion;
            }
            catch
            {
                UnityEngine.Debug.Log("⚠️ Không thể kiểm tra version.");
                serverVersion = localVersion;
            }
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

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // 🔹 Lưu version mới
            File.WriteAllText(localVersionPath, serverVersion);

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
            return;

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

    void LateUpdate()
    {
        CheckIfGameRunning();
    }

    void CheckIfGameRunning()
    {
        string processName = Path.GetFileNameWithoutExtension(exeName);
        Process[] running = Process.GetProcessesByName(processName);

        if (running.Length > 0)
        {
            playButton.interactable = false;
            statusText.text = "Running...";
            return;
        }
        if (File.Exists(Path.Combine(extractPath, exeName)))
        {
            if(serverVersion == localVersion)
            {
                playButton.interactable = true;
                statusText.text = "Play";
            }
            else
            {
                playButton.interactable = true;
                statusText.text = "Update";
            }
           
        }
        else
        {
            playButton.interactable = true;
            statusText.text = "Download";
        }
    }
}
