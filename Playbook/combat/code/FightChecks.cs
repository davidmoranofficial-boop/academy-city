// FightChecks: automatic "eyes" for sim fights (see Playbook/combat/06-auto-checks.md).
// Drop into Assets/HansonDrafts/Demo/Scripts/. Add one FightChecks component to the battle scene,
// call Register() for each fighter, and call the On... hooks from the battle code.
// At the end of a fight call WriteReport(); it writes Demo/Checks/report_<time>.txt.
// Written blind (no editor access); expect small fixes on first compile.
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HansonDrafts.Demo
{
    public enum StrikeSurface { Fist, Shin, Foot, Knee, Elbow, Palm }
    public enum TargetZone { Head, Body, Legs }

    public class FightChecks : MonoBehaviour
    {
        [Header("Thresholds (tune against real clips)")]
        public float maxFacingAngle = 35f;      // check 1
        public float plantHeight = 0.03f;        // check 2: foot height to count as planted (m above floor)
        public float plantSpeed = 0.2f;          // check 2: foot speed to count as planted (m/s)
        public float maxPlantDrift = 0.02f;      // check 2
        public float airborneHeight = 0.05f;     // check 3
        public float maxContactGap = 0.12f;      // check 4: whiff shown as a hit
        public float maxPenetration = 0.08f;     // check 4: clipping into the body
        public float maxStepMismatch = 0.25f;    // check 6
        public float floorY = 0f;

        public Camera exchangeCamera;            // check 10

        class Fighter
        {
            public string name;
            public Animator anim;
            public Transform hips, lFoot, rFoot, lHand, rHand, lShin, rShin, head, chest, lThigh, rThigh, lKnee, rKnee;
            public bool lPlanted, rPlanted;
            public Vector3 lPlantPos, rPlantPos, lPrev, rPrev;
            public bool inKick, inStrike, airborneAllowed;
            public float stepSum; public Vector3 moveStartRoot; public bool moving;
        }

        readonly Dictionary<Animator, Fighter> fighters = new Dictionary<Animator, Fighter>();
        readonly List<string> fails = new List<string>();
        readonly Dictionary<string, int> failCounts = new Dictionary<string, int>();
        int strikes, contacts;

        // ---------- setup ----------
        public void Register(Animator anim, string displayName)
        {
            if (anim == null || !anim.isHuman) { Debug.LogWarning("FightChecks: needs a humanoid Animator"); return; }
            var f = new Fighter
            {
                name = displayName, anim = anim,
                hips = anim.GetBoneTransform(HumanBodyBones.Hips),
                lFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot),
                rFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot),
                lHand = anim.GetBoneTransform(HumanBodyBones.LeftHand),
                rHand = anim.GetBoneTransform(HumanBodyBones.RightHand),
                lShin = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg),
                rShin = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg),
                lThigh = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg),
                rThigh = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg),
                head = anim.GetBoneTransform(HumanBodyBones.Head),
                chest = anim.GetBoneTransform(HumanBodyBones.Chest) ?? anim.GetBoneTransform(HumanBodyBones.Spine),
            };
            f.lKnee = f.lShin; f.rKnee = f.rShin;
            f.lPrev = f.lFoot.position; f.rPrev = f.rFoot.position;
            fighters[anim] = f;
        }

        // ---------- hooks from battle code ----------
        /// Call when a strike's active phase starts (after face/step/plant).
        public void OnStrikeStart(Animator attacker, Animator target, string move, bool isKick)
        {
            if (!Get(attacker, out var a) || !Get(target, out var t)) return;
            strikes++;
            a.inStrike = true; a.inKick = isKick;
            Vector3 fwd = Flat(a.hips.forward);
            Vector3 toT = Flat(t.hips.position - a.hips.position);
            float ang = Vector3.Angle(fwd, toT);
            if (ang > maxFacingAngle) Fail("1 facing", $"{a.name} {move}: strike started {ang:0} deg off target");
        }

        /// Call on the contact frame of a strike that the game counts as landing (hit or blocked).
        public void OnContact(Animator attacker, Animator target, string move, StrikeSurface surface, TargetZone zone, bool kickLeftLeg = false, bool punchLeftHand = false)
        {
            if (!Get(attacker, out var a) || !Get(target, out var t)) return;
            contacts++;
            Vector3 hitPoint = SurfacePoint(a, surface, kickLeftLeg, punchLeftHand);
            Vector3 zonePoint = ZonePoint(t, zone, hitPoint);
            float gap = Vector3.Distance(hitPoint, zonePoint);
            if (gap > maxContactGap) Fail("4 contact", $"{a.name} {move}: limb {gap * 100:0} cm from {zone}");

            // Penetration: striking point well inside the target's body capsule.
            float axisDist = DistToSegment(hitPoint, t.hips.position, t.head.position);
            if (axisDist < 0.15f - maxPenetration) Fail("4 clipping", $"{a.name} {move}: limb inside {t.name}'s body");

            if (surface == StrikeSurface.Shin && zone == TargetZone.Legs)
            {
                Transform foot = kickLeftLeg ? a.lFoot : a.rFoot;
                if (Vector3.Distance(foot.position, zonePoint) < Vector3.Distance(hitPoint, zonePoint))
                    Fail("5 low kick surface", $"{a.name} {move}: foot, not shin, landed");
                // Should land above the knee on the thigh.
                Transform tk = NearestKnee(t, hitPoint);
                if (hitPoint.y < tk.position.y) Fail("5 low kick height", $"{a.name} {move}: landed below the knee");
            }
        }

        public void OnStrikeEnd(Animator attacker) { if (Get(attacker, out var a)) { a.inStrike = false; a.inKick = false; } }

        /// Call when a defence pose starts, with the attacker's planned contact time (Time.time).
        public void OnDefenceStart(Animator defender, string defence, float attackerContactTime)
        {
            if (!Get(defender, out var d)) return;
            if (Time.time > attackerContactTime + 0.001f) Fail("8 defence timing", $"{d.name} {defence}: started after the hit landed");
        }

        /// Call after hit stop on a landed hit, passing whether each fighter was frozen.
        public void OnHitStop(bool attackerFrozen, bool targetFrozen, string move)
        {
            if (!attackerFrozen || !targetFrozen) Fail("9 hit stop", $"{move}: only {(attackerFrozen ? "attacker" : targetFrozen ? "target" : "nobody")} froze");
        }

        /// Wrap footwork moves so steps can be compared to distance covered.
        public void OnMoveStart(Animator who) { if (Get(who, out var f)) { f.moving = true; f.stepSum = 0; f.moveStartRoot = Flat(f.hips.position); } }
        public void OnMoveEnd(Animator who)
        {
            if (!Get(who, out var f) || !f.moving) return;
            f.moving = false;
            float rootDist = Vector3.Distance(Flat(f.hips.position), f.moveStartRoot);
            if (rootDist < 0.1f) return;
            // In a step-drag both feet each travel about the root distance.
            float expected = rootDist * 2f;
            float mismatch = Mathf.Abs(f.stepSum - expected) / expected;
            if (mismatch > maxStepMismatch) Fail("6 steps vs movement", $"{f.name}: moved {rootDist:0.00} m, feet stepped {f.stepSum:0.00} m");
        }

        /// Fight result, for check 12 across many sims (appended to a CSV).
        public void OnFightEnd(string winner, int exchanges)
        {
            string dir = Path.Combine(Application.dataPath, "HansonDrafts/Demo/Checks");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "fight_results.csv"), $"{System.DateTime.Now:s},{winner},{exchanges}\n");
        }

        // ---------- per-frame checks ----------
        void LateUpdate()
        {
            foreach (var f in fighters.Values)
            {
                FootCheck(f, f.lFoot, ref f.lPlanted, ref f.lPlantPos, ref f.lPrev, "left");
                FootCheck(f, f.rFoot, ref f.rPlanted, ref f.rPlantPos, ref f.rPrev, "right");

                bool lAir = f.lFoot.position.y - floorY > airborneHeight;
                bool rAir = f.rFoot.position.y - floorY > airborneHeight;
                if (f.inKick && lAir && rAir) Fail("3 one kicking leg", $"{f.name}: both feet off the ground during a kick");

                // Check 7: feet crossing (lateral order flips relative to the hips' right vector).
                Vector3 right = Flat(f.hips.right);
                float side = Vector3.Dot(Flat(f.rFoot.position - f.lFoot.position), right);
                if (side < -0.05f && !f.inKick) Fail("7 feet crossing", $"{f.name}: feet crossed");

                if (exchangeCamera != null && exchangeCamera.isActiveAndEnabled)
                {
                    foreach (var b in new[] { f.head, f.lFoot, f.rFoot })
                    {
                        Vector3 v = exchangeCamera.WorldToViewportPoint(b.position);
                        if (v.z < 0 || v.x < 0 || v.x > 1 || v.y < 0 || v.y > 1) { Fail("10 framing", $"{f.name}: {b.name} off-screen"); break; }
                    }
                }
            }
        }

        void FootCheck(Fighter f, Transform foot, ref bool planted, ref Vector3 plantPos, ref Vector3 prev, string side)
        {
            Vector3 p = foot.position;
            float speed = Time.deltaTime > 0 ? Vector3.Distance(Flat(p), Flat(prev)) / Time.deltaTime : 0;
            if (f.moving) f.stepSum += Vector3.Distance(Flat(p), Flat(prev));
            bool low = p.y - floorY < plantHeight;
            if (!planted && low && speed < plantSpeed) { planted = true; plantPos = p; }
            else if (planted && !low) planted = false;
            else if (planted)
            {
                float drift = Vector3.Distance(Flat(p), Flat(plantPos));
                if (drift > maxPlantDrift && speed < plantSpeed * 3f)
                { Fail("2 foot sliding", $"{f.name} {side} foot slid {drift * 100:0} cm"); plantPos = p; }
                else if (drift > maxPlantDrift) planted = false; // a real step started
            }
            prev = p;
        }

        // ---------- report ----------
        public string WriteReport(string label = "fight")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"FightChecks report: {label}  {System.DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Strikes: {strikes}, contacts: {contacts}, failures: {fails.Count}");
            sb.AppendLine("By check:");
            foreach (var kv in failCounts) sb.AppendLine($"  {kv.Key}: {kv.Value}");
            sb.AppendLine("Details (first 200):");
            for (int i = 0; i < fails.Count && i < 200; i++) sb.AppendLine("  " + fails[i]);
            string dir = Path.Combine(Application.dataPath, "HansonDrafts/Demo/Checks");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"report_{label}_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"FightChecks: {fails.Count} failures, report at {path}");
            return path;
        }

        public int FailureCount => fails.Count;

        // ---------- helpers ----------
        void Fail(string check, string detail)
        {
            string line = $"[{Time.time:0.00}s] {check}: {detail}";
            // Avoid flooding: skip identical consecutive lines.
            if (fails.Count > 0 && fails[fails.Count - 1].EndsWith(detail)) return;
            fails.Add(line);
            failCounts.TryGetValue(check, out int n); failCounts[check] = n + 1;
        }

        bool Get(Animator a, out Fighter f) => fighters.TryGetValue(a, out f) && f != null;
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        static Vector3 SurfacePoint(Fighter a, StrikeSurface s, bool leftLeg, bool leftHand)
        {
            switch (s)
            {
                case StrikeSurface.Shin:
                    var shin = leftLeg ? a.lShin : a.rShin; var ft = leftLeg ? a.lFoot : a.rFoot;
                    return Vector3.Lerp(shin.position, ft.position, 0.5f);
                case StrikeSurface.Foot: return (leftLeg ? a.lFoot : a.rFoot).position;
                case StrikeSurface.Knee: return (leftLeg ? a.lShin : a.rShin).position;
                default: return (leftHand ? a.lHand : a.rHand).position;
            }
        }

        static Vector3 ZonePoint(Fighter t, TargetZone z, Vector3 from)
        {
            switch (z)
            {
                case TargetZone.Head: return t.head.position;
                case TargetZone.Body: return t.chest.position;
                default:
                    // Thigh just above the knee on the nearer leg.
                    var thigh = Vector3.Distance(from, t.lThigh.position) < Vector3.Distance(from, t.rThigh.position) ? t.lThigh : t.rThigh;
                    var knee = thigh == t.lThigh ? t.lShin : t.rShin;
                    return Vector3.Lerp(knee.position, thigh.position, 0.3f);
            }
        }

        static Transform NearestKnee(Fighter t, Vector3 p) =>
            Vector3.Distance(p, t.lShin.position) < Vector3.Distance(p, t.rShin.position) ? t.lShin : t.rShin;

        static float DistToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a; float tt = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector3.Distance(p, a + ab * tt);
        }
    }
}
