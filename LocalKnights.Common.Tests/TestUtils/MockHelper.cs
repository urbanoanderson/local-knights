using LocalKnights.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LocalKnights.Common.Tests.TestUtils;

public static class MockHelper
{
    public static ILogger CreateMockLogger()
    {
        return new Mock<ILogger>().Object;
    }

    public static IRandomService CreateMockRandomService()
    {
        Mock<IRandomService> mock = new Mock<IRandomService>();

        mock.Setup(service => service.Next()).Returns(0);
        mock.Setup(service => service.Next(It.IsAny<int>())).Returns(0);
        mock.Setup(service => service.Next(It.IsAny<int>(), It.IsAny<int>())).Returns((int minValue, int maxValue) => minValue);
        mock.Setup(service => service.NextDouble()).Returns(0d);
        mock.Setup(service => service.NextDouble(It.IsAny<double>(), It.IsAny<double>())).Returns((double minValue, double maxValue) => minValue);
        mock.Setup(service => service.NextBool()).Returns(false);

        return mock.Object;
    }

    public static IFileSystemService CreateMockFileSystemService()
    {
        Mock<IFileSystemService> mock = new Mock<IFileSystemService>();

        mock.Setup(service => service.FileExists(It.IsAny<string>())).Returns(false);
        mock.Setup(service => service.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
        mock.Setup(service => service.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(Array.Empty<string>());
        mock.Setup(service => service.GetAppDataDirectory()).Returns(string.Empty);
        mock.Setup(service => service.GetClubsDirectory()).Returns(string.Empty);
        mock.Setup(service => service.GetExistingDbFiles()).Returns(Array.Empty<string>());
        mock.Setup(service => service.GetDbFilePath(It.IsAny<string>())).Returns(string.Empty);

        return mock.Object;
    }
}