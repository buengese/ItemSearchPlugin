﻿using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
	public static class ExcelExtensions {

		public static bool HasClass(this ClassJobCategory cjc, int classJobRowId) {
			return classJobRowId switch
			{
				0 => cjc.ADV,
				1 => cjc.GLA,
				2 => cjc.PGL,
				3 => cjc.MRD,
				4 => cjc.LNC,
				5 => cjc.ARC,
				6 => cjc.CNJ,
				7 => cjc.THM,
				8 => cjc.CRP,
				9 => cjc.BSM,
				10 => cjc.ARM,
				11 => cjc.GSM,
				12 => cjc.LTW,
				13 => cjc.WVR,
				14 => cjc.ALC,
				15 => cjc.CUL,
				16 => cjc.MIN,
				17 => cjc.BTN,
				18 => cjc.FSH,
				19 => cjc.PLD,
				20 => cjc.MNK,
				21 => cjc.WAR,
				22 => cjc.DRG,
				23 => cjc.BRD,
				24 => cjc.WHM,
				25 => cjc.BLM,
				26 => cjc.ACN,
				27 => cjc.SMN,
				28 => cjc.SCH,
				29 => cjc.ROG,
				30 => cjc.NIN,
				31 => cjc.MCH,
				32 => cjc.DRK,
				33 => cjc.AST,
				34 => cjc.SAM,
				35 => cjc.RDM,
				36 => cjc.BLU,
				37 => cjc.GNB,
				38 => cjc.DNC,
				_ => false,
			};
		}

		public enum CharacterSex
		{
			Male = 0,
			Female = 1,
			Either = 2,
			Both = 3
		};

		public static bool AllowsRaceSex(this EquipRaceCategory erc, int raceId, CharacterSex sex)
		{
			switch (sex)
			{
				case CharacterSex.Both when (erc.Male == false || erc.Female == false):
				case CharacterSex.Either when (erc.Male == false && erc.Female == false):
				case CharacterSex.Female when erc.Female == false:
				case CharacterSex.Male when erc.Male == false:
					return false;
			}

			return raceId switch
			{
				0 => false,
				1 => erc.Hyur,
				2 => erc.Elezen,
				3 => erc.Lalafell,
				4 => erc.Miqote,
				5 => erc.Roegadyn,
				6 => erc.AuRa,
				7 => erc.unknown6, // Hrothgar
				8 => erc.unknown7, // Viera
				_ => false
			};
		}

	}
}
