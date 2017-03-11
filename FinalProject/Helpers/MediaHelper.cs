using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace FinalProject.Helpers
{
    public static class MediaHelper
    {
        private static readonly string BaseAppDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static async Task ConcatAudioFramesInFolderAsync(string folderPath, string output)
        {
            var framesFiles = FileSystemHelper.GetFilesNameWithinFolder(folderPath, "*.wav");
            var buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (var frameFile in framesFiles)
                {
                    using (var reader = new WaveFileReader(frameFile))
                    {
                        if (waveFileWriter == null)
                        {
                            waveFileWriter = new WaveFileWriter(output, reader.WaveFormat);
                        }
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                            {
                                throw new InvalidOperationException(
                                    "Cannot concatenate WAV fles that don't share the same format");
                            }
                        }

                        int read;

                        while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await waveFileWriter.WriteAsync(buffer, 0, read);
                        }
                    }
                }
            }
            finally
            {
                waveFileWriter?.Dispose();
            }
        }

        public static async Task ConvertFramesInFolderAsync(string folderPath, string currentFormat, string outputFormat)
        {
            await Task.Run(() =>
            {
                var frames = FileSystemHelper.GetFilesNameWithinFolder(folderPath, "*.webm");

                foreach (var frame in frames)
                {
                    var convertMp4Command = $"-i {frame} {Path.GetFileNameWithoutExtension(frame)}.mp4";

                    ExecuteFfMpegCommand(convertMp4Command, folderPath);
                    FileSystemHelper.DeleteFile(frame);
                }
            });
        }

        public static async Task ConcatVideoFramesInFolderAsync(string folderPath, float videoDuration, string output)
        {
            await Task.Run(() =>
            {
                var framesFileName = folderPath + "/frames.txt";
                var framesFileCommand = $"(for %i in (*.mp4) do @echo file '%i') > {framesFileName}";
                var ffmpegCommand = $"-f concat -i frames.txt -c copy {output}";

                ExecuteCommand(framesFileCommand, folderPath);
                ExecuteFfMpegCommand(ffmpegCommand, folderPath);
            });
        }

        public static async Task MergeVideoAudioAsync(string videoFilePath, string audioFilePath, string outputFilePath)
        {
            await Task.Run(() =>
            {
                var tempFileName = Path.GetRandomFileName() + Path.GetExtension(outputFilePath);
                var tempFileDirectory = Path.GetDirectoryName(outputFilePath) ?? outputFilePath;
                var tempVideo = Path.Combine(tempFileDirectory, tempFileName);
                var removeAudioCommand = $"-i {videoFilePath} -an {tempVideo}";
                var mergeAudioCommand = $"-i {tempVideo} -i {audioFilePath} -shortest {outputFilePath}";

                ExecuteFfMpegCommand(removeAudioCommand);
                ExecuteFfMpegCommand(mergeAudioCommand);
                FileSystemHelper.DeleteFile(tempVideo);
            });
        }

        public static async Task GenerateThumbnailFromVideoAsync(string videoFilePath, string outputPath,
            TimeSpan? thumbnailAt = null)
        {
            await Task.Run(() =>
            {
                if (thumbnailAt == null)
                {
                    thumbnailAt = TimeSpan.FromSeconds(5);
                }

                var thumbnailCommand = $"ffmpeg -i {videoFilePath} -ss {thumbnailAt.Value.ToString(@"hh\:mm\:ss\.ff")} -vframes 1 {outputPath}";

                ExecuteCommand(thumbnailCommand);
            });
        }

        private static void ExecuteCommand(
            string command,
            string workingDirectory = null,
            bool showWindow = false)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory,
                    CreateNoWindow = showWindow,
                };

                process.Start();
                process.WaitForExit();
            }
        }

        private static void ExecuteFfMpegCommand(
            string command,
            string workingDirectory = null,
            bool showWindow = false)
        {
            using (var process = new Process())
            {
                var executableFilePath = Path.Combine(BaseAppDirectory, "ffmpeg.exe");

                if (showWindow)
                {
                    process.StartInfo = new ProcessStartInfo("cmd.exe", $"/K {executableFilePath} " + command)
                    {
                        WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory
                    };
                }
                else
                {
                    process.StartInfo = new ProcessStartInfo(executableFilePath, command)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory
                    };
                }

                process.Start();
                process.WaitForExit();
            }
        }
    }
}