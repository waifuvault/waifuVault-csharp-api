using Moq;
using Moq.Protected;

namespace tests;

public class waifuvaultTests
{
    // Mocks
    public Mock<HttpMessageHandler> okResponseNumeric = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseNumericProtected = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseHuman = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> deleteTrue = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> badRequest = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    public waifuvaultTests() {
        setupMocks();
    }
    
    private void setupMocks() {
        okResponseNumeric.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"protected\":false, \"retentionPeriod\":100}")
            })
            .Verifiable();

        okResponseNumericProtected.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"protected\":true, \"retentionPeriod\":100}")
            })
            .Verifiable();

        okResponseHuman.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"protected\":false, \"retentionPeriod\":\"10 minutes\"}")
            })
            .Verifiable();

        badRequest.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("{\"name\": \"BAD_REQUEST\", \"message\": \"Error Test\", \"status\": 400}")
            })
            .Verifiable();

        deleteTrue.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("true")
            })
            .Verifiable();
    }
    
    [Fact]
    public void SimpleTest()
    {
        var uploadfile = new Waifuvault.FileUpload("filetarget.png");
        Assert.Equal("filetarget.png",uploadfile.filename);
    }

    [Fact]
    public async void TestUpload() {
        // Given
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumeric.Object);
        var upload = new Waifuvault.FileUpload("https://walker.moe/assets/sunflowers.png",expires:"10m");
        
        // Then
        var response = await Waifuvault.Api.uploadFile(upload);
        
        // When
        okResponseNumeric.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }
    
    [Fact]
    public async void TestFileInfo() {
        // Given
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseHuman.Object);
        
        // Then
        var response = await Waifuvault.Api.fileInfo("test-token",true);
        
        // When
        okResponseHuman.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.fileprotected);
        Assert.Equal("10 minutes", response.retentionPeriod);
    }

    [Fact]
    public async void TestUpdateInfo() {
        // Given
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumericProtected.Object);
        
        // Then
        var response = await Waifuvault.Api.fileUpdate("test-token","dangerWaifu");
        
        // When
        okResponseNumericProtected.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(true,response.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async void TestDelete() {
        // Given
        Waifuvault.Api.customHttpClient = new HttpClient(deleteTrue.Object);
        
        // Then
        var response = await Waifuvault.Api.deleteFile("test-token");
        
        // When
        deleteTrue.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
        Assert.True(response);
    }
}