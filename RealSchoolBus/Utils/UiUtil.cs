using ColossalFramework.UI;
using UnityEngine;

namespace RealSchoolBus.Utils {
   public static class UiUtil
    {
        private const int ButtonSize = 16;

        public static UISprite CreateAllowSprite(UIComponent parentComponent, MouseEventHandler handler, Vector3 offset)
        {
            return CreateSprite("AllowPrisonHelicoptersButton", null, offset,
                parentComponent, handler);
        }

        public static UISprite CreateSprite(string buttonName, string tooltip, Vector3 offset,
            UIComponent parentComponent, MouseEventHandler handler)
        {

            var sprite = UIView.GetAView().AddUIComponent(typeof(UISprite)) as UISprite;
            if (sprite == null)
            {
                return null;
            }
            sprite.canFocus = false;
            sprite.name = buttonName;
            sprite.width = ButtonSize;
            sprite.height = ButtonSize;
            sprite.tooltip = tooltip;
            sprite.eventClick += handler;
            sprite.AlignTo(parentComponent, UIAlignAnchor.TopRight);
            sprite.relativePosition = offset;
            return sprite;
        }

        public static UILabel CreateLabel(string text, UIComponent parentComponent, Vector3 offset)
        {
            var label = UIView.GetAView().AddUIComponent(typeof(UILabel)) as UILabel;
            if (label == null)
            {
                return null;
            }
            label.text = text;
            label.AlignTo(parentComponent, UIAlignAnchor.TopRight);
            label.relativePosition = offset;
            return label;
        }

        /// <summary>
        /// Adds a simple pushbutton.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="text">Button text</param>
        /// <param name="width">Button width (default 200)</param>
        /// <param name="height">Button height (default 30)</param>
        /// <param name="scale">Text scale (default 0.9)</param>
        /// <param name="vertPad">Vertical text padding within button (default 4)</param>
        /// <param name="tooltip">Tooltip, if any</param>
        /// <returns>New pushbutton</returns>
        public static UIButton AddButton(UIComponent parent, float posX, float posY, string text, float width = 200f, float height = 30f, float scale = 0.9f, int vertPad = 4, string tooltip = null)
        {
            UIButton button = parent.AddUIComponent<UIButton>();

            // Size and position.
            button.size = new Vector2(width, height);
            button.relativePosition = new Vector2(posX, posY);

            // Appearance.
            button.textScale = scale;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(128, 128, 128, 255);
            button.canFocus = false;

            // Add tooltip.
            if (tooltip != null)
            {
                button.tooltip = tooltip;
            }

            // Text.
            button.textScale = scale;
            button.textPadding = new RectOffset(0, 0, vertPad, 0);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.text = text;

            return button;
        }
    }
}
