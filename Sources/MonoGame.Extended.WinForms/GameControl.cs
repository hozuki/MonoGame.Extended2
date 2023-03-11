using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.WinForms;

[SupportedOSPlatform("windows")]
public abstract class GameControl : GraphicsDeviceControl
{

    protected GameControl()
    {
        UpdateKeysInPaint = false;
    }

    public event EventHandler<TimedEventArgs>? GameUpdate;

    public event EventHandler<TimedEventArgs>? GameDraw;

    public event EventHandler<EventArgs>? GameInitialize;

    public event EventHandler<EventArgs>? GameLoadContents;

    public event EventHandler<EventArgs>? GameUnloadContents;

    public event EventHandler<EventArgs>? GameDisposed;

    [Browsable(true)]
    [DefaultValue(true)]
    [Description("Gets/sets whether the game update should be suspended when parent form is inactive.")]
    [Category("MonoGame")]
    public bool SuspendOnFormInactive { get; set; } = true;

    public bool IsActive => _isActive;

    protected override void OnInitialize()
    {
        _stopwatch = Stopwatch.StartNew();

        Application.Idle += Application_Idle;

        var parentForm = FindParentForm();

        if (parentForm is not null)
        {
            parentForm.Activated += ParentForm_Activated;
            parentForm.Deactivate += ParentForm_Deactivated;
            _parentForm = parentForm;
        }

        GameInitialize?.Invoke(this, EventArgs.Empty);

        OnLoadContents();
    }

    protected virtual void OnUpdate(GameTime gameTime)
    {
        if (!IsDesignMode)
        {
            Guard.ArgumentNotNull(gameTime, nameof(gameTime));

            GameUpdate?.Invoke(this, new TimedEventArgs(gameTime));
        }
    }

    protected virtual void OnDraw(GameTime? gameTime)
    {
        if (gameTime is not null)
        {
            GameDraw?.Invoke(this, new TimedEventArgs(gameTime));
        }
    }

    protected sealed override void OnDraw()
    {
        if (!IsDesignMode)
        {
            OnDraw(_gameTime);
        }
    }

    protected override void Dispose(bool disposing)
    {
        OnUnloadContents();

        GameDisposed?.Invoke(this, EventArgs.Empty);

        Application.Idle -= Application_Idle;

        if (_parentForm is not null)
        {
            _parentForm.Activated -= ParentForm_Activated;
            _parentForm.Deactivate -= ParentForm_Deactivated;
        }

        base.Dispose(disposing);
    }

    protected virtual void OnLoadContents()
    {
        GameLoadContents?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnUnloadContents()
    {
        GameUnloadContents?.Invoke(this, EventArgs.Empty);
    }

    private void GameLoop()
    {
        if (SuspendOnFormInactive && !IsActive)
        {
            return;
        }

        UpdateKeys();

        var elapsed = GetStopwatchElapsed();
        Debug.Assert(elapsed >= _elapsed, "elapsed >= _elapsed");
        _gameTime = new GameTime(elapsed, elapsed - _elapsed);
        _elapsed = elapsed;

        OnUpdate(_gameTime);
        Invalidate();
    }

    private void ParentForm_Activated(object? sender, EventArgs e)
    {
        _isActive = true;
        _elapsed = GetStopwatchElapsed();
    }

    private void ParentForm_Deactivated(object? sender, EventArgs e)
    {
        _isActive = false;
    }

    private void Application_Idle(object? sender, EventArgs e)
    {
        GameLoop();
    }

    // TODO: This method always returns null when the control is used in WPF interop.
    private Form? FindParentForm()
    {
        var parent = Parent;

        while (parent is not null)
        {
            if (parent is Form parentForm)
            {
                return parentForm;
            }

            parent = parent.Parent;
        }

        return null;
    }

    private TimeSpan GetStopwatchElapsed()
    {
        return _stopwatch?.Elapsed ?? TimeSpan.Zero;
    }

    private Stopwatch? _stopwatch;
    private GameTime? _gameTime;
    private TimeSpan _elapsed;

    private bool _isActive;
    private Form? _parentForm;

}
