using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAndLoadData : MonoBehaviour
{
    private int mIsRemoveAds;
    public bool IsRemoveAds
    {
        private set { }
        get { return mIsRemoveAds == 1; }
    }



    static public SaveAndLoadData Instance
    {
        private set;
        get;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        LoadAdsState();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LoadAdsState()
    {
        if(PlayerPrefs.HasKey(ConstValues.KEY_REMOVE_ADS))
        {
            mIsRemoveAds = PlayerPrefs.GetInt(ConstValues.KEY_REMOVE_ADS);
        }
    }

    public void UpdateAdsState(bool isRemoveAds = true)
    {
        if(isRemoveAds)
        {
            mIsRemoveAds = 1;
            PlayerPrefs.SetInt(ConstValues.KEY_REMOVE_ADS,1);
        }
    }

}
