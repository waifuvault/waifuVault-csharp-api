# waifuvault-C#-api

![tests](https://github.com/waifuvault/waifuVault-csharp-api/actions/workflows/dotnet.yml/badge.svg)
[![GitHub issues](https://img.shields.io/github/issues/waifuvault/waifuVault-csharp-api.png)](https://github.com/waifuvault/waifuVault-csharp-api/issues)
[![last-commit](https://img.shields.io/github/last-commit/waifuvault/waifuVault-csharp-api)](https://github.com/waifuvault/waifuVault-csharp-api/commits/master)

This contains the official API bindings for uploading, deleting and obtaining files
with [waifuvault.moe](https://waifuvault.moe/). Contains a full up to date API for interacting with the service

## Installation

```sh
dotnet add package Waifuvault
```

## Usage

This API contains 18 interactions:

1. Upload File
2. Get File Info
3. Update File Info
4. Delete File
5. Get File
6. Create Bucket
7. Delete Bucket
8. Get Bucket
9. Create Album
10. Delete Album
11. Get Album
12. Associate File
13. Disassociate File
14. Share Album
15. Revoke Album 
16. Get Restrictions 
17. Clear Restrictions
18. Get File Stats

The package is namespaced to `Waifuvault`, so to import it, simply:

```cs
using Waifuvault;
```

### Upload File

To Upload a file, use the `uploadFile` function. This function takes the following options as an object:

| Option            | Type          | Description                                                     | Required       | Extra info                       |
|-------------------|---------------|-----------------------------------------------------------------|----------------|----------------------------------|
| `filename`        | `string `     | The path to the file to upload                                  | true if File   | File path                        |
| `url`             | `string`      | The URL of the file to target                                   | true if URL    | Filename with extension          |
| `buffer`          | `byte array`  | Byte array containing file to upload                            | true if buffer | Needs filename set also          |
| `bucketToken`     | `string`      | Token for a bucket to upload the file into                      | false          | Create bucket gives token        |
| `expires`         | `string`      | A string containing a number and a unit (1d = 1day)             | false          | Valid units are `m`, `h` and `d` |
| `hideFilename`    | `boolean`     | If true, then the uploaded filename won't appear in the URL     | false          | Defaults to `false`              |
| `password`        | `string`      | If set, then the uploaded file will be encrypted                | false          |                                  |
| `ct`              | `canceltoken` | An optional cancellation token that can be passed in            | false          | Standard cancellation token      |
| `oneTimeDownload` | `boolean`     | if supplied, the file will be deleted as soon as it is accessed | false          |                                  |

> **NOTE:** Server restrictions are checked by the SDK client side *before* upload, and will throw an exception if they are violated

Using a URL:

```cs
using Waifuvault;

var uploadFile = new Waifuvault.FileUpload("https://waifuvault.moe/assets/custom/images/08.png");
var uploadResp = await Waifuvault.Api.uploadFile(uploadFile);

Console.WriteLine(uploadResp.url);
```

Using a file path:

```cs
using Waifuvault;

var uploadFile = new Waifuvault.FileUpload("../aCoolFile.png");
var uploadResp = await Waifuvault.Api.uploadFile(uploadFile);

Console.WriteLine(uploadResp.url);
```

Using a buffer:

```cs
using Waifuvault;
using System.IO;

byte[] buffer = File.ReadAllBytes("./aCoolFile.png");
var uploadFile = new Waifuvault.FileUpload(buffer,"aCoolFile.png");
var uploadResp = await Waifuvault.Api.uploadFile(uploadFile);

Console.WriteLine(uploadResp.url);
```

Cancelable with a file:

```cs
using Waifuvault;

var cts = new CancellationTokenSource(2000);  // Auto cancel in 2s
var cancelFile = new Waifuvault.FileUpload("./largeFile.mkv");
try {
    var cancelled = await Waifuvault.Api.uploadFile(cancelFile,cts.Token);
} catch(OperationCanceledException) {
    Console.WriteLine("Cancelled upload");
}
```

### Get File Info

If you have a token from your upload. Then you can get file info. This results in the following info:

* token
* url
* protected
* retentionPeriod

Use the `fileInfo` function. This function takes the following options as parameters:

| Option      | Type      | Description                                                        | Required | Extra info        |
|-------------|-----------|--------------------------------------------------------------------|----------|-------------------|
| `token`     | `string`  | The token of the upload                                            | true     |                   |
| `formatted` | `boolean` | If you want the `retentionPeriod` to be human-readable or an epoch | false    | defaults to false |
| `ct`        | `canceltoken`| An optional cancellation token that can be passed in            | false    | Standard cancellation token |

Epoch timestamp:

```cs
using Waifuvault;
var tokenInfo = await Waifuvault.Api.fileInfo(uploadResp.token,false);
Console.WriteLine(tokenInfo.url);
Console.WriteLine(tokenInfo.retentionPeriod);
```

Human-readable timestamp:

```cs
using Waifuvault;
var tokenInfo = await Waifuvault.Api.fileInfo(uploadResp.token,true);
Console.WriteLine(tokenInfo.url);
Console.WriteLine(tokenInfo.retentionPeriod);
```

### Update File Info

If you have a token from your upload, then you can update the information for the file.  You can change the password or remove it, 
you can set custom expiry time or remove it, and finally you can choose whether the filename is hidden.

Use the `fileUpdate` function. This function takes the following options as parameters:

| Option              | Type     | Description                                             | Required | Extra info                                  |
|---------------------|----------|---------------------------------------------------------|----------|---------------------------------------------|
| `token`             | `string` | The token of the upload                                 | true     |                                             |
| `password`          | `string` | The current password of the file                        | false    | Set to empty string to remove password      |
| `previousPassword`  | `string` | The previous password of the file, if changing password | false    |                                             |
| `customExpiry`      | `string` | Custom expiry in the same form as upload command        | false    | Set to empty string to remove custom expiry |
| `hideFilename`      | `bool`   | Sets whether the filename is visible in the URL or not  | false    |                                             |

```cs
using Waifuvault;
var updateInfo = await Waifuvault.Api.fileUpdate(uploadResp.token, customExpiry:"2d");
Console.WriteLine(updateInfo.retentionPeriod);
```

### Delete File

To delete a file, you must supply your token to the `deletefile` function.

This function takes the following options as parameters:

| Option  | Type     | Description                              | Required | Extra info |
|---------|----------|------------------------------------------|----------|------------|
| `token` | `string` | The token of the file you wish to delete | true     |            |
| `ct`    | `canceltoken`| An optional cancellation token that can be passed in            | false    | Standard cancellation token |

> **NOTE:** `deleteFile` will only ever either return `true` or throw an exception if the token is invalid

Standard delete:

```cs
using Waifuvault;
var deleted = await Waifuvault.Api.deleteFile(token);
Console.WriteLine(deleted);
```

### Get File

This lib also supports obtaining a file from the API as a Buffer by supplying either the token or the unique identifier
of the file (epoch/filename).

Use the `getFile` function. This function takes the following options an object:

| Option     | Type     | Description                                | Required                           | Extra info                                      |
|------------|----------|--------------------------------------------|------------------------------------|-------------------------------------------------|
| `token`    | `string` | The token of the file you want to download | true only if `filename` is not set | if `filename` is set, then this can not be used |
| `url`      | `string` | The URL of the file                        | true only if `token` is not set    | if `token` is set, then this can not be used    |
| `password` | `string` | The password for the file                  | true if file is encrypted          | Passed as a parameter on the function call      |
| `ct`       | `canceltoken`| An optional cancellation token that can be passed in            | false    | Standard cancellation token |

> **Important!** The Unique identifier filename is the epoch/filename only if the file uploaded did not have a hidden
> filename, if it did, then it's just the epoch.
> For example: `1710111505084/08.png` is the Unique identifier for a standard upload of a file called `08.png`, if this
> was uploaded with hidden filename, then it would be `1710111505084.png`

Obtain an encrypted file

```cs
using Waifuvault;
var file = new FileResponse(token:your_token);
var downloaded = await Waifuvault.Api.getFile(file,"password");
Console.WriteLine(downloaded.Length);
```

Obtain a file from URL

```cs
using Waifuvault;
var file = new FileResponse(url:your_url);
var downloaded = await Waifuvault.Api.getFile(file);
Console.WriteLine(downloaded.Length);
```

Obtain file with ability to cancel:

```cs
using Waifuvault;
var cts = new CancellationTokenSource(2000);  // Auto cancel in 2s
var file = new FileResponse(token:your_token);
try {
    var cancelled = await Waifuvault.Api.getFile(file, cts.Token);
} catch(OperationCanceledException) {
    Console.WriteLine("Canceled download");
}
```

### Create Bucket

Buckets are virtual collections that are linked to your IP and a token. When you create a bucket, you will receive a bucket token that you can use in Get Bucket to get all the files in that bucket

> **NOTE:** Only one bucket is allowed per client IP address, if you call it more than once, it will return the same bucket token

To create a bucket, use the `createBucket` function. This function does not take any arguments.

```csharp
using Waifuvault;
var bucket = await Waifuvault.Api.createBucket();

Console.WriteLine(bucket.token);
```

### Delete Bucket

Deleting a bucket will delete the bucket and all the files it contains.

> **IMPORTANT:**  All contained files will be **DELETED** along with the Bucket!

To delete a bucket, you must call the `deleteBucket` function with the following options as parameters:

| Option      | Type      | Description                       | Required | Extra info        |
|-------------|-----------|-----------------------------------|----------|-------------------|
| `token`     | `string`  | The token of the bucket to delete | true     |                   |

> **NOTE:** `deleteBucket` will only ever either return `true` or throw an exception if the token is invalid

```csharp
using Waifuvault;
var resp = await Waifuvault.Api.deleteBucket("some-bucket-token");

Console.WriteLine(resp);
```

### Get Bucket

To get the list of files contained in a bucket, you use the `getBucket` functions and supply the token.
This function takes the following options as parameters:

| Option      | Type      | Description             | Required | Extra info        |
|-------------|-----------|-------------------------|----------|-------------------|
| `token`     | `string`  | The token of the bucket | true     |                   |

This will respond with the bucket and all the files the bucket contains.

```csharp
using Waifuvault;
var bucket = await Waifuvault.Api.getBucket("some-bucket-token");

Console.WriteLine(bucket.token);
foreach(var file in bucket.files)
{
    Console.WriteLine(file.token);    
}
```

### Create Album
Albums are shareable collections of files that exist within a bucket.

To create an album, you use the `createAlbum` function and supply a bucket token and name.

The function takes the following parameters:

| Option        | Type      | Description                         | Required | Extra info        |
|---------------|-----------|-------------------------------------|----------|-------------------|
| `bucketToken` | `string`  | The token of the bucket             | true     |                   |
| `name`        | `string`  | The name of the album to be created | true     |                   |

This will respond with an album object containing the name and token of the album.

```csharp
using Waifuvault;
var album = await Waifuvault.Api.createAlbum("some-bucket-token", "album-name");

Console.WriteLine(album.token);
Console.WriteLine(album.name);
Console.WriteLine(album.files);  // Array of file objects
```

### Delete Album
To delete an album, you use the `deleteAlbum` function and supply the album token and a boolean indication of whether
or not the files contained in the album should be deleted or not.  If you chose false, the files will be returned to the
bucket.

The function takes the following parameters:

| Option        | Type     | Description                         | Required | Extra info        |
|---------------|----------|-------------------------------------|----------|-------------------|
| `albumToken`  | `string` | The private token of the album      | true     |                   |
| `deleteFiles` | `bool`   | Whether the files should be deleted | true     |                   |

> **NOTE:** If `deleteFiles` is set to true, the files will be permanently deleted

This will respond with a boolean indicating success.

```csharp
using Waifuvault;
var resp = await Waifuvault.Api.deleteAlbum("some-album-token", false);

Console.WriteLine(resp);
```

### Get Album
To get the contents of an album, you use the `getAlbum` function and supply the album token.  The token can be either the private token
or the public token.

The function takes the following parameters:

| Option   | Type     | Description            | Required | Extra info                     |
|----------|----------|------------------------|----------|--------------------------------|
| `token`  | `string` | The token of the album | true     | Can be private or public token |

This will respond with the album object containing the album information and files contained within the album.

```csharp
using Waifuvault;
var album = await Waifuvault.Api.getAlbum("some-album-token");

Console.WriteLine(album.token);
Console.WriteLine(album.bucketToken);
Console.WriteLine(album.publicToken);
Console.WriteLine(album.name);
Console.WriteLine(album.files);  // Array of file objects
```

### Associate File
To add files to an album, you use the `associateFile` function and supply the private album token and
a list of file tokens.

The function takes the following parameters:

| Option  | Type           | Description                         | Required | Extra info |
|---------|----------------|-------------------------------------|----------|------------|
| `token` | `string`       | The private token of the album      | true     |            |
| `files` | `list[string]` | List of file tokens to add to album | true     |            |

This will respond with the new album object containing the added files.

```csharp
using Waifuvault;
var files = new List<string>() { "file-token-1", "file-token-2" };
var album = await Waifuvault.Api.associateFile("some-album-token", files);

Console.WriteLine(album.token);
Console.WriteLine(album.name);
Console.WriteLine(album.files);  // Array of file objects
```

### Disassociate File
To remove files from an album, you use the `disassociateFile` function and supply the private album token and
a list of file tokens.

The function takes the following parameters:

| Option  | Type           | Description                         | Required | Extra info |
|---------|----------------|-------------------------------------|----------|------------|
| `token` | `string`       | The private token of the album      | true     |            |
| `files` | `list[string]` | List of file tokens to add to album | true     |            |

This will respond with the new album object with the files removed.

```csharp
using Waifuvault;
var files = new List<string>() { "file-token-1", "file-token-2" };
var album = await Waifuvault.Api.disassociateFile("some-album-token", files);

Console.WriteLine(album.token);
Console.WriteLine(album.name);
Console.WriteLine(album.files);  // Array of file objects
```

### Share Album
To share an album, so it contents can be accessed from a public URL, you use the `shareAlbum` function and
supply the private token.

The function takes the following parameters:

| Option  | Type           | Description                         | Required | Extra info |
|---------|----------------|-------------------------------------|----------|------------|
| `token` | `string`       | The private token of the album      | true     |            |

This will respond with the public URL with which the album can be found.

```csharp
using Waifuvault;
var url = await Waifuvault.Api.shareAlbum("some-album-token");

Console.WriteLine(url);
```

> **NOTE:** The public album token can now be found in the `getAlbum` results

### Revoke Album
To revoke the sharing of an album, so it will no longer be accessible publicly, you use the `revokeAlbum` function
and supply the private token.

The function takes the following parameters:

| Option  | Type           | Description                         | Required | Extra info |
|---------|----------------|-------------------------------------|----------|------------|
| `token` | `string`       | The private token of the album      | true     |            |

This will respond with a boolean True if the album was revoked.

```csharp
using Waifuvault;
var resp = await Waifuvault.Api.revokeAlbum("some-album-token");

Console.WriteLine(resp);
```

> **NOTE:** Once revoked, the URL for sharing is destroyed.  If the album is later shared again, the URL issued will be different.

### Get Restrictions

To get the list of restrictions applied to the server, you use the `getRestrictions` functions.

This will respond with an array of name, value entries describing the restrictions applied to the server.

> **NOTE:** Restrictions are cached for 10 minutes

```csharp
using Waifuvault;
var restrictions = await Waifuvault.Api.getRestrictions();

foreach(var restriction in restrictions.Restrictions)
{
    Console.WriteLine($"{restriction.type} : {restriction.value}");    
}
```

### Clear Restrictions

To clear the cached restrictions in the SDK, you use the `clearRestrictions` function.

This will remove the cached restrictions and a fresh copy will be downloaded at the next upload.

```csharp
using Waifuvault;
Waifuvault.Api.clearRestrictions();
```

### Get File Stats

To get general file stats for the server, you use the `getFileStats` function.

This takes no parameters and returns the number of files and the size of files on the server.

```csharp
using Waifuvault;
var stats = Waifuvault.Api.clearRestrictions();

console.WriteLine(stats.recordCount);
console.WriteLine(stats.recordSize);
```