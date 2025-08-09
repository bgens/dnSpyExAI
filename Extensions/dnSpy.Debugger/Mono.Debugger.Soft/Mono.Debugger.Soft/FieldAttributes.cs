using System;

namespace Mono.Debugger.Soft {
	[Flags]
	public enum FieldAttributes {
		FieldAccessMask = 7,
		PrivateScope = 0,
		Private = 1,
		FamANDAssem = 2,
		Assembly = FamANDAssem | Private,
		Family = 4,
		FamORAssem = Family | Private,
		Public = Family | FamANDAssem,
		Static = 0x10,
		InitOnly = 0x20,
		Literal = 0x40,
		NotSerialized = 0x80,
		SpecialName = 0x200,
		PinvokeImpl = 0x2000,
		RTSpecialName = 0x400,
		HasFieldMarshal = 0x1000,
		HasDefault = 0x8000,
		HasFieldRVA = 0x100,
	}
}
