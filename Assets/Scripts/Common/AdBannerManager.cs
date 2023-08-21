using UnityEngine;
using GoogleMobileAds.Api;

public class AdBannerManager : MonoBehaviour
{
    
    private static GameObject adBannerManagerObject;

    private static AdBannerManager singleton;

    private BannerView defaultBannerView;

    public static AdBannerManager Instance
    {
        get {
            if (singleton == null)
            {
                // AdBannerManager作成
                adBannerManagerObject = new GameObject("AdBannerManager");
                adBannerManagerObject.AddComponent<AdBannerManager>();

                // 遷移先シーンでもオブジェクトを破棄しない。
                DontDestroyOnLoad(adBannerManagerObject);

                singleton = adBannerManagerObject.GetComponent<AdBannerManager>();
            }
            return singleton;
        }
    }
    void Start()
    {
        MobileAds.Initialize(initStatus => { });
    }

    public void RequestDefaultBanner()
    {
        #if UNITY_IOS
            string adUnitId = Const.CO.IPHONE_DEFAULT_BANNER;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        defaultBannerView.LoadAd(request);
    }

    public void DestroyDefaultBanner() {
        defaultBannerView.Destroy();
    }


}
