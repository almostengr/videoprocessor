using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class AddFileToTarballException : VideoProcessorException
{
    public AddFileToTarballException()
    {
    }

    public AddFileToTarballException(string message) : base(message)
    {
    }
}