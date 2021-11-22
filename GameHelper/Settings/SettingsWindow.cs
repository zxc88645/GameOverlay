﻿// <copyright file="SettingsWindow.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using ClickableTransparentOverlay;
    using Coroutine;
    using CoroutineEvents;
    using ImGuiNET;
    using Plugin;
    using Utils;

    /// <summary>
    ///     Creates the MainMenu on the UI.
    /// </summary>
    internal static class SettingsWindow
    {
        private static Vector4 color = new(1f, 1f, 0f, 1f);
        private static bool isOverlayRunningLocal = true;
        private static bool isSettingsWindowVisible = true;

        /// <summary>
        ///     Initializes the Main Menu.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            HideOnStartCheck();
            CoroutineHandler.Start(SaveCoroutine());
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                RenderCoroutine(),
                "[Settings] Draw Core/Plugin settings",
                int.MaxValue));
        }

        private static void DrawManuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Enable Plugins"))
                {
                    foreach (var pKeyValue in PManager.AllPlugins.ToList())
                    {
                        var pluginContainer = pKeyValue.Value;
                        var isEnabled = pluginContainer.Enable;
                        if (ImGui.Checkbox($"{pKeyValue.Key}", ref isEnabled))
                        {
                            pluginContainer.Enable = !pluginContainer.Enable;
                            if (pluginContainer.Enable)
                            {
                                pluginContainer.Plugin.OnEnable(Core.Process.Address != IntPtr.Zero);
                            }
                            else
                            {
                                pluginContainer.Plugin.OnDisable();
                            }

                            PManager.AllPlugins[pKeyValue.Key] = pluginContainer;
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private static void DrawTabs()
        {
            if (ImGui.BeginTabBar("pluginsTabBar", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable))
            {
                if (ImGui.BeginTabItem("Core"))
                {
                    DrawCoreSettings();
                    ImGui.EndTabItem();
                }

                foreach (var pKeyValue in PManager.AllPlugins.ToList())
                {
                    if (pKeyValue.Value.Enable && ImGui.BeginTabItem(pKeyValue.Key))
                    {
                        pKeyValue.Value.Plugin.DrawSettings();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }

        /// <summary>
        ///     Draws the currently selected settings on ImGui.
        /// </summary>
        private static void DrawCoreSettings()
        {
            ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
            ImGui.TextColored(color, "This is a free software, only use https://ownedcore.com to " +
                "download it. Do not buy from the fake sellers or websites.");
            ImGui.TextColored(color, "请不要花钱购买本软件，否则你就是个傻逼。这是一个免费软件。" +
                "不要从假卖家那里购买。前往 https://ownedcore.com 免费下载。");
            ImGui.NewLine();
            ImGui.TextColored(Vector4.One, "Developer of this software is not responsible for " +
                              "any loss that may happen due to the usage of this software. Use this " +
                              "software at your own risk.");
            ImGui.NewLine();
            ImGui.TextColored(Vector4.One, "All Settings (including plugins) are saved automatically " +
                  $"when you close the overlay or hide it via {Core.GHSettings.MainMenuHotKey} button.");
            ImGui.NewLine();
            ImGui.PopTextWrapPos();
            ImGui.DragInt("Key Timeout", ref Core.GHSettings.KeyPressTimeout, 0.2f, 30, 300);
            ImGuiHelper.ToolTip("When GameOverlay press a key in the game, the key " +
                "has to go to the GGG server for it to work. This process takes " +
                "time equal to your latency x 2. During this time GameOverlay might " +
                "press that key again. Set the key timeout value to latency x 2 so " +
                "this doesn't happen. e.g. for 30ms latency, set it to 60ms.");
            ImGui.NewLine();
            ImGui.Text($"Current Game State: {Core.States.GameCurrentState}");
            ImGui.NewLine();
            ImGuiHelper.NonContinuousEnumComboBox("Settings Window Key", ref Core.GHSettings.MainMenuHotKey);
            ImGui.NewLine();
            ImGui.Checkbox("Performance Stats", ref Core.GHSettings.ShowPerfStats);
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Checkbox("Hide when game is in background", ref Core.GHSettings.HidePerfStatsWhenBg);
            ImGui.Checkbox("Game UiExplorer (GE)", ref Core.GHSettings.ShowGameUiExplorer);
            ImGui.Checkbox("Data Visualization (DV)", ref Core.GHSettings.ShowDataVisualization);
            ImGui.Checkbox("Disable entity processing when in town or hideout",
                ref Core.GHSettings.DisableEntityProcessingInTownOrHideout);
            ImGui.Checkbox("Hide overlay settings upon start", ref Core.GHSettings.HideSettingWindowOnStart);
            ImGui.NewLine();
            ChangeFontWidget();
            ImGui.NewLine();
            if (ImGui.Button("Test Disconnect POE"))
            {
                MiscHelper.KillTCPConnectionForProcess(Core.Process.Pid);
            }
#if DEBUG
            if (ImGui.Button("Reload Plugins"))
            {
                PManager.CleanUpAllPlugins();
                PManager.InitializePlugins();
            }
#endif
        }

        /// <summary>
        ///     Draws the ImGui widget for changing fonts.
        /// </summary>
        private static void ChangeFontWidget()
        {
            if (ImGui.TreeNode("Change Font"))
            {
                ImGui.InputText("Pathname", ref Core.GHSettings.FontPathName, 300);
                ImGui.DragInt("Size", ref Core.GHSettings.FontSize, 0.1f, 13, 40);
                var languageChanged = ImGuiHelper.EnumComboBox("Language", ref Core.GHSettings.FontLanguage);
                var customLanguage = ImGui.InputText("Custom Glyph Ranges", ref Core.GHSettings.FontCustomGlyphRange, 100);
                ImGuiHelper.ToolTip("This is advance level feature. Do not modify this if you don't know what you are doing. " +
                    "Example usage:- If you have downloaded and pointed to the ArialUnicodeMS.ttf font, you can use " +
                    "0x0020, 0xFFFF, 0x00 text in this field to load all of the font texture in ImGui. Note the 0x00" +
                    " as the last item in the range.");
                if (languageChanged)
                {
                    Core.GHSettings.FontCustomGlyphRange = string.Empty;
                }

                if (customLanguage)
                {
                    Core.GHSettings.FontLanguage = FontGlyphRangeType.English;
                }

                if (ImGui.Button("Change Font"))
                {
                    if (MiscHelper.TryConvertStringToImGuiGlyphRanges(Core.GHSettings.FontCustomGlyphRange, out var glyphranges))
                    {
                        Core.Overlay.ReplaceFont(
                            Core.GHSettings.FontPathName,
                            Core.GHSettings.FontSize,
                            glyphranges);
                    }
                    else
                    {
                        Core.Overlay.ReplaceFont(
                            Core.GHSettings.FontPathName,
                            Core.GHSettings.FontSize,
                            Core.GHSettings.FontLanguage);
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Clear Custom Font"))
                {
                    Core.GHSettings.FontPathName = string.Empty;
                    Core.GHSettings.FontSize = 13;
                    Core.GHSettings.FontLanguage = FontGlyphRangeType.English;
                    Core.GHSettings.FontCustomGlyphRange = string.Empty;
                }

                ImGui.TreePop();
            }
        }

        /// <summary>
        ///     Draws the closing confirmation popup on ImGui.
        /// </summary>
        private static void DrawConfirmationPopup()
        {
            ImGui.SetNextWindowPos(new Vector2(Core.Overlay.Size.X / 3f, Core.Overlay.Size.Y / 3f));
            if (ImGui.BeginPopup("GameHelperCloseConfirmation"))
            {
                ImGui.Text("Do you want to quit the GameHelper overlay?");
                ImGui.Separator();
                if (ImGui.Button("Yes", new Vector2(ImGui.GetContentRegionAvail().X / 2f, ImGui.GetTextLineHeight() * 2)))
                {
                    Core.GHSettings.IsOverlayRunning = false;
                    ImGui.CloseCurrentPopup();
                    isOverlayRunningLocal = true;
                }

                ImGui.SameLine();
                if (ImGui.Button("No", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight() * 2)))
                {
                    ImGui.CloseCurrentPopup();
                    isOverlayRunningLocal = true;
                }

                ImGui.EndPopup();
            }
        }

        /// <summary>
        ///     Draws the Settings Window.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> RenderCoroutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (NativeMethods.IsKeyPressedAndNotTimeout((int)Core.GHSettings.MainMenuHotKey))
                {
                    isSettingsWindowVisible = !isSettingsWindowVisible;
                    if (!isSettingsWindowVisible)
                    {
                        CoroutineHandler.RaiseEvent(GameHelperEvents.TimeToSaveAllSettings);
                    }
                }

                if (!isSettingsWindowVisible)
                {
                    continue;
                }

                ImGui.SetNextWindowSizeConstraints(new Vector2(800, 600), Vector2.One * float.MaxValue);
                var isMainMenuExpanded = ImGui.Begin(
                    $"Game Overlay Settings [ {Core.GetVersion()} ]",
                    ref isOverlayRunningLocal,
                    ImGuiWindowFlags.MenuBar);

                if (!isOverlayRunningLocal)
                {
                    ImGui.OpenPopup("GameHelperCloseConfirmation");
                }

                DrawConfirmationPopup();
                if (!Core.GHSettings.IsOverlayRunning)
                {
                    CoroutineHandler.RaiseEvent(GameHelperEvents.TimeToSaveAllSettings);
                }

                if (!isMainMenuExpanded)
                {
                    ImGui.End();
                    continue;
                }

                DrawManuBar();
                DrawTabs();
                ImGui.End();
            }
        }

        /// <summary>
        ///     Saves the GameHelper settings to disk.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> SaveCoroutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
                JsonHelper.SafeToFile(Core.GHSettings, State.CoreSettingFile);
            }
        }

        private static void HideOnStartCheck()
        {
            if (Core.GHSettings.HideSettingWindowOnStart)
            {
                isSettingsWindowVisible = false;
            }
        }
    }
}