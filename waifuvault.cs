using System.Net;
using System.Text.Json;

namespace Waifuvault;

public class Api
{
    public const string baseURL = "https://waifuvault.moe/rest";
    public static HttpClient? customHttpClient;

    public static async Task<FileResponse> uploadFile(FileUpload fileObj, CancellationToken? ct = null) {
        var retval = new FileResponse();
        var targetUrl = fileObj.buildURL(baseURL);

        if (!String.IsNullOrEmpty(fileObj.url)) {
            // URL Upload
            var urlContent = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string,string>("url", fileObj.url)
                });
            retval = await sendContent(targetUrl, urlContent, ct);
        }
        else if(!String.IsNullOrEmpty(fileObj.filename) && fileObj.buffer == null) {
            // File Upload
            var content = new MultipartFormDataContent();
            using(var fileStream = new FileStream(ExpandHomedir(fileObj.filename), FileMode.Open)) {
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileObj.filename));
                retval = await sendContent(targetUrl, content, ct);
            }
        }
        else {
            // Buffer Upload
            var content = new MultipartFormDataContent();
            using(var memStream = new MemoryStream(fileObj.buffer)) {
                content.Add(new StreamContent(memStream), "file", fileObj.filename);
                retval = await sendContent(targetUrl, content, ct);
            }
        }

        return retval;
    }

    public static async Task<FileResponse> fileInfo(string token, bool formatted, CancellationToken? ct = null) {
        var client = customHttpClient!=null ? customHttpClient : new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/{token}?formatted={formatted}";
        var infoResponse = await client.GetAsync(url,ct != null ? ct.Value : cts.Token);
        await checkError(infoResponse,false);
        var infoResponseData = await infoResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileResponse>(infoResponseData) ?? new FileResponse();
    }

    public static async Task<FileResponse> fileUpdate(string token, string? password = null, string? previousPassword = null, string? customExpiry = null, bool hideFilename = false, CancellationToken? ct = null) {
        var client = customHttpClient!=null ? customHttpClient : new HttpClient();
        var url = $"{baseURL}/{token}";
        var fields = new List<KeyValuePair<string,string>>();
        if (password!=null) {
            fields.Add(new KeyValuePair<string,string>("password", password));
        }
        if (previousPassword!=null) {
            fields.Add(new KeyValuePair<string,string>("previousPassword", previousPassword));
        }
        if (customExpiry!=null) {
            fields.Add(new KeyValuePair<string,string>("customExpiry", customExpiry));
        }
        fields.Add(new KeyValuePair<string,string>("hideFilename", hideFilename.ToString().ToLower()));
        var content = new FormUrlEncodedContent(fields.ToArray());
        var infoResponse = await client.PatchAsync(url, content);
        await checkError(infoResponse,false);
        var infoResponseData = await infoResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileResponse>(infoResponseData) ?? new FileResponse();
    }

    public static async Task<bool> deleteFile(string token, CancellationToken? ct = null) {
        var client = customHttpClient!=null ? customHttpClient : new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/{token}";
        var urlResponse = await client.DeleteAsync(url,ct != null ? ct.Value : cts.Token);
        await checkError(urlResponse,false);
        var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
        return urlResponseData == "true"; 
    }

    public static async Task<byte[]> getFile(FileResponse fileObj, string? password = null, CancellationToken? ct = null) {
        var client = customHttpClient!=null ? customHttpClient : new HttpClient();
        var cts = new CancellationTokenSource();
        if(String.IsNullOrEmpty(fileObj.url) && !String.IsNullOrEmpty(fileObj.token)) {
            var fileUrl = await fileInfo(fileObj.token, false, ct != null ? ct.Value : cts.Token);
            fileObj.url = fileUrl.url;
        }
        if(!String.IsNullOrEmpty(password)) {
            client.DefaultRequestHeaders.Add("x-password",password);
        }
        var fileResponse = await client.GetAsync(fileObj.url, ct != null ? ct.Value : cts.Token);
        await checkError(fileResponse,true);
        var fileData = await fileResponse.Content.ReadAsByteArrayAsync();
        return fileData;
    }

    private static async Task<FileResponse> sendContent(string targetUrl, HttpContent content, CancellationToken? ct) {
        var client = customHttpClient!=null ? customHttpClient : new HttpClient();
        var cts = new CancellationTokenSource();
        var fileResponse = await client.PutAsync(targetUrl, content, ct != null ? ct.Value : cts.Token);
        await checkError(fileResponse,false);
        var fileResponseData = await fileResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileResponse>(fileResponseData);
    }

    private static async Task checkError(HttpResponseMessage? response, bool isDownload) {
        if(response == null) {
            throw new ArgumentNullException("Response is empty");
        }
        if(!response.IsSuccessStatusCode) {
            var body = await response.Content.ReadAsStringAsync();
            ErrorResponse error = new ErrorResponse();
            try {
                error = JsonSerializer.Deserialize<ErrorResponse>(body);
            } catch {
                error.name = "Deserialization Failed";
                error.message = body;
            }
            if(response.StatusCode == HttpStatusCode.Forbidden && isDownload) {
                throw new Exception("Password is incorrect");
            }
            throw new Exception($"Error {response.StatusCode.ToString()} ({error.name}:{error.message})");
        }
    }
    
    private static string ExpandHomedir(string path)
    {
        if (path.StartsWith("~"))
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(home, path.TrimStart('~').TrimStart('/', '\\'));
        }
        return path;
    }
}