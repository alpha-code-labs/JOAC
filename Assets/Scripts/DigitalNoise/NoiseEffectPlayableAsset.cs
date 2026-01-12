// NoiseEffectPlayableAsset.cs
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;

[System.Serializable]
public class NoiseEffectPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    [Header("Noise Settings")]
    public float intensity = 1f;
    public float scale = 0.1f;
    public float speed = 1f;
    public Vector2 scrollDirection = Vector2.right;

    [Header("Visual Settings")]
    public Color noiseColor = Color.white;
    public NoiseBlendMode blendMode = NoiseBlendMode.Overlay;

    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<NoiseEffectPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.intensity = intensity;
        behaviour.scale = scale;
        behaviour.speed = speed;
        behaviour.scrollDirection = scrollDirection;
        behaviour.noiseColor = noiseColor;
        behaviour.blendMode = blendMode;

        return playable;
    }
}

// Custom enum for blend modes
[System.Serializable]
public enum NoiseBlendMode
{
    Additive,
    Multiply,
    Overlay,
    Screen,
    Alpha
}