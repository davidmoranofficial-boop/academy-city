using UnityEngine;

namespace AcademyCity
{
    /// <summary>Builds the playable Hanson: the model, toon materials, a CharacterController and the animation player.</summary>
    public static class HansonFactory
    {
        // Model material name (from the FBX) -> texture in Resources/Characters/Hanson/Textures
        static readonly (string mat, string tex)[] Skins =
        {
            ("MI_Peasant", "T_Hanson_Outfit"),
            ("MI_Regular_Male", "T_Hanson_Skin_Body"),
            ("MI_Superhero_Male", "T_Hanson_Skin_Head"),
            ("MI_Eyes", "T_Hanson_Eyes"),
            ("MI_Hair", "T_Hanson_Hair"),
        };

        public const float HeadScale = 1.08f; // a slightly bigger head reads younger and more anime

        public static GameObject Spawn(Vector3 at)
        {
            var root = new GameObject("Hanson");
            root.transform.position = at;

            var prefab = Resources.Load<GameObject>("Characters/Hanson/Hanson");
            if (prefab == null) { Debug.LogError("[Academy City] Hanson model not found in Resources/Characters/Hanson."); return root; }
            var model = Object.Instantiate(prefab, root.transform);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            ApplyToon(model);

            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var head = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            if (head != null) head.localScale = Vector3.one * HeadScale;
            else Debug.LogWarning("[Academy City] Hanson is not set up as Humanoid yet; reimport Assets/AcademyCity/Resources/Characters.");

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.28f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.skinWidth = 0.03f;

            root.AddComponent<CharacterAnimation>().animator = animator;
            root.AddComponent<PlayerController>();
            return root;
        }

        static void ApplyToon(GameObject model)
        {
            var shader = Resources.Load<Shader>("Shaders/AcademyToon");
            if (shader == null) shader = Shader.Find("AcademyCity/Toon");
            var cache = new System.Collections.Generic.Dictionary<string, Material>();
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    string name = mats[i] != null ? mats[i].name : "";
                    string tex = null;
                    foreach (var s in Skins) if (name.StartsWith(s.mat)) { tex = s.tex; break; }
                    if (tex == null) { Debug.LogWarning("[Academy City] No toon texture for material " + name); continue; }
                    if (!cache.TryGetValue(tex, out var m))
                    {
                        m = new Material(shader) { name = "Toon " + tex };
                        m.SetTexture("_BaseMap", Resources.Load<Texture2D>("Characters/Hanson/Textures/" + tex));
                        if (tex == "T_Hanson_Eyes") m.SetFloat("_OutlineWidth", 0f);
                        cache[tex] = m;
                    }
                    mats[i] = m;
                }
                r.sharedMaterials = mats;
                if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = true;
            }
        }
    }
}
