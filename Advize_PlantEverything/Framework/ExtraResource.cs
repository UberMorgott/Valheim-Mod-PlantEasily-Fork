namespace Advize_PlantEverything;

internal sealed class ExtraResource
{
    public string prefabName;
    public string resourceName;
    public int resourceCost;
    public bool groundOnly;
    public string pieceName;
    public string pieceDescription;

    internal bool IsValid() => !(prefabName == default || prefabName.StartsWith("PE_Fake") || resourceName == default || resourceCost == default);
}
