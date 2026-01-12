// NoiseEffectTrack.cs - Updated to support both Image and Renderer
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[TrackColor(0.5f, 0.8f, 0.2f)]
[TrackClipType(typeof(NoiseEffectPlayableAsset))]
[TrackBindingType(typeof(Component))] // More flexible binding
public class NoiseEffectTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<NoiseEffectMixerBehaviour>.Create(graph, inputCount);
    }

    // Validate that the bound component is supported
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        var binding = director.GetGenericBinding(this);
        if (binding != null)
        {
            if (binding is Image || binding is Renderer)
            {
                // Valid binding
            }
            else
            {
                Debug.LogWarning($"NoiseEffectTrack: Unsupported component type {binding.GetType()}. Use Image or Renderer components.");
            }
        }
    }
}