﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Debugging
{
    /// <summary>
    /// Visual debugger to show touches as GUI elements.
    /// </summary>
    [AddComponentMenu("TouchScript/Touch Debugger")]
    public class TouchDebugger : MonoBehaviour
    {
        #region Public properties

        /// <summary>Gets or sets the texture to use.</summary>
        public Texture2D TouchTexture
        {
            get { return texture; }
            set
            {
                texture = value;
                update();
            }
        }

        /// <summary>Gets or sets whether <see cref="TouchDebugger"/> is using DPI to scale touch cursors.</summary>
        /// <value><c>true</c> if dpi value is used; otherwise, <c>false</c>.</value>
        public bool UseDPI
        {
            get { return useDPI; }
            set
            {
                useDPI = value;
                update();
            }
        }

        /// <summary>Gets or sets the size of touch cursors in cm.</summary>
        /// <value>The size of touch cursors in cm.</value>
        public float TouchSize
        {
            get { return touchSize; }
            set
            {
                touchSize = value;
                update();
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private Texture2D texture;
        [SerializeField]
        private bool useDPI = true;
        [SerializeField]
        private float touchSize = 1f;

        private Dictionary<int, ITouch> dummies = new Dictionary<int, ITouch>(10);
        private float textureDPI, scale, dpi;
        private int width, height, halfWidth, halfHeight;

        #endregion

        #region Unity methods

        private void OnEnable()
        {
            if (TouchTexture == null)
            {
                Debug.LogError("Touch Debugger doesn't have touch texture assigned!");
                return;
            }

            update();

            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchesBegan += touchesBeganHandler;
                TouchManager.Instance.TouchesEnded += touchesEndedHandler;
                TouchManager.Instance.TouchesMoved += touchesMovedHandler;
                TouchManager.Instance.TouchesCancelled += touchesCancelledHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchesBegan -= touchesBeganHandler;
                TouchManager.Instance.TouchesEnded -= touchesEndedHandler;
                TouchManager.Instance.TouchesMoved -= touchesMovedHandler;
                TouchManager.Instance.TouchesCancelled -= touchesCancelledHandler;
            }
        }

        private void OnGUI()
        {
            if (TouchTexture == null) return;
            checkDPI();

            foreach (KeyValuePair<int, ITouch> dummy in dummies)
            {
                var x = dummy.Value.Position.x;
                var y = Screen.height - dummy.Value.Position.y;
                GUI.DrawTexture(new Rect(x - halfWidth, y - halfHeight, width, height), TouchTexture, ScaleMode.ScaleToFit);
            }
        }

        #endregion

        #region Private functions

        private void checkDPI()
        {
            if (useDPI && !Mathf.Approximately(dpi, TouchManager.Instance.DPI)) update();
        }

        private void update()
        {
            if (!useDPI)
            {
                width = 32;
                height = 32;
                scale = 1/4f;
                computeConsts();
            } else
            {
                dpi = TouchManager.Instance.DPI;
                textureDPI = texture.width * TouchManager.INCH_TO_CM / touchSize;
                scale = dpi / textureDPI;
                width = (int)(texture.width * scale);
                height = (int)(texture.height * scale);
                computeConsts();
            }
        }

        private void computeConsts()
        {
            halfWidth = width / 2;
            halfHeight = height / 2;
        }

        private void updateDummy(ITouch dummy)
        {
            dummies[dummy.Id] = dummy;
        }

        #endregion

        #region Event handlers

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                dummies.Add(touch.Id, touch);
            }
        }

        private void touchesMovedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                ITouch dummy;
                if (!dummies.TryGetValue(touch.Id, out dummy)) return;
                updateDummy(touch);
            }
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                ITouch dummy;
                if (!dummies.TryGetValue(touch.Id, out dummy)) return;
                dummies.Remove(touch.Id);
            }
        }

        private void touchesCancelledHandler(object sender, TouchEventArgs e)
        {
            touchesEndedHandler(sender, e);
        }

        #endregion
    }
}