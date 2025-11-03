using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class BorderlessLauncher : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    const int GWL_STYLE = -16;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_POPUP = 0x80000000;

    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    const int WM_NCLBUTTONDOWN = 0xA1;
    const int HTCAPTION = 0x2;

    private IntPtr handle;

   public void StartA()
    {
        handle = GetActiveWindow();

        // Ẩn thanh tiêu đề (chỉ giữ nội dung cửa sổ)
        SetWindowLong(handle, GWL_STYLE, WS_VISIBLE | WS_POPUP);
    }

    // Cho phép kéo cửa sổ khi giữ chuột
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
