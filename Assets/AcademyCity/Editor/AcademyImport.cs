#if UNITY_EDITOR
using UnityEditor;

namespace AcademyCity.EditorTools
{
    /// <summary>
    /// Import settings for characters and animations, so nobody has to click through the inspector.
    /// Characters and the animation library are set up as Humanoid (Unity retargets the animations onto any body),
    /// with "Bake Axis Conversion" on, and every clip whose name ends in _Loop is set to loop.
    /// </summary>
    public class AcademyImport : AssetPostprocessor
    {
        const string Characters = "Assets/AcademyCity/Resources/Characters/";
        const string Animations = "Assets/AcademyCity/Resources/Animations/";

        bool IsCharacter => assetPath.StartsWith(Characters);
        bool IsAnimation => assetPath.StartsWith(Animations);

        void OnPreprocessModel()
        {
            if (!IsCharacter && !IsAnimation) return;
            var mi = (ModelImporter)assetImporter;
            mi.bakeAxisConversion = true;
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            mi.importAnimation = IsAnimation;
            mi.importBlendShapes = true;
        }

        void OnPreprocessAnimation()
        {
            if (!IsAnimation) return;
            var mi = (ModelImporter)assetImporter;
            var clips = mi.defaultClipAnimations;
            foreach (var c in clips)
            {
                bool loop = c.name.EndsWith("_Loop");
                c.loopTime = loop;
                c.lockRootRotation = true;
                c.lockRootHeightY = true;
                c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = true;
                c.keepOriginalPositionY = true;
                c.keepOriginalPositionXZ = true;
            }
            mi.clipAnimations = clips;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/AcademyCity/Resources/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.mipmapEnabled = true;
            ti.anisoLevel = 8;
            if (assetPath.Contains("/Environment/")) ti.wrapMode = UnityEngine.TextureWrapMode.Repeat;
        }
    }
}
#endif
