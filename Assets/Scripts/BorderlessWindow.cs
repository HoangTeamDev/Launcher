using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

public class BorderlessLauncher : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    // Các hằng số WinAPI
    const int GWL_STYLE = -16;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_POPUP = 0x80000000;
    const uint WS_CLIPSIBLINGS = 0x04000000;
    const uint WS_CLIPCHILDREN = 0x02000000;

    const int WM_NCLBUTTONDOWN = 0xA1;
    const int HTCAPTION = 0x2;

    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_SHOWWINDOW = 0x0040;

    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] static extern bool SetFocus(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private IntPtr handle;

    public void StartA()
    {
        handle = GetActiveWindow();

        // Giữ lại flag cần thiết (ẩn thanh tiêu đề nhưng không lỗi UI)
        uint style = WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | WS_POPUP;
        SetWindowLong(handle, GWL_STYLE, style);

        // Đảm bảo nhận focus và click đầu tiên ngay
        BringWindowToTop(handle);
        SetForegroundWindow(handle);
        SetFocus(handle);
        Canvas.ForceUpdateCanvases();
        // Dịch cửa sổ sang trái một chút (50 pixel)
        if (GetWindowRect(handle, out RECT rect))
        {
            int offset = 50; // khoảng cách dịch sang trái
            int newX = rect.Left - offset;
            int newY = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            SetWindowPos(handle, IntPtr.Zero, newX, newY, width, height, SWP_NOZORDER | SWP_SHOWWINDOW);
        }
    }

    // Cho phép kéo cửa sổ (chỉ khi không bấm UI)
    public void TryDragWindow()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            DragWindow();
        }
    }

    private void DragWindow()
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
