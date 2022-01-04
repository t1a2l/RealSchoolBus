using System;
using UnityEngine;
using ColossalFramework.UI;

namespace RealSchoolBus.AI
{
    /// <summary>
    /// Static class to manage the ABLC building panel.
    /// </summary>
    internal static class BuildingPanelManager
    {
        // Instance references.
        private static GameObject uiGameObject;
        private static SchoolBusPanel panel;
        internal static SchoolBusPanel Panel => panel;

        /// <summary>
        /// Adds event handler to show/hide building panel as appropriate (in line with ZonedBuildingWorldInfoPanel).
        /// </summary>
        internal static void Hook()
        {
            // Get building info panel instance.
            UIComponent buildingInfoPanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name)?.component;
            if (buildingInfoPanel == null)
            {
                LogHelper.Error("couldn't hook building info panel");
            }
            else
            {
                // Toggle button and/or panel visibility when game building info panel visibility changes.
                buildingInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create / destroy our panel as and when the info panel is shown or hidden.
                    if (isVisible)
                    {
                        Create();
                    }
                    else
                    {
                        Close();
                    }
                };
            }
        }


        /// <summary>
        /// Handles a change in target building from the WorldInfoPanel.
        /// </summary>
        internal static void TargetChanged()
        {
            // Communicate target change to the panel (if it's currently instantiated).
            Panel?.BuildingChanged();
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create()
        {
            try
            {
                // If no instance already set, create one.
                if (uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    uiGameObject = new GameObject("SchoolBusPanel");
                    uiGameObject.transform.parent = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name)?.component.transform;

                    panel = uiGameObject.AddComponent<SchoolBusPanel>();

                    // Set up and show panel.
                    Panel.transform.parent = uiGameObject.transform.parent;
                    Panel.Setup();
                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("exception creating SchoolBusPanel", e);
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(panel);
            GameObject.Destroy(uiGameObject);

            panel = null;
            uiGameObject = null;
        }

    }
}