using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

namespace Goblinos.Scripts.UI.Combat;

public partial class ExperienceProgress : Panel
{
    [Export] private ProgressBar _progressBar;
    [Export] private Label _titleLabel;
    [Export] private Label _expValueLabel;
    
    public int StartValue;
    public int EndValue;
    
    public override void _Ready()
    {
        Debug.Assert(_progressBar != null, $"[{nameof(ExperienceProgress)}] - [{nameof(_progressBar)}] element not bound.");
        Debug.Assert(_titleLabel != null, $"[{nameof(ExperienceProgress)}] - [{nameof(_titleLabel)}] element not bound.");
        Debug.Assert(_expValueLabel != null, $"[{nameof(ExperienceProgress)}] - [{nameof(_expValueLabel)}] element not bound.");
    }

    public async Task Show(string title, int startVal, int endVal)
    {
        SetTitle(title);
        StartValue = startVal;
        EndValue = endVal;

        var tcs = new TaskCompletionSource();
        await AnimateExpBar(startVal, endVal);
        
        await ToSignal(
            GetTree().CreateTimer(1f),
            SceneTreeTimer.SignalName.Timeout
        );
        
        QueueFree();
        // return Task.CompletedTask;
    }

    public Task ShowInstant(string title, int startVal, int endVal)
    {
        SetTitle(title);
        StartValue = startVal;
        EndValue = endVal;
        SetExpValue(endVal);
        QueueFree();
        return Task.CompletedTask;
    }
    
    private Task AnimateExpBar(int startVal, int endVal)
    {
        var duration = 1f;

        var tween = CreateTween();
    
        // Animate the progress bar value
        tween.TweenProperty(_progressBar, "value", endVal, duration)
            .From(startVal)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);

        // Keep the label in sync with the bar during animation
        tween.Parallel().TweenMethod(
            Callable.From((float v) => SetExpValue((int)v)),
            startVal,
            endVal,
            duration
        );

        var tcs = new TaskCompletionSource();
        tween.Finished += () => tcs.SetResult();
        return tcs.Task;
    }

    private void SetTitle(string val)
    {
        if (_titleLabel != null)
            _titleLabel.Text = val;
    }
    
    private void SetExpValue(int val)
    {
        _progressBar.Value = val;
        if (_expValueLabel != null)
            _expValueLabel.Text = val.ToString();
    }
}