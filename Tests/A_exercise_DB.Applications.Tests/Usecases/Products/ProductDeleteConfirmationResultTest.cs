using A_exercise_DB.Applications.Usecases.Products;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;

/// <summary>
/// ProductDeleteConfirmationResultの単体テスト
/// </summary>
[TestClass]
[TestCategory("Applications/Usecases/Products")]
public class ProductDeleteConfirmationResultTest
{
    /// <summary>
    /// 商品削除確認結果を生成できること
    /// </summary>
    [TestMethod(DisplayName = "ProductDeleteConfirmationResult_商品削除確認結果を生成できる")]
    public void ProductDeleteConfirmationResult_CanCreate()
    {
        // Arrange
        var productUuid = Guid.NewGuid();

        // Act
        var result = new ProductDeleteConfirmationResult(
            productUuid,
            "テスト商品",
            1000,
            50,
            "食品",
            "/images/test.png");

        // Assert
        Assert.AreEqual(productUuid, result.ProductUuid);
        Assert.AreEqual("テスト商品", result.ProductName);
        Assert.AreEqual(1000, result.Price);
        Assert.AreEqual(50, result.Quantity);
        Assert.AreEqual("食品", result.CategoryName);
        Assert.AreEqual("/images/test.png", result.ImageUrl);
    }

    /// <summary>
    /// 商品画像URLがnullでも商品削除確認結果を生成できること
    /// </summary>
    [TestMethod(DisplayName = "ProductDeleteConfirmationResult_ImageUrlがnullでも生成できる")]
    public void ProductDeleteConfirmationResult_WhenImageUrlIsNull_CanCreate()
    {
        // Arrange
        var productUuid = Guid.NewGuid();

        // Act
        var result = new ProductDeleteConfirmationResult(
            productUuid,
            "テスト商品",
            1000,
            50,
            "食品",
            null);

        // Assert
        Assert.AreEqual(productUuid, result.ProductUuid);
        Assert.AreEqual("テスト商品", result.ProductName);
        Assert.AreEqual(1000, result.Price);
        Assert.AreEqual(50, result.Quantity);
        Assert.AreEqual("食品", result.CategoryName);
        Assert.IsNull(result.ImageUrl);
    }
}