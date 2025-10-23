using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

public class ScrollBanner : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public Button buttonLeft;
    public Button buttonRight;
    private float bannerWidth;
    [Header("Settings")]
    public float slideDuration = 0.5f;
    public float autoSlideDelay = 5f;
    public bool autoSlide = true;

    private int currentIndex = 0;
    private int bannerCount;  
    private Tween slideTween;
    public ButtonPanel ButtonPanel;
    public List<ButtonPanel> listbuton;
    void Init()
    {
        List<string> strings = new List<string>();
        List<string> strings1 = new List<string>();
        string bn12 = "https://genshin.hoyoverse.com/vi/home";
        string bn13 = "https://www.youtube.com/watch?v=gxlnhFx0JCs&list=RDgxlnhFx0JCs&start_radio=1";
        string bn14 = "https://ygoprodeck.com/card-database/?sort=new&sortorder=asc&num=24&offset=0";
        string img = "Event1";
        string img1 = "Event2";
        string img2 = "Event3";
        strings1.Add(img);
        strings1.Add(img1);
        strings1.Add(img2);
        strings.Add(bn12);
        strings.Add(bn13);
        strings.Add(bn14);
        for(int i = 0; i < strings.Count; i++)
        {
            CreateBanner(strings[i], strings1[i]);
        }
        bannerCount = strings.Count;
        bannerWidth = ((RectTransform)content.GetChild(0)).rect.width;
        buttonLeft.onClick.AddListener(PreviousBanner);
        buttonRight.onClick.AddListener(NextBanner);

        if (autoSlide)
            InvokeRepeating(nameof(NextBanner), autoSlideDelay, autoSlideDelay);
    }
    private void Start()
    {
        Init();
    }
    public async void CreateBanner(string list, string imagename)
    {
        Debug.Log("aaa" + imagename);
        var banner = Instantiate(ButtonPanel, content);
        listbuton.Add(banner);
        ButtonPanel buttonPanel=banner.GetComponent<ButtonPanel>();
        buttonPanel.gameObject.SetActive(true);
        buttonPanel.Image.sprite= await AddressablesManager.Instance.LoadAssetAsync<Sprite>($"{imagename}");
        buttonPanel.links = list;
        buttonPanel.Init();
    }
    void NextBanner()
    {
        if (slideTween != null && slideTween.IsActive()) slideTween.Kill();

        currentIndex++;
        if (currentIndex >= bannerCount)
            currentIndex = 0;

        MoveToCurrentBanner();
    }

    void PreviousBanner()
    {
        if (slideTween != null && slideTween.IsActive()) slideTween.Kill();

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = bannerCount - 1;

        MoveToCurrentBanner();
    }

    void MoveToCurrentBanner()
    {
        float targetNormalizedPos = (float)currentIndex / (bannerCount - 1);
        slideTween = DOTween.To(() => scrollRect.horizontalNormalizedPosition,
                                x => scrollRect.horizontalNormalizedPosition = x,
                                targetNormalizedPos,
                                slideDuration)
                            .SetEase(Ease.InOutQuad);
    }
}
