using System.Globalization;

namespace WinUia.Recording;

internal static class FfmpegArguments
{
    public static IReadOnlyList<string> Build(string outputPath, FfmpegRecorder recorder, ScreenRect? area)
    {
        if (recorder.FrameRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recorder), "FrameRate must be greater than zero.");
        }

        if (area is { IsEmpty: true })
        {
            throw new ArgumentException("Area must not be empty.", nameof(area));
        }

        var arguments = new List<string>
        {
            "-hide_banner",
            "-loglevel", "info",
            "-nostats",
            "-f", "gdigrab",
            "-framerate", recorder.FrameRate.ToString(CultureInfo.InvariantCulture),
            "-draw_mouse", recorder.DrawMouse ? "1" : "0",
        };

        if (area is { } rect)
        {
            // Offsets may be negative for monitors left of or above the primary one.
            arguments.AddRange(
            [
                "-offset_x", rect.Left.ToString(CultureInfo.InvariantCulture),
                "-offset_y", rect.Top.ToString(CultureInfo.InvariantCulture),
                "-video_size", $"{rect.Width.ToString(CultureInfo.InvariantCulture)}x{rect.Height.ToString(CultureInfo.InvariantCulture)}",
            ]);
        }

        arguments.AddRange(
        [
            "-i", "desktop",
            // H.264 with yuv420p needs even dimensions.
            "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2",
            "-pix_fmt", "yuv420p",
            // Screen capture shares the machine with the tests, so favour low CPU over file size.
            "-preset", "ultrafast",
            // Fragmented mp4: a killed process still leaves a playable file, minus whatever came after the last keyframe.
            "-movflags", "+frag_keyframe+empty_moov",
            "-y",
            outputPath,
        ]);
        return arguments;
    }
}
