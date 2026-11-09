namespace WinUia.Recording.UnitTests.Ffmpeg;

public class FfmpegArgumentsTests
{
    [Test]
    public void Defaults_capture_the_desktop_at_15_fps_with_the_mouse_to_a_fragmented_mp4()
    {
        var args = FfmpegArguments.Build(@"C:\out\test.mp4", new FfmpegRecorder(), area: null);

        Assert.That(args, Is.EqualTo(new[]
        {
            "-hide_banner", "-loglevel", "info", "-nostats",
            "-f", "gdigrab", "-framerate", "15", "-draw_mouse", "1", "-i", "desktop",
            "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2",
            "-pix_fmt", "yuv420p",
            "-preset", "ultrafast",
            "-movflags", "+frag_keyframe+empty_moov",
            "-y", @"C:\out\test.mp4",
        }));
    }

    [Test]
    public void Frame_rate_and_mouse_options_are_applied()
    {
        var args = FfmpegArguments.Build("a.mp4", new FfmpegRecorder { FrameRate = 30, DrawMouse = false }, area: null);

        Assert.That(args, Does.Contain("30"));
        Assert.That(args[(args.ToList().IndexOf("-draw_mouse") + 1)], Is.EqualTo("0"));
    }

    [Test]
    public void An_area_adds_offsets_and_size_before_the_input_and_supports_negative_offsets()
    {
        var args = FfmpegArguments.Build("a.mp4", new FfmpegRecorder(), new ScreenRect(-1920, 100, 0, 1180)).ToList();

        Assert.That(args[args.IndexOf("-offset_x") + 1], Is.EqualTo("-1920"));
        Assert.That(args[args.IndexOf("-offset_y") + 1], Is.EqualTo("100"));
        Assert.That(args[args.IndexOf("-video_size") + 1], Is.EqualTo("1920x1080"));
        Assert.That(args.IndexOf("-video_size"), Is.LessThan(args.IndexOf("-i")));
    }

    [Test]
    public void Without_an_area_no_offsets_or_size_are_passed()
    {
        var args = FfmpegArguments.Build("a.mp4", new FfmpegRecorder(), area: null);

        Assert.That(args, Does.Not.Contain("-video_size").And.Not.Contain("-offset_x"));
    }

    [Test]
    public void An_empty_area_throws()
    {
        Assert.Throws<ArgumentException>(() => FfmpegArguments.Build("a.mp4", new FfmpegRecorder(), new ScreenRect(0, 0, 0, 0)));
    }

    [Test]
    public void The_output_path_is_the_last_argument_unquoted()
    {
        var args = FfmpegArguments.Build(@"C:\with space\a.mp4", new FfmpegRecorder(), area: null);

        Assert.That(args[^1], Is.EqualTo(@"C:\with space\a.mp4"));
    }

    [Test]
    public void A_frame_rate_of_zero_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FfmpegArguments.Build("a.mp4", new FfmpegRecorder { FrameRate = 0 }, area: null));
    }
}
