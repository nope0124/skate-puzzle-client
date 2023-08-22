using UnityEngine;
using GoogleMobileAds.Api;

public class AdmobManager : MonoBehaviour
{
    
    private static GameObject AdmobManagerObject;
    private static AdmobManager singleton;
    private BannerView defaultBannerView;
    private InterstitialAd defaultInterstitialAd;

    public static AdmobManager Instance
    {
        get {
            if (singleton == null)
            {
                // AdmobManager作成
                AdmobManagerObject = new GameObject("AdmobManager");
                AdmobManagerObject.AddComponent<AdmobManager>();

                // 遷移先シーンでもオブジェクトを破棄しない。
                DontDestroyOnLoad(AdmobManagerObject);

                singleton = AdmobManagerObject.GetComponent<AdmobManager>();
            }
            return singleton;
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => { });
    }

    public void RequestDefaultBanner(string position)
    {
        #if UNITY_IOS
            string adUnitId = Const.CO.IPHONE_DEFAULT_BANNER;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        switch (position) {
            case "Top":
                defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Top);
                break;
            case "Bottom":
                defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Top);
                break;
            default:
                break;
        }

        AdRequest request = new AdRequest.Builder().Build();
        defaultBannerView.LoadAd(request);
    }

    public void DestroyDefaultBanner() {
        defaultBannerView.Destroy();
    }

    public void RequestDefaultInterstitial()
    {
        #if UNITY_IOS
            string adUnitId = Const.CO.IPHONE_STAGE_TRANS_INTERSTITIAL;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        defaultInterstitialAd = new InterstitialAd(adUnitId);
        defaultInterstitialAd.OnAdLoaded += HandleOnAdLoaded;
        defaultInterstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        defaultInterstitialAd.OnAdOpening += HandleOnAdOpened;
        defaultInterstitialAd.OnAdClosed += HandleOnAdClosed;
        defaultInterstitialAd.OnAdDidRecordImpression += HandleOnAdLeavingApplication;

        AdRequest request = new AdRequest.Builder().Build();
        defaultInterstitialAd.LoadAd(request);
    }

    // 広告の読み込み完了時
    private void HandleOnAdLoaded(object sender, System.EventArgs args)
    {
    }

    // 広告の読み込み失敗時
    private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // 次のシーンに遷移
    }

    // 広告がデバイスの画面いっぱいに表示されたとき
    private void HandleOnAdOpened(object sender, System.EventArgs args)
    {
    }

    // 広告を閉じたとき
    private void HandleOnAdClosed(object sender, System.EventArgs args)
    {
        // 再び広告をロード
        AdRequest request = new AdRequest.Builder().Build();
        defaultInterstitialAd.LoadAd(request);

        // 次のシーンに遷移
    }
    
    // 別のアプリ（Google Play ストアなど）を起動した時
    private void HandleOnAdLeavingApplication(object sender, System.EventArgs args)
    {
    }

    public void ShowDefaultInterstitial() {
        if(defaultInterstitialAd.IsLoaded()) {
            defaultInterstitialAd.Show();
        }
    }

    public void DestroyDefaultInterstitial() {
        defaultInterstitialAd.Destroy();
    }

}
