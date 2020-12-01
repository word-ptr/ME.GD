﻿using System.Collections.Generic;

namespace ME.GD {

    using Parsers;
    
    public enum GDValueType {

        Unknown = 0,
        String,
        Float,
        Integer,

    }

    [System.Serializable]
    public struct Item {

        public string key;
        public GDValueType type;
        public string s;
        public float f;
        public int i;

    }

    [System.Serializable]
    public struct GDInt {

        public string key;

        public int Get() {

            if (GDSystem.active.Get(this, out int val) == true) return val;
            return default;

        }
        
        public static implicit operator int(GDInt key) {

            return key.Get();

        }

    }

    [System.Serializable]
    public struct GDString {
        
        public string key;

        public string Get() {

            if (GDSystem.active.Get(this, out string val) == true) return val;
            return default;

        }
        
        public static implicit operator string(GDString key) {

            return key.Get();

        }

    }
    
    [System.Serializable]
    public struct GDFloat {
        
        public string key;

        public float Get() {

            if (GDSystem.active.Get(this, out float val) == true) return val;
            return default;

        }
        
        public static implicit operator float(GDFloat key) {

            return key.Get();

        }
        
    }
    
    [System.Serializable]
    public struct GDKey {

        public string key;

        public float GetFloat() {

            if (GDSystem.active.Get(this, out float val) == true) return val;
            return default;

        }

        public int GetInt() {

            if (GDSystem.active.Get(this, out int val) == true) return val;
            return default;

        }

        public string GetString() {

            if (GDSystem.active.Get(this, out string val) == true) return val;
            return default;

        }

        public static implicit operator float(GDKey key) {

            return key.GetFloat();

        }

        public static implicit operator int(GDKey key) {

            return key.GetInt();

        }

        public static implicit operator string(GDKey key) {

            return key.GetString();

        }

    }

    public class GDSystem {

        public static GDSystem active;
        private Dictionary<string, Item> lines = new Dictionary<string, Item>();
        private GDData data;

        public static void SetActive(GDSystem system) {

            GDSystem.active = system;

        }

        public GDData GetData() {

            return this.data;

        }

        public int GetKeysCount() {

            if (this.data == null) return 0;
            return this.data.items.Count;

        }

        public bool Get(GDInt key, out int value, bool forced = false) {

            return this.Get(new GDKey() { key = key.key }, out value, forced);

        }

        public bool Get(GDFloat key, out float value, bool forced = false) {

            return this.Get(new GDKey() { key = key.key }, out value, forced);

        }

        public bool Get(GDString key, out string value, bool forced = false) {

            return this.Get(new GDKey() { key = key.key }, out value, forced);

        }

        public bool Get(GDKey key, out float value, bool forced = false) {

            value = default;
            if (this.lines.TryGetValue(key.key, out var item) == true && (item.type == GDValueType.Float || forced == true)) {

                value = item.f;
                return true;

            }

            return false;

        }

        public bool Get(GDKey key, out int value, bool forced = false) {
            
            value = default;
            if (this.lines.TryGetValue(key.key, out var item) == true && (item.type == GDValueType.Integer || forced == true)) {

                value = item.i;
                return true;

            }

            return false;

        }

        public bool Get(GDKey key, out string value, bool forced = false) {
            
            value = default;
            if (this.lines.TryGetValue(key.key, out var item) == true && (item.type == GDValueType.String || forced == true)) {

                value = item.s;
                return true;

            }

            return false;

        }

        public void Use(GDData data) {

            this.data = data;

            this.lines.Clear();
            for (int i = 0; i < data.items.Count; ++i) {

                var item = data.items[i];
                this.lines.Add(item.key, item);

            }

        }
        
        public void Update(string data, string version, GDData output) {

            output.Clear();

            var reader = new CsvReader(new System.IO.StringReader(data), ",");
            var line = 0;
            var versionIndex = -1;
            while (reader.Read() == true) {

                ++line;
                if (line == 1) {
                    
                    // check version
                    for (int i = 1; i < reader.FieldsCount; ++i) {

                        var ver = reader[i];
                        if (ver == version) {

                            versionIndex = i;
                            break;

                        }

                    }

                    if (versionIndex == -1) return;
                    
                    continue;
                    
                }
                
                var key = reader[0];
                var value = reader[versionIndex];

                var item = new Item() {
                    key = key,
                    type = GDValueType.String,
                    s = value,
                };
                if (float.TryParse(value, out item.f) == true) item.type = GDValueType.Float;
                if (int.TryParse(value, out item.i) == true) item.type = GDValueType.Integer;

                output.Add(item);

            }

            output.version = version;
            output.SetDirty();

        }
        
        public System.Collections.IEnumerator DownloadAndUpdate(string url, string version, GDData data, System.Action<bool> onComplete) {

            var request = UnityEngine.Networking.UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            while (request.isDone == false) yield return null;

            var hasError = (request.isNetworkError == true || request.isHttpError == true);
            if (hasError == false) this.Update(request.downloadHandler.text, version, data);
            onComplete.Invoke(hasError == false);

        }
        
    }

}