﻿
namespace ME.GD {

    public class GDSystemMonoBehaviour : UnityEngine.MonoBehaviour {

        public string url;
        public string version;
        public GDData data;

        public void Awake() {

            UnityEngine.GameObject.DontDestroyOnLoad(this.gameObject);

            this.Init();
            this.StartCoroutine(this.UpdateData(GDSystem.active));
            
        }

        public void Init() {
            
            var gdSystem = new GDSystem();
            GDSystem.SetActive(gdSystem);

        }
        
        public System.Collections.IEnumerator UpdateData(GDSystem gdSystem) {

            var url = this.url.Replace("{streaming_assets}", UnityEngine.Application.streamingAssetsPath);
            yield return gdSystem.DownloadAndUpdate(url, this.version, this.data, (result) => {

                if (result == true) {
                    
                    gdSystem.Use(this.data);
                    
                } else {
                    
                    UnityEngine.Debug.LogError("Failed to load " + this.url + ", version: " + this.version);
                    
                }
                
            });

        }

    }

}