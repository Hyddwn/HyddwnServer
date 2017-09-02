// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Scripting.Compilers
{
    public interface IPreCompiler
    {
        string PreCompile(string script);
    }
}