using System;
using System.Collections.Generic;
using TMPro;


internal class SafeTextMeshProUGUI : TextMeshProUGUI
{
    protected override void GenerateTextMesh()
    {
        try {base.GenerateTextMesh();}
        catch(NullReferenceException) { }
    }
}
