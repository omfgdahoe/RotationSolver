﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;
using RotationSolver.Commands;

namespace RotationSolver.Updaters;

internal static class InputUpdater
{
    static readonly SortedList<VirtualKey, bool> _keys = new();
    static readonly SortedList<GamepadButtons, bool> _buttons = new();

    public static SpecialCommandType RecordingSpecialType { get ; set; }
    public static StateCommandType RecordingStateType { get ; set; }
    public static bool RecordingDoAction { get; set; }
    public static DateTime RecordingTime { get; set; } = DateTime.MinValue;

    internal static unsafe void UpdateCommand()
    {
        if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
            || Svc.Condition[ConditionFlag.Occupied33]
            || Svc.Condition[ConditionFlag.Occupied38]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.Mounted]
            || Svc.Condition[ConditionFlag.SufferingStatusAffliction2]
            || Svc.Condition[ConditionFlag.RolePlaying]
            || Svc.Condition[ConditionFlag.UsingParasol]
            || Svc.Condition[ConditionFlag.InFlight]) return;

        if (DateTime.Now - RecordingTime > TimeSpan.FromSeconds(10))
        {
            RecordingSpecialType = SpecialCommandType.None;
            RecordingStateType = StateCommandType.None;
            RecordingDoAction = false;
        }

        
        foreach (var key in Svc.KeyState.GetValidVirtualKeys())
        {
            if (key is VirtualKey.CONTROL) continue;
            if (key is VirtualKey.MENU) continue;
            if (key is VirtualKey.SHIFT) continue;

            var value = Svc.KeyState[key];

            if (_keys.ContainsKey(key))
            {
                if (!_keys[key] && value)
                {
                    KeyDown(new KeyRecord(key, Svc.KeyState[VirtualKey.CONTROL], 
                        Svc.KeyState[VirtualKey.MENU], Svc.KeyState[VirtualKey.SHIFT]));
                }
            }
            _keys[key] = value;
        }

        foreach (var button in Enum.GetValues<GamepadButtons>())
        {
            if (button is GamepadButtons.L2) continue;
            if (button is GamepadButtons.R2) continue;

            
            var value = Svc.GamepadState.Raw(button) > 0.5f;
            if (_buttons.ContainsKey(button))
            {
                if (!_buttons[button] && value)
                {
                    ButtonDown(new ButtonRecord(button,
                        Svc.GamepadState.Raw(GamepadButtons.L2) > 0.5f, 
                        Svc.GamepadState.Raw(GamepadButtons.R2) > 0.5f));
                }
            }
            _buttons[button] = value;
        }
    }

    static readonly Dalamud.Game.Gui.Toast.QuestToastOptions QUEST = new()
    {
        IconId = 101,
        PlaySound = true,
        DisplayCheckmark = true,
    };

    private static void KeyDown(KeyRecord key)
    {
        if (RecordingSpecialType != SpecialCommandType.None)
        {
            OtherConfiguration.InputConfig.KeySpecial[RecordingSpecialType] = key;
            Svc.Toasts.ShowQuest($"{RecordingSpecialType}: {key.ToStr()}",
               QUEST);

            RecordingSpecialType = SpecialCommandType.None;
            OtherConfiguration.SaveInputConfig();
            return;
        }
        else if (RecordingStateType != StateCommandType.None )
        {
            OtherConfiguration.InputConfig.KeyState[RecordingStateType] = key;
            Svc.Toasts.ShowQuest($"{RecordingStateType}: {key.ToStr()}",
                QUEST);

            RecordingStateType = StateCommandType.None;
            OtherConfiguration.SaveInputConfig();
            return;
        }
        else if (RecordingDoAction)
        {
            OtherConfiguration.InputConfig.KeyDoAction = key;
            Svc.Toasts.ShowQuest($"Do Action: {key.ToStr()}", QUEST);
            RecordingDoAction = false;
            OtherConfiguration.SaveInputConfig();
            return;
        }

        if (!Service.Config.UseKeyboardCommand) return;

        if (OtherConfiguration.InputConfig.KeyState.ContainsValue(key))
        {
            Svc.Commands.ProcessCommand(OtherConfiguration.InputConfig.KeyState
                .FirstOrDefault(k => k.Value == key && k.Key != StateCommandType.None).Key.GetCommandStr());
        }
        else if (OtherConfiguration.InputConfig.KeySpecial.ContainsValue(key))
        {
            Svc.Commands.ProcessCommand(OtherConfiguration.InputConfig.KeySpecial
                .FirstOrDefault(k => k.Value == key && k.Key != SpecialCommandType.None).Key.GetCommandStr());
        }
        else if(OtherConfiguration.InputConfig.KeyDoAction == key)
        {
            RSCommands.DoAction();
        }
    }

    private static void ButtonDown(ButtonRecord button)
    {
        if (RecordingSpecialType != SpecialCommandType.None)
        {
            OtherConfiguration.InputConfig.ButtonSpecial[RecordingSpecialType] = button;
            Svc.Toasts.ShowQuest($"{RecordingSpecialType}: {button.ToStr()}",
                QUEST);

            RecordingSpecialType = SpecialCommandType.None;
            OtherConfiguration.SaveInputConfig();
            return;
        }
        else if (RecordingStateType != StateCommandType.None)
        {
            OtherConfiguration.InputConfig.ButtonState[RecordingStateType] = button;
            Svc.Toasts.ShowQuest($"{RecordingStateType}: {button.ToStr()}",
                QUEST);

            RecordingStateType = StateCommandType.None;
            OtherConfiguration.SaveInputConfig();
            return;
        }
        else if (RecordingDoAction)
        {
            OtherConfiguration.InputConfig.ButtonDoAction = button;
            Svc.Toasts.ShowQuest($"Do Action: {button.ToStr()}", QUEST);
            RecordingDoAction = false;
            OtherConfiguration.SaveInputConfig();
            return;
        }

        if (!Service.Config.UseGamepadCommand) return;

        if (OtherConfiguration.InputConfig.ButtonState.ContainsValue(button))
        {
            Svc.Commands.ProcessCommand(OtherConfiguration.InputConfig.ButtonState
                .FirstOrDefault(k => k.Value == button && k.Key != StateCommandType.None).Key.GetCommandStr());
        }
        else if (OtherConfiguration.InputConfig.ButtonSpecial.ContainsValue(button))
        {
            Svc.Commands.ProcessCommand(OtherConfiguration.InputConfig.ButtonSpecial
                .FirstOrDefault(k => k.Value == button && k.Key != SpecialCommandType.None).Key.GetCommandStr());
        }
        else if (OtherConfiguration.InputConfig.ButtonDoAction == button)
        {
            RSCommands.DoAction();
        }
    }
}
