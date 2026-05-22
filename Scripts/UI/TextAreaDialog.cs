using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

namespace Goblinos.Scripts.UI;

public partial class TextAreaDialog : Control
{
    /** Events */
    [Signal]
    public delegate void ClosedEventHandler();
    
    /** Components */
    [Export] private Label _contentLabel;
    [Export] private Label _titleLabel;
    [Export] private Button _closeButton;

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public override void _Ready()
    {
        Debug.Assert(_contentLabel != null, $"[{nameof(TextAreaDialog)}] [{nameof(_contentLabel)}] Not Bound");
        Debug.Assert(_titleLabel != null, $"[{nameof(TextAreaDialog)}] [{nameof(_titleLabel)}] Not Bound");
        Debug.Assert(_closeButton != null, $"[{nameof(TextAreaDialog)}] [{nameof(_closeButton)}] Not Bound");
        
        _closeButton.Pressed += Close;
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------
    
    public void Close()
    {
        EmitSignal(SignalName.Closed);
        QueueFree();
    }
    
    public void Show(string title, string content)
    {
        SetTitle(title);
        SetContent(content);
        SetVisible(true);
    }
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    private void SetTitle(string txt)
    {
        if (_titleLabel != null)
            _titleLabel.Text = txt;
    }

    private void SetContent(string txt)
    {
        if (_contentLabel != null)
            _contentLabel.Text = txt;
    }
}