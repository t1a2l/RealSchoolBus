using System;
using UnityEngine;
using ColossalFramework.UI;
using RealSchoolBus.Utils;

namespace RealSchoolBus.Panels
{
    /// <summary>
    /// ABLC info panel base class.
    /// </summary>
    internal class BasePanel : UIPanel
    {
        // Layout constants.
        protected const float Margin = 5f;
        protected const float MenuWidth = 60f;
        protected virtual float PanelHeight => 350f;

        // Reference variables.
        protected ushort targetID;

        // Event toggler.
        protected bool disableEvents = false;

        // Panel components.
        protected UIButton m_DrawRouteButton;
		protected UIButton m_DeleteRouteButton;

        /// <summary>
        /// Performs initial setup for the panel; we don't use Start() as that's not sufficiently reliable (race conditions), and is not needed with the dynamic create/destroy process.
        /// </summary>
        internal virtual void Setup()
        {
            try
            {
                // Basic setup.
                name = "SchoolBusRoutes";
                AlignTo(parent, UIAlignAnchor.TopRight);
                autoLayout = false;
                backgroundSprite = "MenuPanel2";
                opacity = 0.95f;
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                size = new Vector2(220f, PanelHeight);

                 m_DrawRouteButton = UiUtil.AddButton(this, Margin, PanelHeight - 80f, "Draw Route", this.width - (Margin * 2), tooltip: "Draw a school bus route");

                // Delete Route button.
                m_DeleteRouteButton = UiUtil.AddButton(this, Margin, PanelHeight - 40f, "Delete Route", this.width - (Margin * 2), tooltip: "Delete a school bus route");
            }
            catch (Exception e)
            {
                LogHelper.Error("exception setting up panel base", e);
            }
        }

    }
}