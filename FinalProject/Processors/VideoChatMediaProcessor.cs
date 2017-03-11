using System;
using System.Threading.Tasks;
using FinalProject.Helpers;

namespace FinalProject.Processors
{
    public class VideoChatMediaProcessor : IVideoChatMediaProcessor
    {
        private const string ThumbnailFormat = "png";

        private const string InputVideoFormat = "webm";

        private const string OutputVideoFormat = "mp4";

        private const string OutputAudioFormat = "wav";

        private static readonly string VideoChatMediaFolderPath = AppDomain.CurrentDomain.BaseDirectory
                                                                  + @"\App_Data\Chats\{0}\";

        public VideoChatMediaProcessor(long chatId, float duration)
        {
            ChatId = chatId;
            Duration = duration;
        }

        public float Duration { get; set; }

        public long ChatId { get; }

        public async Task ProcessAsync()
        {
            var folderPath = string.Format(VideoChatMediaFolderPath, ChatId);

            var videoFolderPath = folderPath + @"\Video\";
            var outputVideoFolderPath = $@"{folderPath}\Video\VIDEO_OUTPUT.{OutputVideoFormat}";

            var audioFolderPath = folderPath + @"\Audio\";
            var outputAudioFolderPath = $@"{folderPath}\Audio\AUDIO_OUTPUT.{OutputAudioFormat}";

            var finalResultPath = FileSystemHelper.BuildPath(folderPath, $"{ChatId}.{OutputVideoFormat}");
            var thumbnailPath = $"{AppDomain.CurrentDomain.BaseDirectory}/VideoChatThumbs/{ChatId}.{ThumbnailFormat}";

            // Convert from WEBM to MP4
            await MediaHelper.ConvertFramesInFolderAsync(videoFolderPath, InputVideoFormat, OutputVideoFormat);

            // Create video file
            var videoTask = MediaHelper.ConcatVideoFramesInFolderAsync(videoFolderPath, Duration, outputVideoFolderPath);

            // Create audio file
            var audioTask = MediaHelper.ConcatAudioFramesInFolderAsync(audioFolderPath, outputAudioFolderPath);

            // Wait for video and audio process to finish
            await Task.WhenAll(videoTask, audioTask);

            // Create video + audio file
            await MediaHelper.MergeVideoAudioAsync(outputVideoFolderPath, outputAudioFolderPath, finalResultPath);

            // Generate thumbnail
            await MediaHelper.GenerateThumbnailFromVideoAsync(finalResultPath, thumbnailPath);
        }
    }
}