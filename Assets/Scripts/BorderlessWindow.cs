using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Runtime.InteropServices;

public class BorderlessLauncher : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    const int GWL_STYLE = -16;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_POPUP = 0x80000000;
    const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

    enum DwmWindowCornerPreference
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref DwmWindowCornerPreference attrValue, int attrSize);

    const int WM_NCLBUTTONDOWN = 0xA1;
    const int HTCAPTION = 0x2;

    private IntPtr handle;

    void Start()
    {
        handle = GetActiveWindow();

        // Ẩn thanh tiêu đề
        SetWindowLong(handle, GWL_STYLE, WS_VISIBLE | WS_POPUP);

        // Bo góc cửa sổ (Windows 11)
        Invoke(nameof(ApplyRoundedCorners), 0.1f);
    }
    void ApplyRoundedCorners()
    {
        IntPtr handle = GetActiveWindow();
        var preference = DwmWindowCornerPreference.Round;
        DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
    }
    public void DragWindow()
    {
        ReleaseCapture();
        SendMessage(handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
    }

    public void CloseApp()
    {
        Application.Quit();
    }
#endif

}
