using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MHIdle.Net
{
    /// <summary>
    /// 最小云存档客户端。开发期用 guest 登录；微信正式环境再换成真实 code。
    /// </summary>
    public class CloudSaveClient : MonoBehaviour
    {
        [SerializeField] string baseUrl = "http://127.0.0.1:8000";
        [SerializeField] string guestKey = "unity-editor-device";

        public string Token { get; private set; }
        public int PlayerId { get; private set; }
        public int Revision { get; private set; }

        [Serializable]
        class LoginBody
        {
            public string code;
            public string guest_key;
        }

        [Serializable]
        class LoginResp
        {
            public string token;
            public int player_id;
            public string openid;
            public int expires_at;
        }

        [Serializable]
        class SaveBody
        {
            public string dataJson; // 简化：外层再包一层，实际用原始 JSON 字符串更稳
            public int revision;
        }

        public async Task<bool> LoginGuestAsync()
        {
            var body = JsonUtility.ToJson(new LoginBody { code = "guest", guest_key = guestKey });
            using var req = new UnityWebRequest(baseUrl.TrimEnd('/') + "/api/login", "POST");
            byte[] raw = Encoding.UTF8.GetBytes(body);
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogWarning("云登录失败: " + req.error + " " + req.downloadHandler.text);
                return false;
            }

            var resp = JsonUtility.FromJson<LoginResp>(req.downloadHandler.text);
            Token = resp.token;
            PlayerId = resp.player_id;
            Debug.Log($"云登录成功 player={PlayerId}");
            return true;
        }

        public async Task<string> PullSaveRawAsync()
        {
            if (string.IsNullOrEmpty(Token)) return null;
            using var req = UnityWebRequest.Get(baseUrl.TrimEnd('/') + "/api/save");
            req.SetRequestHeader("Authorization", "Bearer " + Token);
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogWarning("拉档失败: " + req.error);
                return null;
            }

            // 服务器返回 { data, revision, updated_at } —— 这里先整包返回，后续再对接 SaveSystem
            return req.downloadHandler.text;
        }

        /// <summary>
        /// 推送原始存档 JSON 对象字符串（需是对象，不是数组）。
        /// </summary>
        public async Task<bool> PushSaveRawAsync(string saveObjectJson, int revision)
        {
            if (string.IsNullOrEmpty(Token)) return false;

            // 手动拼 JSON，避免 JsonUtility 对 Dictionary 支持差
            string body = "{\"data\":" + saveObjectJson + ",\"revision\":" + revision + "}";
            using var req = new UnityWebRequest(baseUrl.TrimEnd('/') + "/api/save", "PUT");
            byte[] raw = Encoding.UTF8.GetBytes(body);
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + Token);
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogWarning("推档失败: " + req.error + " " + req.downloadHandler.text);
                return false;
            }

            Debug.Log("云存档已推送");
            return true;
        }
    }
}
