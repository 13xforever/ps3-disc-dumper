using System;
using System.Globalization;
using Avalonia.Animation;
using Avalonia.Data.Converters;
using UI.Avalonia.Animations;

namespace UI.Avalonia.Converters;

public class PageTransitionConverter: IValueConverter
{
    // https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing
    private static readonly TimeSpan AnimationTime = TimeSpan.FromMilliseconds(167);
    private static readonly IPageTransition Normal = new CustomPageSlide(AnimationTime, PageSlide.SlideAxis.Horizontal, false);
    private static readonly IPageTransition Reversed = new CustomPageSlide(AnimationTime, PageSlide.SlideAxis.Horizontal, true);
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Normal : Reversed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}