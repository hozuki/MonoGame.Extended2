using System;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Demo.WinForms.WindowsDX.Test;
using MonoGame.Extended.WinForms;

namespace Demo.WinForms.WindowsDX;

[SupportedOSPlatform("windows7.0")]
public partial class Form1 : Form
{

    public Form1()
    {
        InitializeComponent();
        RegisterEventHandlers();
    }

    public int BottomHitCount { get; private set; }

    public event EventHandler BottomHitCountChanged;

    public void SetBallCount(int count)
    {
        while (count > _engine.BallCount)
        {
            _engine.AddBall();
        }

        while (count < _engine.BallCount)
        {
            _engine.RemoveBall();
        }
    }

    protected override bool ProcessKeyPreview(ref Message m)
    {
        gameControl.ProcessKeyMessage(ref m, true);
        return base.ProcessKeyPreview(ref m);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        gameControl.ProcessKeyMessage(ref msg, true);

        if (checkBox1.Checked)
        {
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void RegisterEventHandlers()
    {
        gameControl.GameInitialize += GameControl_GameInitialize;
        gameControl.GameLoadContents += GameControl_GameLoadContents;
        gameControl.GameUnloadContents += GameControl_GameUnloadContents;
        gameControl.GameUpdate += GameControl_GameUpdate;
        gameControl.GameDraw += GameControl_GameDraw;
        gameControl.GameDisposed += GameControl_GameDisposed;
        BottomHitCountChanged += HandleBottomHitCountChanged;
        numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
    }

    private void GameControl_GameDisposed(object sender, EventArgs e)
    {
    }

    private void GameControl_GameDraw(object sender, TimedEventArgs e)
    {
        _engine.Draw(e.GameTime);
    }

    private void GameControl_GameUpdate(object sender, TimedEventArgs e)
    {
        _engine.Update(e.GameTime);

        if (BottomHitCount != _engine.BottomHitCount)
        {
            BottomHitCount = _engine.BottomHitCount;
            OnBottomHitCountChanged(EventArgs.Empty);
        }
    }

    private void GameControl_GameUnloadContents(object sender, EventArgs e)
    {
    }

    private void GameControl_GameLoadContents(object sender, EventArgs e)
    {
    }

    private void GameControl_GameInitialize(object sender, EventArgs e)
    {
        InputManager.Initialize(new ControlInputManager());

        _engine = new Engine(gameControl.GraphicsDeviceService);
        _engine.Initialize();
    }

    private void HandleBottomHitCountChanged(object sender, EventArgs e)
    {
        textBox1.Text = BottomHitCount.ToString();
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
        SetBallCount((int)numericUpDown1.Value);
    }

    private void OnBottomHitCountChanged(EventArgs e)
    {
        BottomHitCountChanged?.Invoke(this, e);
    }

    private Engine _engine;

}
