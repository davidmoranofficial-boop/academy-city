using UnityEngine;
using UnityEngine.Rendering;

namespace AcademyCity
{
    /// <summary>
    /// Test playground: press Play in any scene and you get a white marble floor, Hanson and a follow camera.
    /// Everything is built from code, so there is no scene file to keep in sync.
    /// </summary>
    public static class Playground
    {
        public const float FloorSize = 80f;      // metres per side
        const float SlabTile = 2f;               // the marble texture covers 2 x 2 m (four 1 m slabs)

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (GameObject.Find("Hanson") != null) return;

            SetUpLight();
            MakeFloor();
            var hanson = HansonFactory.Spawn(new Vector3(0, 0, 0));

            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.nearClipPlane = 0.1f;
            var follow = cam.GetComponent<FollowCamera>();
            if (follow == null) follow = cam.gameObject.AddComponent<FollowCamera>();
            follow.target = hanson.transform;
            hanson.GetComponent<PlayerController>().cameraTransform = cam.transform;
        }

        static void SetUpLight()
        {
            Light sun = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null) sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.9f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.78f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.75f, 0.73f, 0.72f);
            RenderSettings.ambientGroundColor = new Color(0.55f, 0.53f, 0.52f);
        }

        static void MakeFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane); // 10 x 10 m, with a collider
            floor.name = "Marble Floor";
            floor.transform.localScale = new Vector3(FloorSize / 10f, 1f, FloorSize / 10f);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { name = "Marble" };
            var tex = Resources.Load<Texture2D>("Environment/T_Marble_Floor");
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", Vector2.one * (FloorSize / SlabTile));
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.78f);
            mat.SetFloat("_Metallic", 0f);
            floor.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
