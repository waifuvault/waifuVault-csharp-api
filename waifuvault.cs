using System.Net;
using System.Text;
using System.Text.Json;

namespace Waifuvault;

public class Api
{
    public static string baseURL = "https://waifuvault.moe/rest";
    public static HttpClient? customHttpClient;
    public static RestrictionResponse? restrictions;

    public static async Task<RestrictionResponse> getRestrictions()
    {
        var client = customHttpClient ?? new HttpClient();
        var url = $"{baseURL}/resources/restrictions";
        var getRestrictionsResponse = await client.GetAsync(url);
        await checkError(getRestrictionsResponse,false);
        var getRestrictionsResponseData = await getRestrictionsResponse.Content.ReadAsStringAsync();
        var restrictionList = JsonSerializer.Deserialize<List<Restriction>>(getRestrictionsResponseData) ??
                              new List<Restriction>();
        restrictions = new RestrictionResponse(restrictionList);
        return restrictions;
    }

    public static void clearRestrictions()
    {
        restrictions = null;
    }

    public static async Task<FilesInfoResponse> getFileStats()
    {
        var client = customHttpClient ?? new HttpClient();
        var url = $"{baseURL}/resources/stats/files";
        var getFilesResponse = await client.GetAsync(url);
        await checkError(getFilesResponse,false);
        var getFilesResponseData = await getFilesResponse.Content.ReadAsStringAsync();
        var statsObj = JsonSerializer.Deserialize<FilesInfoResponse>(getFilesResponseData) ??
                       new FilesInfoResponse(0, 0);
        return statsObj;
    }

    public static void setAltBaseURL(string url)
    {
        baseURL = url;
    }
    
    public static async Task<BucketResponse> createBucket()
    {
        var client = customHttpClient ?? new HttpClient();
        var url = $"{baseURL}/bucket/create";
        var createResponse = await client.GetAsync(url);
        await checkError(createResponse,false);
        var createResponseData = await createResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BucketResponse>(createResponseData) ?? new BucketResponse();
    }

    public static async Task<bool> deleteBucket(string token, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/bucket/{token}";
        var urlResponse = await client.DeleteAsync(url,ct != null ? ct.Value : cts.Token);
        await checkError(urlResponse,false);
        var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
        return urlResponseData == "true"; 
    }
    
    public static async Task<BucketResponse> getBucket(string token, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/bucket/get";
        var data = new { bucket_token = token };
        var jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var getResponse = await client.PostAsync(url, content, ct != null ? ct.Value : cts.Token);
        await checkError(getResponse,false);
        var getResponseData = await getResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BucketResponse>(getResponseData) ?? new BucketResponse();
    }

    public static async Task<AlbumResponse> createAlbum(string bucketToken, string name, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/{bucketToken}";
        var data = new { name = name };
        var jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var getResponse = await client.PostAsync(url, content, ct != null ? ct.Value : cts.Token);
        await checkError(getResponse,false);
        var getResponseData = await getResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AlbumResponse>(getResponseData) ?? new AlbumResponse();
    }

    public static async Task<bool> deleteAlbum(string albumToken, bool deleteFiles, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/{albumToken}?deleteFiles={deleteFiles.ToString().ToLower()}";
        var urlResponse = await client.DeleteAsync(url,ct != null ? ct.Value : cts.Token);
        await checkError(urlResponse,false);
        var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
        var deleteObj = JsonSerializer.Deserialize<GeneralResponse>(urlResponseData);
        return deleteObj != null ? deleteObj.success : false;
    }

    public static async Task<AlbumResponse> getAlbum(string token, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/{token}";
        var getAlbumResponse = await client.GetAsync(url, ct != null ? ct.Value : cts.Token);
        await checkError(getAlbumResponse,false);
        var getAlbumResponseData = await getAlbumResponse.Content.ReadAsStringAsync();
        var albumObj = JsonSerializer.Deserialize<AlbumResponse>(getAlbumResponseData) ??
                       new AlbumResponse();
        return albumObj;
    }

    public static async Task<AlbumResponse> associateFile(string token, List<string> fileTokens, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/{token}/associate";
        var data = new { fileTokens = fileTokens };
        var jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var getResponse = await client.PostAsync(url, content, ct != null ? ct.Value : cts.Token);
        await checkError(getResponse,false);
        var getResponseData = await getResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AlbumResponse>(getResponseData) ?? new AlbumResponse();
    }
    
    public static async Task<AlbumResponse> disassociateFile(string token, List<string> fileTokens, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/{token}/disassociate";
        var data = new { fileTokens = fileTokens };
        var jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var getResponse = await client.PostAsync(url, content, ct != null ? ct.Value : cts.Token);
        await checkError(getResponse,false);
        var getResponseData = await getResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AlbumResponse>(getResponseData) ?? new AlbumResponse();
    }

    public static async Task<string> shareAlbum(string token, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/share/{token}";
        var getResponse = await client.GetAsync(url, ct != null ? ct.Value : cts.Token);
        await checkError(getResponse,false);
        var getResponseData = await getResponse.Content.ReadAsStringAsync();
        var resp = JsonSerializer.Deserialize<GeneralResponse>(getResponseData);
        return resp != null ? resp.description : "";
    }

    public static async Task<bool> revokeAlbum(string token, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/revoke/{token}";
        var getResponse = await client.GetAsync(url, ct != null ? ct.Value : cts.Token);
        await checkError(getResponse,false);
        var getResponseData = await getResponse.Content.ReadAsStringAsync();
        var resp = JsonSerializer.Deserialize<GeneralResponse>(getResponseData);
        return resp != null ? resp.success : false;
    }

    public static async Task<byte[]> downloadAlbum(string token, CancellationToken? ct = null)
    {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/album/download/{token}";
        var data = new List<int>();
        var jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var fileResponse = await client.PostAsync(url, content, ct != null ? ct.Value : cts.Token);
        await checkError(fileResponse,true);
        var fileData = await fileResponse.Content.ReadAsByteArrayAsync();
        return fileData;
    }

    public static async Task<FileResponse> uploadFile(FileUpload fileObj, CancellationToken? ct = null) {
        var retval = new FileResponse();
        await checkRestrictions(fileObj);
        var targetUrl = fileObj.buildURL(String.IsNullOrEmpty(fileObj.bucketToken) ? baseURL : baseURL + $"/{fileObj.bucketToken}");

        if (!String.IsNullOrEmpty(fileObj.url)) {
            // URL Upload
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("url", fileObj.url)
            };

            if (!String.IsNullOrEmpty(fileObj.password))
            {
                keyValuePairs.Add(new KeyValuePair<string, string>("password", fileObj.password));
            }

            var urlContent = new FormUrlEncodedContent(keyValuePairs);
            retval = await sendContent(targetUrl, urlContent, ct);
        }
        else if(!String.IsNullOrEmpty(fileObj.filename) && fileObj.buffer == null) {
            // File Upload
            var content = new MultipartFormDataContent();
            using(var fileStream = new FileStream(fileObj.filename, FileMode.Open)) {
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileObj.filename));
                if (!String.IsNullOrEmpty(fileObj.password))
                {
                    content.Add(new StringContent(fileObj.password), "password");
                }
                retval = await sendContent(targetUrl, content, ct);
            }
        }
        else {
            // Buffer Upload
            var content = new MultipartFormDataContent();
            using(var memStream = new MemoryStream(fileObj.buffer)) {
                content.Add(new StreamContent(memStream), "file", fileObj.filename);
                if (!String.IsNullOrEmpty(fileObj.password))
                {
                    content.Add(new StringContent(fileObj.password), "password");
                }
                retval = await sendContent(targetUrl, content, ct);
            }
        }

        return retval;
    }

    public static async Task<FileResponse> fileInfo(string token, bool formatted, CancellationToken? ct = null) {
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/{token}?formatted={formatted}";
        var infoResponse = await client.GetAsync(url,ct != null ? ct.Value : cts.Token);
        await checkError(infoResponse,false);
        var infoResponseData = await infoResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileResponse>(infoResponseData) ?? new FileResponse();
    }

    public static async Task<FileResponse> fileUpdate(string token, string? password = null, string? previousPassword = null, string? customExpiry = null, bool hideFilename = false, CancellationToken? ct = null) {
        var client = customHttpClient ?? new HttpClient();
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
        var client = customHttpClient ?? new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/{token}";
        var urlResponse = await client.DeleteAsync(url,ct != null ? ct.Value : cts.Token);
        await checkError(urlResponse,false);
        var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
        return urlResponseData == "true"; 
    }

    public static async Task<byte[]> getFile(FileResponse fileObj, string? password = null, CancellationToken? ct = null) {
        var client = customHttpClient ?? new HttpClient();
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
        var client = customHttpClient ?? new HttpClient();
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

    private static async Task checkRestrictions(FileUpload fileObj)
    {
        if (restrictions == null)
        {
            await getRestrictions();
        }

        if (restrictions != null && restrictions.Expires < DateTime.Now)
        {
            await getRestrictions();
        }

        if (restrictions != null && restrictions.Restrictions != null)
        {
            foreach (var restriction in restrictions.Restrictions)
            {
                restriction.passes(fileObj);
            }
        }
    }
}