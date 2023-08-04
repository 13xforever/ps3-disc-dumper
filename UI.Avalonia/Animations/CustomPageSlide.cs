using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;

namespace UI.Avalonia.Animations;

public class CustomPageSlide : PageSlide
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageSlide"/> class.
    /// </summary>
    public CustomPageSlide()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSlide"/> class.
    /// </summary>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="orientation">The axis on which the animation should occur</param>
    /// <param name="reverseAnimation">Whether to reverse the animation direction</param>
    public CustomPageSlide(TimeSpan duration, SlideAxis orientation = SlideAxis.Horizontal, bool reverseAnimation = false)
    {
        Duration = duration;
        Orientation = orientation;
        ReverseAnimation = reverseAnimation;
    }
    
    public bool ReverseAnimation { get; set; }

    /// <inheritdoc />
    public override Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (ReverseAnimation)
            forward = !forward;

        return base.Start(from, to, forward, cancellationToken);
    }
}
