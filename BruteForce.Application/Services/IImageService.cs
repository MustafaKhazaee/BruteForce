
using BruteForce.Application.Models;

namespace BruteForce.Application.Services;

public interface IImageService
{
    string SaveImage(ImageConfigs imageConfigs);
}
