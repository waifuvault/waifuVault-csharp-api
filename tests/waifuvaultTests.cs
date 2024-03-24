namespace tests;

public class waifuvaultTests
{
    [Fact]
    public void SimpleTest()
    {
        var uploadfile = new Waifuvault.FileUpload("filetarget.png");
        Assert.Equal("filetarget.png",uploadfile.filename);
    }
}