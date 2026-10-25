using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

/*>
> https://docs.unity.com/en-us/engine/6000.7/manual/platform-specific/webgl/develop/networking?utm_source=chatgpt.com
<*/

public class Network : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Calling...");
        StartCoroutine(Login());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Login()
    {
        Debug.Log("Calling 2...");

        string url = "http://127.0.0.1:5001/api/auth/login";

        string json = "{\"username\":\"BitMiller\",\"password\":\"Almafa123-\"}";

        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                UnityEngine.Debug.Log(response);
            }
            else
            {
                UnityEngine.Debug.LogError(request.error);
            }
        }
    }
}
