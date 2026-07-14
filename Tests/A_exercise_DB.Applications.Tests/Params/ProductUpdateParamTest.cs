using A_exercise_DB.Applications.Params;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Applications.Tests.Params;

[TestClass]
public sealed class ProductUpdateParamTest
{
    [TestMethod(DisplayName = "全ての入力値を指定した場合、各プロパティに値が設定される")]
    public void Constructor_WithAllValues_ShouldSetAllProperties()
    {
        // Arrange
        var productUuid = Guid.NewGuid();
        var productCategoryUuid = Guid.NewGuid();
        const string name = "テスト商品";
        const int price = 1_000;
        const int stockQuantity = 10;
        const string imageFileName = "test-image.png";
        const string imageContentType = "image/png";
        const long imageLength = 1_024;

        using var imageContent = new MemoryStream([1, 2, 3]);

        // Act
        var param = new ProductUpdateParam(
            productUuid,
            name,
            price,
            productCategoryUuid,
            stockQuantity,
            imageContent,
            imageFileName,
            imageContentType,
            imageLength);

        // Assert
        Assert.AreEqual(productUuid, param.ProductUuid);
        Assert.AreEqual(name, param.Name);
        Assert.AreEqual(price, param.Price);
        Assert.AreEqual(productCategoryUuid, param.ProductCategoryUuid);
        Assert.AreEqual(stockQuantity, param.StockQuantity);
        Assert.AreSame(imageContent, param.ImageContent);
        Assert.AreEqual(imageFileName, param.ImageFileName);
        Assert.AreEqual(imageContentType, param.ImageContentType);
        Assert.AreEqual(imageLength, param.ImageLength);
    }

    [TestMethod(DisplayName = "ImageLengthを省略した場合、初期値として0が設定される")]
    public void Constructor_WithoutImageLength_ShouldSetImageLengthToZero()
    {
        // Arrange
        var productUuid = Guid.NewGuid();
        var productCategoryUuid = Guid.NewGuid();

        using var imageContent = new MemoryStream();

        // Act
        var param = new ProductUpdateParam(
            productUuid,
            "テスト商品",
            1_000,
            productCategoryUuid,
            10,
            imageContent,
            "test-image.png",
            "image/png");

        // Assert
        Assert.AreEqual(0L, param.ImageLength);
    }
}