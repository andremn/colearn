using FinalProject.Helpers;
using FinalProject.Model;
using static FinalProject.Shared.Constants;

namespace FinalProject.Extensions
{
    public static class StudentExtensions
    {
        public static string GetProfilePicture(this StudentDataTransfer student)
        {
            if (string.IsNullOrEmpty(student?.ProfilePictureId))
            {
                return DefaultProfilePictureVirtualPath;
            }

            var picturePath = $"{ProfilePicturesFolderVirtualPath}/{student.ProfilePictureId}.png";

            return FileSystemHelper.FileExists(FileSystemHelper.GetAbsolutePath(picturePath))
                ? picturePath
                : DefaultProfilePictureVirtualPath;
        }

        public static string GetProfilePicture(this RecommendedStudentDataTransfer student)
        {
            if (string.IsNullOrEmpty(student?.ProfilePictureId))
            {
                return DefaultProfilePictureVirtualPath;
            }

            var picturePath = $"{ProfilePicturesFolderVirtualPath}/{student.ProfilePictureId}.png";

            return FileSystemHelper.FileExists(FileSystemHelper.GetAbsolutePath(picturePath))
                ? picturePath
                : DefaultProfilePictureVirtualPath;
        }
    }
}