
using UnityEngine;
using UnityEngine.UI;

public class ButtonPanel : MonoBehaviour
{
    public string links;
    public Image Image;
    public Button button;
   
    public void Init()
    {
        button.onClick.AddListener(() =>
        {
            Application.OpenURL($"{links}");
        });
    }
}
