using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.WinForms.Input;
using MonoGame.Extended.WinForms.Interop;
using Keys = System.Windows.Forms.Keys;

namespace MonoGame.Extended.WinForms {
    public abstract class GraphicsDeviceControl : Control {

        protected GraphicsDeviceControl() {
            IsDesignMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            _serviceContainer = new ServiceContainer();
            _keyState = new List<Microsoft.Xna.Framework.Input.Keys>();

            SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, false);
        }

        [Browsable(false)]
        public GraphicsDevice GraphicsDevice => _graphicsDeviceService.GraphicsDevice;

        [Browsable(false)]
        public GraphicsDeviceWrapper GraphicsDeviceService => _graphicsDeviceService;

        [Browsable(false)]
        public ServiceContainer ServiceContainer => _serviceContainer;

        public event EventHandler<EventArgs> ControlInitialized;

        public event EventHandler<EventArgs> ControlInitializing;

        [Browsable(false)]
        public bool IsDesignMode { get; }

        [Browsable(true)]
        [DefaultValue(GraphicsProfile.HiDef)]
        [Description("Gets/sets the graphics profile that its GraphicsDevice uses.")]
        [Category("MonoGame")]
        public GraphicsProfile Profile { get; set; } = GraphicsProfile.HiDef;

        /// <summary>
        /// Processes a keyboard message.
        /// </summary>
        /// <param name="m">The keyboard message.</param>
        /// <param name="isDirectlyFromForm">
        /// <para>Whether this message is directly passed from form, such as in <see cref="Form.ProcessKeyPreview"/> or <see cref="Form.ProcessCmdKey"/>.</para>
        /// <para>Warning: Incorrectly setting this value will lead to a <see cref="StackOverflowException"/>.</para>
        /// </param>
        /// <returns><see langword="true"/> if the message is fully processed, otherwise <see langword="false"/>.</returns>
        public bool ProcessKeyMessage(ref Message m, bool isDirectlyFromForm) {
            if (m.Msg == NativeConstants.WM_KEYDOWN) {
                var xkey = XnaKeyboardHelper.ToXna((Keys)m.WParam);

                if (!_keyState.Contains(xkey)) {
                    _keyState.Add(xkey);
                }
            } else if (m.Msg == NativeConstants.WM_KEYUP) {
                var xnaKey = XnaKeyboardHelper.ToXna((Keys)m.WParam);

                if (_keyState.Contains(xnaKey)) {
                    _keyState.Remove(xnaKey);
                }
            }

            bool result;

            if (!isDirectlyFromForm) {
                // Yep possible StackOverflowException.
                result = base.ProcessKeyMessage(ref m);
            } else {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// To enable the backend's full life cycle, this property must be set in constructor.
        /// </summary>
        [CanBeNull]
        protected IWindowBackend WindowBackend { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e) {
            // Don't paint the background.
        }

        protected abstract void OnInitialize();

        protected abstract void OnDraw();

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (IsDesignMode) {
                var clientRectangle = ClientRectangle;

                Debug.Assert(_backgroundBrush != null, nameof(_backgroundBrush) + " != null");
                Debug.Assert(_borderPen != null, nameof(_borderPen) + " != null");

                e.Graphics.FillRectangle(_backgroundBrush, clientRectangle);

                clientRectangle.Width -= 1;
                clientRectangle.Height -= 1;
                e.Graphics.DrawRectangle(_borderPen, clientRectangle);

                return;
            }

            if (UpdateKeysInPaint) {
                UpdateKeys();
            }

            _errorTextDrawn = false;

            var beginDrawError = BeginDraw();

            if (beginDrawError == DeviceResetError.None) {
                OnDraw();
                EndDraw();
            } else {
                string errorText;

                switch (beginDrawError) {
                    case DeviceResetError.DeviceLost:
                        errorText = "Graphics device lost.";
                        break;
                    case DeviceResetError.Failed:
                        errorText = "Resetting graphics device failed.";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                DrawErrorText(e.Graphics, errorText);
            }
        }

        protected override void OnCreateControl() {
            base.OnCreateControl();

            if (IsDesignMode) {
                _backgroundBrush = new SolidBrush(Color.Gray);
                _borderPen = new Pen(ForeColor);

                return;
            }

            var clientSize = ClientSize;

            _graphicsDeviceService = GraphicsDeviceWrapper.AddRef(Handle, clientSize.Width, clientSize.Height, Profile);
            _serviceContainer.AddService<IGraphicsDeviceService>(_graphicsDeviceService);

            WindowBackend?.Initialize(this);

            ControlInitializing?.Invoke(this, EventArgs.Empty);
            OnInitialize();
            ControlInitialized?.Invoke(this, EventArgs.Empty);

            if (!IsDesignMode) {
                Mouse.WindowHandle = Handle;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e) {
            if (!IsDesignMode) {
                Mouse.WindowHandle = IntPtr.Zero;
            }

            base.OnHandleDestroyed(e);
        }

        protected void UpdateKeys() {
            ControlKeyboard.SetKeys(_keyState);
        }

        protected override void OnClientSizeChanged(EventArgs e) {
            base.OnClientSizeChanged(e);
            WindowBackend?.WindowSizeChanged(this);
        }

        protected override void Dispose(bool disposing) {
            if (IsDesignMode) {
                _backgroundBrush?.Dispose();
                _borderPen?.Dispose();
            }

            WindowBackend?.Dispose();

            if (!IsDesignMode) {
                _graphicsDeviceService.Release(disposing);
            }

            base.Dispose(disposing);
        }

        protected bool UpdateKeysInPaint { get; set; } = true;

        private DeviceResetError BeginDraw() {
            if (_graphicsDeviceService == null) {
                return DeviceResetError.None;
            }

            var deviceResetError = HandleDeviceReset();

            if (deviceResetError != DeviceResetError.None) {
                return deviceResetError;
            }

            var clientSize = ClientSize;
            var graphicsDevice = GraphicsDevice;

            WindowBackend?.PrepareDraw(this, graphicsDevice.PresentationParameters);

            var viewport = new Viewport();

            viewport.X = 0;
            viewport.Y = 0;

            viewport.Width = clientSize.Width;
            viewport.Height = clientSize.Height;

            viewport.MinDepth = 0;
            viewport.MaxDepth = 1;

            if (!graphicsDevice.Viewport.Equals(viewport)) {
                graphicsDevice.Viewport = viewport;
            }

            WindowBackend?.BeginDraw(this);

            return DeviceResetError.None;
        }

        private void EndDraw() {
            WindowBackend?.EndDraw(this);
        }

        private DeviceResetError HandleDeviceReset() {
            bool needsReset;
            var clientSize = ClientSize;

            switch (GraphicsDevice.GraphicsDeviceStatus) {
                case GraphicsDeviceStatus.Lost:
                    return DeviceResetError.DeviceLost;
                case GraphicsDeviceStatus.NotReset:
                    needsReset = true;
                    break;
                default:
                    var pp = GraphicsDevice.PresentationParameters;
                    needsReset = clientSize.Width > pp.BackBufferWidth || clientSize.Height > pp.BackBufferHeight;
                    break;
            }

            if (needsReset) {
                try {
                    _graphicsDeviceService.ResetDevice(clientSize.Width, clientSize.Height);
                } catch (Exception ex) {
                    Debug.Print(ex.ToString());
                    return DeviceResetError.Failed;
                }
            }

            return DeviceResetError.None;
        }

        protected void DrawErrorText([NotNull] Graphics graphics, [NotNull] string text) {
            if (_errorTextDrawn) {
                return;
            }

            graphics.Clear(Color.Black);

            using (Brush brush = new SolidBrush(Color.White)) {
                using (var format = new StringFormat()) {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    graphics.DrawString(text, Font, brush, ClientRectangle, format);
                }
            }

            _errorTextDrawn = true;
        }

        private enum DeviceResetError {

            None = 0,
            DeviceLost = 1,
            Failed = 2

        }

        private GraphicsDeviceWrapper _graphicsDeviceService;
        private readonly ServiceContainer _serviceContainer;
        private readonly List<Microsoft.Xna.Framework.Input.Keys> _keyState;

        [CanBeNull]
        private Brush _backgroundBrush;

        [CanBeNull]
        private Pen _borderPen;

        private bool _errorTextDrawn;

    }
}
