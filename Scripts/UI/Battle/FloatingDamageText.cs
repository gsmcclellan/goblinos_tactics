using System.Threading.Tasks;
using Godot;
using Goblinos.Logging;
using Goblinos.Scripts.Util;

namespace Goblinos.Scripts.UI.Battle
{
    /// <summary>
    /// Displays floating combat text such as damage or healing,
    /// animates upward, fades out, and frees itself.
    /// </summary>
    public partial class FloatingDamageText : Node2D
    {
        [Export] private Label _label = null!;

        [Export] private float _riseDistance = 24.0f;
        [Export] private float _durationSeconds = 0.6f;
        [Export] private float _fadeDelayDuration = 0.3f;

        private readonly GobLogger _logger = GobLogManager.For<FloatingDamageText>();
        
        /// <summary>
        /// Configures the text and starts its animation.
        /// </summary>
        public async Task ShowValue(int value, bool isCritical = false, bool isHealing = false)
        {
            _logger.Log(
                $"ShowValue called with value={value}, isCritical={isCritical}, isHealing={isHealing}",
                GobLogSeverity.Info,
                GobLogCategory.UiNavigation
            );

            if (!DebugUtil.Require(_label != null, "FloatingDamageText label is not assigned.") ||
                !DebugUtil.Require(_durationSeconds > _fadeDelayDuration, "Full animation duration must be greater than fade delay"))
                return;

            _label.Text = isHealing ? $"+{value}" : value.ToString();

            if (isCritical)
                _label.Scale = Vector2.One * 1.25f;
            else
                _label.Scale = Vector2.One;
            
            var endPosition = Position + new Vector2(0.0f, -_riseDistance);
            var fadeDelay = Mathf.Clamp(_fadeDelayDuration, 0.0f, _durationSeconds);
            var fadeDuration = Mathf.Max(0.01f, _durationSeconds - fadeDelay);

            Modulate = Colors.White;

            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(this, "position", endPosition, _durationSeconds);
            
            if (fadeDelay > 0.0f)
                tween.Chain().TweenInterval(fadeDelay);
            
            tween.TweenProperty(this, "modulate:a", 0.0f, fadeDuration);

            await ToSignal(tween, Tween.SignalName.Finished);

            _logger.Log(
                "Floating damage text animation finished. Freeing node.",
                GobLogSeverity.Trace,
                GobLogCategory.UiNavigation
            );

            QueueFree();
        }
    }
}