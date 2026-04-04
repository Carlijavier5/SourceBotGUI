using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SearchRequest
{
    public string query;
    public int matches = 5;
    public string sortBy = "pagerank";
}

[System.Serializable]
public class SearchResponse
{
    public SearchStats stats;
    public List<SearchFile> files;
    public List<RepositoryInfo> repositoryInfo;
    public bool isSearchExhaustive;
}

[System.Serializable]
public class SearchStats
{
    public int actualMatchCount;
    public int totalMatchCount;
    public int fileCount;
}

[System.Serializable]
public class SearchFile
{
    public FileName fileName;
    public string repository;
    public string language;
    public string webUrl;
    public string externalWebUrl;
    public List<Chunk> chunks;
}

[System.Serializable]
public class FileName
{
    public string text;
}

[System.Serializable]
public class Chunk
{
    public string content;
    public List<MatchRange> matchRanges;
    public ContentPosition contentStart;
}

[System.Serializable]
public class MatchRange
{
    public ContentPosition start;
    public ContentPosition end;
}

[System.Serializable]
public class ContentPosition
{
    public int byteOffset;
    public int column;
    public int lineNumber;
}

[System.Serializable]
public class RepositoryInfo
{
    public int id;
    public string name;
    public string displayName;
    public string webUrl;
}

public class SourcebotSearch : MonoBehaviour
{
    [Header("API Config")]
    [SerializeField] private string apiEndpoint = "http://52.9.90.18:3000/api/search";
    private string apiKey;

    private void Awake() {
        string path = Path.Combine(Application.dataPath, "apikey.txt");
        if (File.Exists(path)) {
            apiKey = File.ReadAllText(path).Trim();
        }
    }

    public void Search(string query, int matches, string sortBy, System.Action<SearchResponse> onSuccess, System.Action<string> onError) {
        StartCoroutine(SearchCoroutine(query, matches, sortBy, onSuccess, onError));
    }

    private IEnumerator SearchCoroutine(string query, int matches, string sortBy, System.Action<SearchResponse> onSuccess, System.Action<string> onError) {

        SearchRequest requestBody = new() { query = query, matches = matches, sortBy = sortBy };
        string json = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new(apiEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success) {
            onError?.Invoke($"Request failed: {request.error}");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        SearchResponse response;
        try {
            response = JsonUtility.FromJson<SearchResponse>(responseText);
        } catch (System.Exception e) {
            onError?.Invoke($"Failed to parse response: {e.Message}");
            yield break;
        }

        onSuccess?.Invoke(response);
    }
}
