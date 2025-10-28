using SFB; // StandaloneFileBrowser
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstallPathPanel : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI pathInput;
    public TextMeshProUGUI spaceInfoText;
    public TextMeshProUGUI warningText;
    public Button changeButton;
    public Button installButton;
    public Button cancelButton;
    public Launcher launcher;
    [Header("Config")]
    public float requiredSpaceGB = 5f; // dung lượng cần thiết (game bạn có thể chỉnh lại)
    private string selectedPath;

    void Start()
    {
        gameObject.SetActive(false);
        changeButton.onClick.AddListener(OnChangeFolder);
        installButton.onClick.AddListener(OnStartInstall);
        cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Open(string defaultPath)
    {
        selectedPath = defaultPath;
        pathInput.text = selectedPath;
        gameObject.SetActive(true);
        UpdateDiskSpace();
    }

    void OnChangeFolder()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Install Folder", "", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedPath = paths[0];

            launcher.extractPath = Path.Combine(selectedPath, "NinjaHuyenThoai");
            if (!Directory.Exists(launcher.extractPath))
                Directory.CreateDirectory(launcher.extractPath);
            launcher.zipPath = Path.Combine(launcher.extractPath, "Game.zip");
            string configPath = Path.Combine(Application.persistentDataPath, "install_path.txt");
            File.WriteAllText(configPath, launcher.extractPath);
            pathInput.text = launcher.extractPath;
            UpdateDiskSpace();
        }
    }

    void UpdateDiskSpace()
    {
        string root = Path.GetPathRoot(selectedPath);
        DriveInfo drive = new DriveInfo(root);
        double freeGB = drive.AvailableFreeSpace / 1024f / 1024f / 1024f;
        spaceInfoText.text = $"Available Space: {freeGB:F2} GB";

        if (freeGB < requiredSpaceGB)
        {
            warningText.text = $"Insufficient disk space. Need {requiredSpaceGB:F2} GB, available {freeGB:F2} GB.";
            installButton.interactable = false;
        }
        else
        {
            warningText.text = "";
            installButton.interactable = true;
        }
    }

    void OnStartInstall()
    {
        if (!Directory.Exists(launcher.extractPath))
            Directory.CreateDirectory(launcher.extractPath);
       
        gameObject.SetActive(false);
        FindObjectOfType<Launcher>().StartDownloadAtPath();
    }
}
