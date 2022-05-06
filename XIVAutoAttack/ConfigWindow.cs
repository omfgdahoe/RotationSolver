using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using XIVComboPlus.Attributes;
using XIVComboPlus.Combos;

namespace XIVComboPlus;

internal class ConfigWindow : Window
{
    private readonly Vector4 shadedColor = new Vector4(0.68f, 0.68f, 0.68f, 1f);

    public ConfigWindow()
        : base("�Զ���������", 0, false)
    {
        RespectCloseHotkey = true;

        SizeCondition = (ImGuiCond)4;
        Size = new Vector2(740f, 490f);
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("##tabbar"))
        {
            if (ImGui.BeginTabItem("�����趨"))
            {
                ImGui.Text("��������ڣ�������趨�Լ�ϲ�����Զ������趨��");

                ImGui.BeginChild("����", new Vector2(0f, -1f), true);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));
                int num = 1;


                //ImGui.Text(IconReplacer.CustomCombosDict.Keys.ToString());
                foreach (string key in IconReplacer.CustomCombosDict.Keys)
                {
                    var combos = IconReplacer.CustomCombosDict[key];
                    if (combos == null || combos.Length == 0) continue;

                    if (ImGui.CollapsingHeader(key))
                    {
                        foreach (var combo in combos)
                        {
                            //ImGui.Text(combo.ComboFancyName);

                            bool enable = combo.IsEnabled;
                            ImGui.PushItemWidth(200f);
                            if (ImGui.Checkbox(combo.JobName, ref enable))
                            {
                                combo.IsEnabled = enable;
                                Service.Configuration.Save();
                            }
                            ImGui.PopItemWidth();
                            string text = $"#{num}: �滻����Ϊ{combo.JobName}������GCDս�������ܡ�";
                            if(!string.IsNullOrEmpty(combo.Description))
                            {
                                text += '\n' + combo.Description;
                            }
                            ImGui.TextColored(shadedColor, text);
                            //ImGui.Spacing();
                            //if (item == CustomComboPreset.DancerDanceComboCompatibility && enable)
                            //{
                            //    int[] array2 = Service.Configuration.DancerDanceCompatActionIDs.Cast<int>().ToArray();
                            //    if (false | ImGui.InputInt("Emboite (Red) ActionID", ref array2[0], 0) | ImGui.InputInt("Entrechat (Blue) ActionID", ref array2[1], 0) | ImGui.InputInt("Jete (Green) ActionID", ref array2[2], 0) | ImGui.InputInt("Pirouette (Yellow) ActionID", ref array2[3], 0))
                            //    {
                            //        Service.Configuration.DancerDanceCompatActionIDs = array2.Cast<uint>().ToArray();
                            //        Service.IconReplacer.UpdateEnabledActionIDs();
                            //        Service.Configuration.Save();
                            //    }
                            //    ImGui.Spacing();
                            //}
                            num++;
                        }
                    }
                    else
                    {
                        num += combos.Length;
                    }
                }

                ImGui.PopStyleVar();
                ImGui.EndChild();

                ImGui.EndTabItem();

            }


            if (ImGui.BeginTabItem("�����趨"))
            {
#if DEBUG
                ImGui.Text(DNCCombo.JobGauge.CompletedSteps.ToString());
                foreach (var item in Service.ClientState.LocalPlayer.StatusList)
                {
                    if (item.SourceID == Service.ClientState.LocalPlayer.ObjectId)
                    {
                        ImGui.Text(item.GameData.Name + item.StatusId);
                    }
                }
#endif
                //ImGui.Text(Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty].ToString());
                //ImGui.Text(TargetHelper.Time.ToString());
                //if (TargetHelper.times.Count > 0)
                //    ImGui.Text(TargetHelper.times.Average().ToString());

                ImGui.Text("��������ڣ�������趨�ͷż�������Ĳ�����");

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));

                if (ImGui.BeginChild("����", new Vector2(0f, -1f), true))
                {
                    bool isAllTargetAsHostile = Service.Configuration.AllTargeAsHostile;
                    if (ImGui.Checkbox("�Ƿ��趨���п��Թ�����Ŀ���Ϊ�ж�Ŀ��", ref isAllTargetAsHostile))
                    {
                        Service.Configuration.AllTargeAsHostile = isAllTargetAsHostile;
                        Service.Configuration.Save();
                    }

                    bool isOnlyGCD = Service.Configuration.OnlyGCD;
                    if (ImGui.Checkbox("�Ƿ�ֻʹ��GCDѭ������ȥ������", ref isOnlyGCD))
                    {
                        Service.Configuration.OnlyGCD = isOnlyGCD;
                        Service.Configuration.Save();
                    }

                    bool autoBreak = Service.Configuration.AutoBreak;
                    if (ImGui.Checkbox("�Ƿ��Զ����б���", ref autoBreak))
                    {
                        Service.Configuration.AutoBreak = autoBreak;
                        Service.Configuration.Save();
                    }

                    float specialDuration = Service.Configuration.SpecialDuration;
                    if (ImGui.DragFloat("����״̬�������", ref specialDuration, 0.02f, 1, 20))
                    {
                        Service.Configuration.SpecialDuration = specialDuration;
                        Service.Configuration.Save();
                    }

                    ImGui.Separator();

                    int multiCount = Service.Configuration.HostileCount;
                    if (ImGui.DragInt("��Χ����������Ҫ������", ref multiCount, 0.02f, 2, 5))
                    {
                        Service.Configuration.HostileCount = multiCount;
                        Service.Configuration.Save();
                    }

                    int partyCount = Service.Configuration.PartyCount;
                    if (ImGui.DragInt("��Χ����������Ҫ������", ref partyCount, 0.02f, 2, 5))
                    {
                        Service.Configuration.PartyCount = partyCount;
                        Service.Configuration.Save();
                    }
                    ImGui.Separator();

                    float speed = 0.005f;
                    float healthDiff = Service.Configuration.HealthDifference;
                    if (ImGui.DragFloat("���ٵ�HP��׼�����£�������Ⱥ��", ref healthDiff, speed * 2, 0, 0.5f))
                    {
                        Service.Configuration.HealthDifference = healthDiff;
                        Service.Configuration.Save();
                    }


                    float healthAreaA = Service.Configuration.HealthAreaAbility;
                    if (ImGui.DragFloat("���ٵ�HP��������������Ⱥ��", ref healthAreaA, speed, 0, 1))
                    {
                        Service.Configuration.HealthAreaAbility = healthAreaA;
                        Service.Configuration.Save();
                    }

                    float healthAreaS = Service.Configuration.HealthAreafSpell;
                    if (ImGui.DragFloat("���ٵ�HP��������GCDȺ��", ref healthAreaS, speed, 0, 1))
                    {
                        Service.Configuration.HealthAreafSpell = healthAreaS;
                        Service.Configuration.Save();
                    }

                    ImGui.Separator();

                    float healthSingleA = Service.Configuration.HealthSingleAbility;
                    if (ImGui.DragFloat("���ٵ�HP������������������", ref healthSingleA, speed, 0, 1))
                    {
                        Service.Configuration.HealthSingleAbility = healthSingleA;
                        Service.Configuration.Save();
                    }

                    float healthSingleS = Service.Configuration.HealthSingleSpell;
                    if (ImGui.DragFloat("���ٵ�HP��������GCD����", ref healthSingleS, speed, 0, 1))
                    {
                        Service.Configuration.HealthSingleSpell = healthSingleS;
                        Service.Configuration.Save();
                    }
                    ImGui.EndChild();
                }

                ImGui.PopStyleVar();


                ImGui.EndTabItem();

            }

            if (ImGui.BeginTabItem("�����ͷ��¼�"))
            {
                ImGui.Text("��������ڣ�������趨һЩ�����ͷź�ʹ��ʲô�ꡣ");

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));

                if (ImGui.Button("����"))
                {
                    Service.Configuration.Events.Add(new ActionEvents());
                }

                if (ImGui.BeginChild("�¼�", new Vector2(0f, -1f), true))
                {
                    for (int i = 0; i < Service.Configuration.Events.Count; i++)
                    {
                        string name = Service.Configuration.Events[i].Name;
                        if (ImGui.InputText("��������" + i.ToString(), ref name, 50))
                        {
                            Service.Configuration.Events[i].Name = name;
                            Service.Configuration.Save();
                        }

                        //ImGui.SameLine();

                        int macroindex = Service.Configuration.Events[i].MacroIndex;
                        if (ImGui.DragInt("����" + i.ToString(), ref macroindex, 1, 0, 99))
                        {
                            Service.Configuration.Events[i].MacroIndex = macroindex;
                        }


                        bool isShared = Service.Configuration.Events[i].IsShared;
                        if (ImGui.Checkbox("������" + i.ToString(), ref isShared))
                        {
                            Service.Configuration.Events[i].IsShared = isShared;
                            Service.Configuration.Save();
                        }

                        ImGui.SameLine();
                        if (ImGui.Button("ɾ��" + i.ToString()))
                        {
                            Service.Configuration.Events.RemoveAt(i);
                        }
                        ImGui.Separator();
                    }
                    ImGui.EndChild();
                }
                ImGui.PopStyleVar();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("�����ĵ�"))
            {
                ImGui.Text("��������ڣ�����Կ���һ��Ѱ������ݡ�");

                if (ImGui.BeginChild("����", new Vector2(0f, -1f), true))
                {

                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));

                    ImGui.Text("/aauto HealArea ��ʾ����һ�η�Χ���ƵĴ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto HealSingle ��ʾ����һ�ε������ƵĴ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto DefenseArea ��ʾ����һ�η�Χ�����Ĵ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto DefenseSingle ��ʾ����һ�ε�������Ĵ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto Esuna ��ʾ����һ�ο����Ĵ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto Raise ��ʾ����ǿ�ƾ��˻�ͻ���Ĵ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto AntiRepulsion ��ʾ����һ�η����˵Ĵ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto Break ��ʾ����һ�α����Ĵ����ڡ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto AttackBig ��ʼ��������������ΪHitBox���ġ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto AttackSmall ��ʼ��������������ΪHitBox��С�ġ�");
                    ImGui.Separator();
                    ImGui.Text("/aauto AttackManual ��ʼ��������������Ϊ�ֶ�ѡ��");
                    ImGui.Separator();
                    ImGui.Text("/aauto AttackCancel ֹͣ�������ǵ�һ��Ҫ�����ص���");

                    ImGui.EndChild();
                }
                ImGui.PopStyleVar();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
        ImGui.End();
    }

    //private static uint GetActionsByName(string name)
    //{
    //    var enumerator = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetEnumerator();

    //    while (enumerator.MoveNext())
    //    {
    //        var action = enumerator.Current;
    //        if (action.Name == name && action.ClassJobLevel != 0 && !action.IsPvP)
    //        {
    //            return action.RowId;
    //        }
    //    }
    //    return 0;
    //}

    //private static string GetActionsByName(uint actionID)
    //{
    //    var act = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(actionID);

    //    return act == null ? "" : act.Name;
    //}
}