# waifuvault-C#-api

This contains the official API bindings for uploading, deleting and obtaining files
with [waifuvault.moe](https://waifuvault.moe/). Contains a full up to date API for interacting with the service

## Installation

```sh
dotnet add package Waifuvault
```

## Usage

This API contains 4 interactions:

1. Upload File
2. Get File Info
3. Delete File
4. Get File

The package is namespaced to `Waifuvault`, so to import it, simply:

```cs
using Waifuvault;
```

### Upload File

To Upload a file, use the `uploadFile` function. This function takes the following options as an object:

| Option         | Type         | Description                                                 | Required       | Extra info                       |
|----------------|--------------|-------------------------------------------------------------|----------------|----------------------------------|
| `filename`     | `string `    | The path to the file to upload                              | true if File   | File path                        |
| `url`          | `string`     | The URL of the file to target                               | true if URL    | Filename with extension          |
| `buffer`       | `byte array` | Byte array containing file to upload                        | true if buffer | Needs filename set also          |
| `expires`      | `string`     | A string containing a number and a unit (1d = 1day)         | false          | Valid units are `m`, `h` and `d` |
| `hideFilename` | `boolean`    | If true, then the uploaded filename won't appear in the URL | false          | Defaults to `false`              |
| `password`     | `string`     | If set, then the uploaded file will be encrypted            | false          |                                  |
| `ct`           | `canceltoken`| An optional cancellation token that can be passed in        | false          | Standard cancellation token      |

Using a URL:

```cs
using Waifuvault;

var upload_file = new Waifuvault.FileUpload("https://waifuvault.moe/assets/custom/images/08.png");
var upload_resp = await Waifuvault.Api.uploadFile(upload_file);

Console.WriteLine(upload_resp.url);
```

Using a file path:

```cs
using Waifuvault;

var upload_file = new Waifuvault.FileUpload("../aCoolFile.png");
var upload_resp = await Waifuvault.Api.uploadFile(upload_file);

Console.WriteLine(upload_resp.url);
```

Using a buffer:

```cs
using Waifuvault;
using System.IO;

byte[] buffer = File.ReadAllBytes("./aCoolFile.png");
var upload_file = new Waifuvault.FileUpload(buffer,"aCoolFile.png");
var upload_resp = await Waifuvault.Api.uploadFile(upload_file);

Console.WriteLine(upload_resp.url);
```

Cancelable with a file:

```cs
using Waifuvault;

var cts = new CancellationTokenSource(2000);  // Auto cancel in 2s
var cancel_file = new Waifuvault.FileUpload("./largeFile.mkv");
try {
    var cancelled = await Waifuvault.Api.uploadFile(cancel_file,cts.Token);
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
var token_info = await Waifuvault.Api.fileInfo(upload_resp.token,false);
Console.WriteLine(token_info.url);
Console.WriteLine(token_info.retentionPeriod);
```

Human-readable timestamp:

```cs
using Waifuvault;
var token_info = await Waifuvault.Api.fileInfo(upload_resp.token,true);
Console.WriteLine(token_info.url);
Console.WriteLine(token_info.retentionPeriod);
```

### Delete File

To delete a file, you must supply your token to the `deletefile` function.

This function takes the following options as parameters:

| Option  | Type     | Description                              | Required | Extra info |
|---------|----------|------------------------------------------|----------|------------|
| `token` | `string` | The token of the file you wish to delete | true     |            |
| `ct`    | `canceltoken`| An optional cancellation token that can be passed in            | false    | Standard cancellation token |

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