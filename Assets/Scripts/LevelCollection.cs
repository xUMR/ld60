using UnityEngine;

[CreateAssetMenu(fileName = "LevelCollection", menuName = "SO/LevelCollection")]
public sealed class LevelCollection : ScriptableObject
{
    public TextAsset[] LevelAssets;
}
