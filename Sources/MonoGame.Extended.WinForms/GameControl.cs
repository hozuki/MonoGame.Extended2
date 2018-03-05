using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.WinForms {
    public abstract class GameControl : GraphicsDeviceControl {

        protected GameControl() {
            UpdateKeysInPaint = false;
        }

        public event EventHandler<TimedEventArgs> GameUpdate;

        public event EventHandler<TimedEventArgs> GameDraw;

        public event EventHandler<EventArgs> GameInitialize;

        public event EventHandler<EventArgs> GameLoadContents;

        public event EventHandler<EventArgs> GameUnloadContents;

        public event EventHandler<EventArgs> GameDisposed;

        [Browsable(true)]
        [DefaultValue(true)]
        [Description("Gets/sets whether the game update should be suspended when parent form is inactive.")]
        [Category("MonoGame")]
        public bool SuspendOnFormInactive { get; set; } = true;

        public bool IsActive => _isActive;

        protected override void OnInitialize() {
            _stopwatch = Stopwatch.StartNew();

            Application.Idle += Application_Idle;

            var parentForm = FindParentForm();

            if (parentForm != null) {
                parentForm.Activated += ParentForm_Activated;
                parentForm.Deactivate += ParentForm_Deactivated;
                _parentForm = parentForm;
            }

            GameInitialize?.Invoke(this, EventArgs.Empty);

            OnLoadContents();
        }

        protected virtual void OnUpdate([NotNull] GameTime gameTime) {
            if (!IsDesignMode) {
                Guard.ArgumentNotNull(gameTime, nameof(gameTime));

                GameUpdate?.Invoke(this, new TimedEventArgs(gameTime));
            }
        }

        protected virtual void OnDraw([CanBeNull] GameTime gameTime) {
            if (gameTime != null) {
                GameDraw?.Invoke(this, new TimedEventArgs(gameTime));
            }
        }

        protected sealed override void OnDraw() {
            if (!IsDesignMode) {
                OnDraw(_gameTime);
            }
        }

        protected override void Dispose(bool disposing) {
            OnUnloadContents();

            GameDisposed?.Invoke(this, EventArgs.Empty);

            Application.Idle -= Application_Idle;

            if (_parentForm != null) {
                _parentForm.Activated -= ParentForm_Activated;
                _parentForm.Deactivate -= ParentForm_Deactivated;
            }

            base.Dispose(disposing);
        }

        protected virtual void OnLoadContents() {
            GameLoadContents?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUnloadContents() {
            GameUnloadContents?.Invoke(this, EventArgs.Empty);
        }

        private void GameLoop() {
            if (SuspendOnFormInactive && !IsActive) {
                return;
            }

            UpdateKeys();

            _gameTime = new GameTime(_stopwatch.Elapsed, _stopwatch.Elapsed - _elapsed);
            _elapsed = _stopwatch.Elapsed;

            OnUpdate(_gameTime);
            Invalidate();
        }

        private void ParentForm_Activated(object sender, EventArgs e) {
            _isActive = true;
            _elapsed = _stopwatch.Elapsed;
        }

        private void ParentForm_Deactivated(object sender, EventArgs e) {
            _isActive = false;
        }

        private void Application_Idle(object sender, EventArgs e) {
            GameLoop();
        }

        // TODO: This method always returns null when the control is used in WPF interop.
        [CanBeNull]
        private Form FindParentForm() {
            var parent = Parent;

            while (parent != null) {
                if (parent is Form) {
                    return parent as Form;
                }

                parent = parent.Parent;
            }

            return null;
        }

        private Stopwatch _stopwatch;
        private GameTime _gameTime;
        private TimeSpan _elapsed;

        private bool _isActive;
        [CanBeNull]
        private Form _parentForm;

    }
}
