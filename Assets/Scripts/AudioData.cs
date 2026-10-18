using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "SO/AudioData")]
public sealed class AudioData : ScriptableObject
{
    public AudioClip Clip;
    public Vector2 PitchRange =  new(1, 1);
    public Vector2 VolumeRange =  new(1, 1);

    public void Apply(AudioSource source)
    {
        source.clip = Clip;
        source.pitch = Random.Range(PitchRange.x, PitchRange.y);
        source.volume = Random.Range(VolumeRange.x, VolumeRange.y);
    }
}
