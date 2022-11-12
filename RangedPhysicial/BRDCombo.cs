using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Actions.BaseAction;
using XIVAutoAttack.Combos.Attributes;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using static XIVAutoAttack.Combos.RangedPhysicial.BRDCombo;

namespace XIVAutoAttack.Combos.RangedPhysicial;

[ComboDevInfo(@"https://github.com/ArchiDog1998/XIVAutoAttack/blob/main/XIVAutoAttack/Combos/RangedPhysicial/BRDCombo.cs")]
internal sealed class BRDCombo : JobGaugeCombo<BRDGauge, CommandType>
{
    internal enum CommandType : byte
    {
        None,
    }

    protected override SortedList<CommandType, string> CommandDescription => new SortedList<CommandType, string>()
    {
        //{CommandType.None, "" }, //д��ע�Ͱ���������ʾ�û��ġ�
    };

    public override uint[] JobIDs => new uint[] { 23, 5 };

    public static readonly BaseAction
        //ǿ�����
        HeavyShoot = new(97) { BuffsProvide = new[] { StatusIDs.StraightShotReady } },

        //ֱ�����
        StraitShoot = new(98) { BuffsNeed = new[] { StatusIDs.StraightShotReady } },

        //��ҩ��
        VenomousBite = new(100, isDot: true) { TargetStatus = new[] { StatusIDs.VenomousBite, StatusIDs.CausticBite } },

        //��ʴ��
        Windbite = new(113, isDot: true) { TargetStatus = new[] { StatusIDs.Windbite, StatusIDs.Stormbite } },

        //��������
        IronJaws = new(3560, isDot: true)
        {
            OtherCheck = b =>
            {
                if (IsLastWeaponSkill(false, IronJaws)) return false;

                if (Player.HaveStatus(StatusIDs.RagingStrikes) &&
                    Player.WillStatusEndGCD(1, 1, true, StatusIDs.RagingStrikes)) return true;

                return b.HaveStatus(StatusIDs.VenomousBite, StatusIDs.CausticBite) & b.HaveStatus(StatusIDs.Windbite, StatusIDs.Stormbite)
                & (b.WillStatusEndGCD((uint)Service.Configuration.AddDotGCDCount, 0, true, StatusIDs.VenomousBite, StatusIDs.CausticBite)
                | b.WillStatusEndGCD((uint)Service.Configuration.AddDotGCDCount, 0, true, StatusIDs.Windbite, StatusIDs.Stormbite));
            },
        },

        //���ߵ�����ҥ
        MagesBallad = new(114),

        //�����������
        ArmysPaeon = new(116),

        //�������С������
        WanderersMinuet = new(3559),

        //ս��֮��
        BattleVoice = new(118, true),

        //����ǿ��
        RagingStrikes = new(101)
        {
            OtherCheck = b =>
            {
                if (JobGauge.Song == Song.WANDERER || !WanderersMinuet.EnoughLevel && BattleVoice.WillHaveOneChargeGCD(1, 1)
                    || !BattleVoice.EnoughLevel) return true;

                return false;
            },
        },

        //���������������
        RadiantFinale = new(25785, true)
        {
            OtherCheck = b =>
            {
                static bool SongIsNotNone(Song value) => value != Song.NONE;
                static bool SongIsWandererMinuet(Song value) => value == Song.WANDERER;
                if ((Array.TrueForAll(JobGauge.Coda, SongIsNotNone) || Array.Exists(JobGauge.Coda, SongIsWandererMinuet))
                    && BattleVoice.WillHaveOneChargeGCD()
                    && RagingStrikes.IsCoolDown
                    && Player.HaveStatus(StatusIDs.RagingStrikes)
                    && RagingStrikes.ElapsedAfterGCD(1)) return true;
                return false;
            },
        },

        //���Ҽ�
        Barrage = new(107)
        {
            BuffsProvide = new[] { StatusIDs.StraightShotReady },
            OtherCheck = b =>
            {
                if (!EmpyrealArrow.IsCoolDown || EmpyrealArrow.WillHaveOneChargeGCD() || JobGauge.Repertoire == 3) return false;
                return true;
            }
        },

        //��������
        EmpyrealArrow = new(3558),

        //��������
        PitchPerfect = new(7404)
        {
            OtherCheck = b => JobGauge.Song == Song.WANDERER,
        },

        //ʧѪ��
        Bloodletter = new(110)
        {
            OtherCheck = b =>
            {
                if (EmpyrealArrow.EnoughLevel && (!EmpyrealArrow.IsCoolDown || EmpyrealArrow.WillHaveOneChargeGCD())) return false;
                return true;
            }
        },

        //��������
        RainofDeath = new(117),

        //�����
        QuickNock = new(106) { BuffsProvide = new[] { StatusIDs.ShadowbiteReady } },

        //Ӱ�ɼ�
        Shadowbite = new(16494) { BuffsNeed = new[] { StatusIDs.ShadowbiteReady } },

        //����������޿���
        WardensPaean = new(3561),

        //��������������
        NaturesMinne = new(7408),

        //����յ���
        Sidewinder = new(3562),

        //�����
        ApexArrow = new(16496)
        {
            OtherCheck = b =>
            {
                if (Player.HaveStatus(StatusIDs.BlastArrowReady) || (QuickNock.ShouldUse(out _) && JobGauge.SoulVoice == 100)) return true;

                //�챬����,���ŵȱ���
                if (JobGauge.SoulVoice == 100 && BattleVoice.WillHaveOneCharge(25)) return false;

                //���������,������ﻹ�о����,�ͰѾ�������ȥ
                if (JobGauge.SoulVoice >= 80 && Player.HaveStatus(StatusIDs.RagingStrikes) && Player.WillStatusEnd(10, false, StatusIDs.RagingStrikes)) return true;

                if (JobGauge.SoulVoice == 100
                    && Player.HaveStatus(StatusIDs.RagingStrikes)
                    && Player.HaveStatus(StatusIDs.BattleVoice)
                    && (Player.HaveStatus(StatusIDs.RadiantFinale) || !RadiantFinale.EnoughLevel)) return true;

                if (JobGauge.Song == Song.MAGE && JobGauge.SoulVoice >= 80 && JobGauge.SongTimer < 22 && JobGauge.SongTimer > 18) return true;

                //����֮������100�����ڱ�����Ԥ��״̬
                if (!Player.HaveStatus(StatusIDs.RagingStrikes) && JobGauge.SoulVoice == 100) return true;

                return false;
            },
        },

        //����
        Troubadour = new(7405, true)
        {
            BuffsProvide = new[]
            {
                    StatusIDs.Troubadour,
                    StatusIDs.Tactician1,
                    StatusIDs.Tactician2,
                    StatusIDs.ShieldSamba,
            },
        };
    public override SortedList<DescType, string> DescriptionDict => new()
    {
        {DescType.��Χ����, $"{Troubadour}"},
        {DescType.��������, $"{NaturesMinne}"},
    };
    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //����
        if (Troubadour.ShouldUse(out act)) return true;


        return false;
    }

    private protected override bool HealSingleAbility(byte abilityRemain, out IAction act)
    {
        //��������������
        if (NaturesMinne.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool GeneralGCD(out IAction act)
    {
        //��������
        if (IronJaws.ShouldUse(out act)) return true;

        //�Ŵ��У�
        if (ApexArrow.ShouldUse(out act, mustUse: true)) return true;

        //Ⱥ��GCD
        if (Shadowbite.ShouldUse(out act)) return true;
        if (QuickNock.ShouldUse(out act)) return true;

        //ֱ�����
        if (StraitShoot.ShouldUse(out act)) return true;

        //�϶�
        if (VenomousBite.ShouldUse(out act)) return true;
        if (Windbite.ShouldUse(out act)) return true;

        //ǿ�����
        if (HeavyShoot.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool EmergercyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        //���������Ҫ�϶�����Ҫֱ������������ˡ�
        if (nextGCD.IsAnySameAction(true, StraitShoot, VenomousBite,
            Windbite, IronJaws))
        {
            return base.EmergercyAbility(abilityRemain, nextGCD, out act);
        }
        else if (abilityRemain != 0 &&
            (!RagingStrikes.EnoughLevel || Player.HaveStatus(StatusIDs.RagingStrikes)) &&
            (!BattleVoice.EnoughLevel || Player.HaveStatus(StatusIDs.BattleVoice)))
        {
            //���Ҽ�
            if (Barrage.ShouldUse(out act)) return true;
        }

        return base.EmergercyAbility(abilityRemain, nextGCD, out act);
    }

    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak && JobGauge.Song != Song.NONE && MagesBallad.EnoughLevel)
        {

            //����ǿ��
            if (RagingStrikes.ShouldUse(out act)) return true;

            //���������������
            if (abilityRemain == 2 && RadiantFinale.ShouldUse(out act, mustUse: true)) return true;

            //ս��֮��
            if (BattleVoice.ShouldUse(out act, mustUse: true))
            {
                if (RadiantFinale.IsCoolDown && RadiantFinale.EnoughLevel) return true;
                if (RagingStrikes.IsCoolDown && RagingStrikes.ElapsedAfterGCD(1) && !RadiantFinale.EnoughLevel) return true;
            }
        }

        if (RadiantFinale.IsCoolDown && !RadiantFinale.ElapsedAfterGCD())
        {
            act = null;
            return false;
        }
        //�������С������
        if ((JobGauge.Song == Song.NONE || ((JobGauge.Song != Song.NONE || Player.HaveStatus(StatusIDs.ArmyEthos)) && abilityRemain == 1))
            && JobGauge.SongTimer < 3000)
        {
            if (WanderersMinuet.ShouldUse(out act)) return true;
        }

        //��������
        if (JobGauge.Song != Song.NONE && EmpyrealArrow.ShouldUse(out act)) return true;

        //��������
        if (PitchPerfect.ShouldUse(out act))
        {
            if (JobGauge.SongTimer < 3000 && JobGauge.Repertoire > 0) return true;

            if (JobGauge.Repertoire == 3 || JobGauge.Repertoire == 2 && EmpyrealArrow.WillHaveOneChargeGCD(1)) return true;
        }

        //���ߵ�����ҥ
        if (JobGauge.SongTimer < 3000 && MagesBallad.ShouldUse(out act)) return true;

        //�����������
        if (JobGauge.SongTimer < 12000 && (JobGauge.Song == Song.MAGE
            || JobGauge.Song == Song.NONE) && ArmysPaeon.ShouldUse(out act)) return true;

        //����յ���
        if (Sidewinder.ShouldUse(out act))
        {
            if (Player.HaveStatus(StatusIDs.BattleVoice) && (Player.HaveStatus(StatusIDs.RadiantFinale) || !RadiantFinale.EnoughLevel)) return true;

            if (!BattleVoice.WillHaveOneCharge(10, false) && !RadiantFinale.WillHaveOneCharge(10, false)) return true;

            if (RagingStrikes.IsCoolDown && !Player.HaveStatus(StatusIDs.RagingStrikes)) return true;
        }

        //����������û�п�����ǿ����ս��֮��
        bool empty = (Player.HaveStatus(StatusIDs.RagingStrikes)
            && (Player.HaveStatus(StatusIDs.BattleVoice)
            || !BattleVoice.EnoughLevel)) || JobGauge.Song == Song.MAGE;
        //��������
        if (RainofDeath.ShouldUse(out act, emptyOrSkipCombo: empty)) return true;

        //ʧѪ��
        if (Bloodletter.ShouldUse(out act, emptyOrSkipCombo: empty)) return true;

        return false;
    }
}