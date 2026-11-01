using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace AcademyCity
{
    /// <summary>
    /// Plays the Quaternius animation library on any Humanoid body without an Animator Controller asset:
    /// idle, walk, jog and sprint are blended by the current speed.
    /// </summary>
    public class CharacterAnimation : MonoBehaviour
    {
        public Animator animator;
        [Tooltip("Ground speed in m/s; set by the controller.")]
        public float speed;

        // Clip name and the ground speed (m/s) it was animated for.
        static readonly (string clip, float speed)[] Gaits =
        {
            ("Idle_Loop", 0f),
            ("Walk_Loop", 1.7f),
            ("Jog_Fwd_Loop", 4.0f),
            ("Sprint_Loop", 6.5f),
        };

        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        AnimationClipPlayable[] players;
        float smoothSpeed;

        void Start()
        {
            var all = Resources.LoadAll<AnimationClip>("Animations/UAL1_Standard");
            graph = PlayableGraph.Create("Hanson animation");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            mixer = AnimationMixerPlayable.Create(graph, Gaits.Length);
            output.SetSourcePlayable(mixer);
            players = new AnimationClipPlayable[Gaits.Length];
            for (int i = 0; i < Gaits.Length; i++)
            {
                var clip = Find(all, Gaits[i].clip);
                if (clip == null) { Debug.LogWarning("[Academy City] Missing animation " + Gaits[i].clip); continue; }
                players[i] = AnimationClipPlayable.Create(graph, clip);
                players[i].SetApplyFootIK(true);
                graph.Connect(players[i], 0, mixer, i);
            }
            mixer.SetInputWeight(0, 1f);
            graph.Play();
        }

        static AnimationClip Find(AnimationClip[] all, string name)
        {
            foreach (var c in all) if (c.name == name) return c;
            foreach (var c in all) if (c.name.EndsWith(name) && !c.name.StartsWith("__preview__")) return c;
            return null;
        }

        void Update()
        {
            if (!graph.IsValid()) return;
            smoothSpeed = Mathf.Lerp(smoothSpeed, speed, 1f - Mathf.Exp(-10f * Time.deltaTime));
            float s = smoothSpeed;
            // Find the two gaits around the current speed and cross-fade between them.
            int hi = 1;
            while (hi < Gaits.Length - 1 && s > Gaits[hi].speed) hi++;
            int lo = hi - 1;
            float t = Mathf.InverseLerp(Gaits[lo].speed, Gaits[hi].speed, s);
            for (int i = 0; i < Gaits.Length; i++)
                mixer.SetInputWeight(i, i == lo ? 1f - t : i == hi ? t : 0f);
            // Play moving clips a bit faster or slower so the feet match the ground speed.
            for (int i = 1; i < Gaits.Length; i++)
                if (players[i].IsValid())
                    players[i].SetSpeed(s < 0.05f ? 1f : Mathf.Clamp(s / Mathf.Lerp(Gaits[lo].speed, Gaits[hi].speed, t), 0.6f, 1.5f));
        }

        void OnDestroy()
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }
}
